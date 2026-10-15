const DATABASE_FILE = /^(CURRENT|MANIFEST-\d+|\d+\.(log|ldb|sst))$/i;

function split(path) {
    const parts = String(path).replace(/\\/g, "/").split("/").filter(Boolean);
    const name = parts.pop() || "";

    return { folder: parts.join("/"), name };
}

function rank(folder) {
    const last = (folder.split("/").pop() || "").toLowerCase();

    if (last === "kvs") return 0;
    if (last === "kvs_cl2") return 2;

    return 1;
}

export function pickDatabase(paths) {
    const folders = new Map();

    for (const path of paths) {
        const { folder, name } = split(path);

        if (!DATABASE_FILE.test(name)) {
            continue;
        }

        if (!folders.has(folder)) {
            folders.set(folder, []);
        }

        folders.get(folder).push({ path, name });
    }

    const usable = [...folders.entries()]
        .filter(([, files]) => files.some(file => file.name === "CURRENT" || /\.(ldb|sst|log)$/i.test(file.name)))
        .sort(([left], [right]) => rank(left) - rank(right) || left.length - right.length);

    if (usable.length === 0) {
        return null;
    }

    const [folder, files] = usable[0];

    return { folder, files };
}

async function inflate(bytes) {
    const stream = new Blob([bytes]).stream().pipeThrough(new DecompressionStream("deflate-raw"));

    return new Uint8Array(await new Response(stream).arrayBuffer());
}

function zip64Sizes(view, extraStart, extraLength, sizes) {
    let at = extraStart;

    while (at + 4 <= extraStart + extraLength) {
        const id = view.getUint16(at, true);
        const length = view.getUint16(at + 2, true);

        if (id === 1) {
            let field = at + 4;

            for (const name of ["uncompressed", "compressed", "offset"]) {
                if (sizes[name] === 0xffffffff) {
                    sizes[name] = Number(view.getBigUint64(field, true));
                    field += 8;
                }
            }
        }

        at += 4 + length;
    }

    return sizes;
}

export function listZip(bytes) {
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    let end = -1;

    for (let at = bytes.length - 22; at >= Math.max(0, bytes.length - 22 - 65535); at--) {
        if (view.getUint32(at, true) === 0x06054b50) {
            end = at;
            break;
        }
    }

    if (end < 0) {
        throw new Error("not-zip");
    }

    let count = view.getUint16(end + 10, true);
    let directory = view.getUint32(end + 16, true);

    if ((count === 0xffff || directory === 0xffffffff) && end >= 20 && view.getUint32(end - 20, true) === 0x07064b50) {
        const record = Number(view.getBigUint64(end - 12, true));

        count = Number(view.getBigUint64(record + 32, true));
        directory = Number(view.getBigUint64(record + 48, true));
    }

    const decoder = new TextDecoder();
    const entries = [];
    let at = directory;

    for (let index = 0; index < count; index++) {
        if (view.getUint32(at, true) !== 0x02014b50) {
            throw new Error("broken-zip");
        }

        const method = view.getUint16(at + 10, true);
        const nameLength = view.getUint16(at + 28, true);
        const extraLength = view.getUint16(at + 30, true);
        const commentLength = view.getUint16(at + 32, true);
        const name = decoder.decode(bytes.subarray(at + 46, at + 46 + nameLength));
        const sizes = zip64Sizes(view, at + 46 + nameLength, extraLength, {
            compressed: view.getUint32(at + 20, true),
            uncompressed: view.getUint32(at + 24, true),
            offset: view.getUint32(at + 42, true),
        });

        entries.push({ name, method, ...sizes });
        at += 46 + nameLength + extraLength + commentLength;
    }

    return entries;
}

export async function readZipEntry(bytes, entry) {
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    const local = entry.offset;
    const start = local + 30 + view.getUint16(local + 26, true) + view.getUint16(local + 28, true);
    const data = bytes.subarray(start, start + entry.compressed);

    if (entry.method === 0) {
        return data;
    }

    if (entry.method === 8) {
        return inflate(data);
    }

    throw new Error("zip-method");
}

export async function fromZip(file) {
    const bytes = new Uint8Array(await file.arrayBuffer());
    const entries = listZip(bytes).filter(entry => !entry.name.endsWith("/"));
    const picked = pickDatabase(entries.map(entry => entry.name));

    if (!picked) {
        return null;
    }

    const wanted = new Set(picked.files.map(file => file.path));
    const files = [];

    for (const entry of entries) {
        if (wanted.has(entry.name)) {
            files.push({ name: split(entry.name).name, bytes: await readZipEntry(bytes, entry) });
        }
    }

    return { folder: picked.folder, files };
}

export async function fromFiles(list) {
    const files = [...list];
    const zip = files.find(file => /\.zip$/i.test(file.name));

    if (zip && files.length === 1) {
        return fromZip(zip);
    }

    const byPath = new Map(files.map(file => [file.webkitRelativePath || file.relativePath || file.name, file]));
    const picked = pickDatabase([...byPath.keys()]);

    if (!picked) {
        return zip ? fromZip(zip) : null;
    }

    const read = [];

    for (const entry of picked.files) {
        read.push({ name: entry.name, bytes: new Uint8Array(await byPath.get(entry.path).arrayBuffer()) });
    }

    return { folder: picked.folder, files: read };
}

function readAll(reader) {
    return new Promise((resolve, reject) => {
        const all = [];

        const next = () => reader.readEntries(batch => {
            if (batch.length === 0) {
                resolve(all);
            } else {
                all.push(...batch);
                next();
            }
        }, reject);

        next();
    });
}

async function walk(entry, prefix, out) {
    if (entry.isFile) {
        const file = await new Promise((resolve, reject) => entry.file(resolve, reject));

        file.relativePath = prefix + entry.name;
        out.push(file);

        return;
    }

    if (entry.isDirectory) {
        for (const child of await readAll(entry.createReader())) {
            await walk(child, prefix + entry.name + "/", out);
        }
    }
}

export async function fromDrop(transfer) {
    const items = [...(transfer.items || [])];
    const entries = items.map(item => (item.webkitGetAsEntry ? item.webkitGetAsEntry() : null)).filter(Boolean);

    if (entries.length === 0) {
        return fromFiles(transfer.files || []);
    }

    const files = [];

    for (const entry of entries) {
        await walk(entry, "", files);
    }

    return fromFiles(files);
}

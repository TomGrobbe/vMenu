const LOG_BLOCK = 32768;
const TABLE_MAGIC_LOW = 0x8b80fb57;
const TABLE_MAGIC_HIGH = 0xdb477524;

class Reader {
    constructor(bytes, at = 0, end = bytes.length) {
        this.bytes = bytes;
        this.at = at;
        this.end = end;
    }

    get done() {
        return this.at >= this.end;
    }

    byte() {
        if (this.at >= this.end) {
            throw new Error("Unexpected end of data.");
        }

        return this.bytes[this.at++];
    }

    varint() {
        let result = 0;
        let scale = 1;

        for (let step = 0; step < 10; step++) {
            const next = this.byte();

            result += (next & 0x7f) * scale;

            if ((next & 0x80) === 0) {
                return result;
            }

            scale *= 128;
        }

        throw new Error("Broken varint.");
    }

    take(length) {
        if (this.at + length > this.end) {
            throw new Error("Unexpected end of data.");
        }

        const slice = this.bytes.subarray(this.at, this.at + length);

        this.at += length;

        return slice;
    }

    slice() {
        return this.take(this.varint());
    }

    fixed32() {
        const value = this.bytes[this.at] | (this.bytes[this.at + 1] << 8) | (this.bytes[this.at + 2] << 16) | (this.bytes[this.at + 3] << 24);

        this.take(4);

        return value >>> 0;
    }

    fixed64() {
        const low = this.fixed32();
        const high = this.fixed32();

        return high * 0x100000000 + low;
    }
}

export function unsnappy(input) {
    const reader = new Reader(input);
    const output = new Uint8Array(reader.varint());
    let written = 0;

    while (!reader.done) {
        const tag = reader.byte();
        const kind = tag & 3;

        if (kind === 0) {
            let length = tag >> 2;

            if (length >= 60) {
                const extra = length - 59;

                length = 0;

                for (let index = 0; index < extra; index++) {
                    length |= reader.byte() << (8 * index);
                }
            }

            length += 1;
            output.set(reader.take(length), written);
            written += length;

            continue;
        }

        let length;
        let offset;

        if (kind === 1) {
            length = ((tag >> 2) & 7) + 4;
            offset = ((tag >> 5) << 8) | reader.byte();
        } else if (kind === 2) {
            length = (tag >> 2) + 1;
            offset = reader.byte() | (reader.byte() << 8);
        } else {
            length = (tag >> 2) + 1;
            offset = reader.fixed32();
        }

        if (offset === 0 || offset > written) {
            throw new Error("Broken snappy copy.");
        }

        for (let index = 0; index < length; index++) {
            output[written] = output[written - offset];
            written++;
        }
    }

    return output.subarray(0, written);
}

function logRecords(bytes) {
    const records = [];
    let pending = null;

    for (let blockStart = 0; blockStart < bytes.length; blockStart += LOG_BLOCK) {
        const blockEnd = Math.min(blockStart + LOG_BLOCK, bytes.length);
        let at = blockStart;

        while (at + 7 <= blockEnd) {
            const length = bytes[at + 4] | (bytes[at + 5] << 8);
            const type = bytes[at + 6];

            if (type === 0 && length === 0) {
                break;
            }

            const start = at + 7;
            const stop = start + length;

            if (stop > blockEnd) {
                pending = null;
                break;
            }

            const data = bytes.subarray(start, stop);

            if (type === 1) {
                records.push(data);
                pending = null;
            } else if (type === 2) {
                pending = [data];
            } else if (type === 3 && pending) {
                pending.push(data);
            } else if (type === 4 && pending) {
                pending.push(data);
                records.push(join(pending));
                pending = null;
            } else {
                pending = null;
            }

            at = stop;
        }
    }

    return records;
}

function join(parts) {
    const total = parts.reduce((sum, part) => sum + part.length, 0);
    const joined = new Uint8Array(total);
    let at = 0;

    for (const part of parts) {
        joined.set(part, at);
        at += part.length;
    }

    return joined;
}

function readLog(bytes, found) {
    let damaged = 0;

    for (const record of logRecords(bytes)) {
        try {
            const reader = new Reader(record);
            let sequence = reader.fixed64();
            const count = reader.fixed32();

            for (let index = 0; index < count && !reader.done; index++, sequence++) {
                const tag = reader.byte();
                const key = reader.slice();
                const value = tag === 1 ? reader.slice() : null;

                remember(found, key, sequence, value);
            }
        } catch {
            damaged++;
        }
    }

    return damaged;
}

function blockAt(bytes, offset, size) {
    const data = bytes.subarray(offset, offset + size);
    const compression = bytes[offset + size];

    if (compression === 0) {
        return data;
    }

    if (compression === 1) {
        return unsnappy(data);
    }

    throw new Error("Unknown block compression " + compression + ".");
}

function blockEntries(block) {
    const restarts = new DataView(block.buffer, block.byteOffset + block.length - 4, 4).getUint32(0, true);
    const end = block.length - 4 - restarts * 4;
    const reader = new Reader(block, 0, end);
    const entries = [];
    let previous = new Uint8Array(0);

    while (!reader.done) {
        const shared = reader.varint();
        const unshared = reader.varint();
        const valueLength = reader.varint();
        const key = new Uint8Array(shared + unshared);

        key.set(previous.subarray(0, shared), 0);
        key.set(reader.take(unshared), shared);

        entries.push({ key, value: reader.take(valueLength) });
        previous = key;
    }

    return entries;
}

function readTable(bytes, found) {
    if (bytes.length < 48) {
        throw new Error("Table too small.");
    }

    const footer = new Reader(bytes, bytes.length - 48);
    const magic = new DataView(bytes.buffer, bytes.byteOffset + bytes.length - 8, 8);

    if (magic.getUint32(0, true) !== TABLE_MAGIC_LOW || magic.getUint32(4, true) !== TABLE_MAGIC_HIGH) {
        throw new Error("Not a table file.");
    }

    footer.varint();
    footer.varint();

    const indexOffset = footer.varint();
    const indexSize = footer.varint();

    for (const handle of blockEntries(blockAt(bytes, indexOffset, indexSize))) {
        const reader = new Reader(handle.value);
        const offset = reader.varint();
        const size = reader.varint();

        for (const entry of blockEntries(blockAt(bytes, offset, size))) {
            if (entry.key.length < 8) {
                continue;
            }

            const userKey = entry.key.subarray(0, entry.key.length - 8);
            const trailer = new Reader(entry.key, entry.key.length - 8);
            const packed = trailer.fixed64();
            const type = packed % 256;
            const sequence = Math.floor(packed / 256);

            remember(found, userKey, sequence, type === 1 ? entry.value : null);
        }
    }
}

const latin = new TextDecoder("latin1");

function remember(found, key, sequence, value) {
    const id = latin.decode(key);
    const known = found.get(id);

    if (!known || known.sequence < sequence) {
        found.set(id, { key, sequence, value: value ? value.slice() : null });
    }
}

function liveFiles(manifest) {
    const tables = new Set();
    let logNumber = 0;
    let previousLog = 0;

    for (const record of logRecords(manifest)) {
        const reader = new Reader(record);

        while (!reader.done) {
            const tag = reader.varint();

            if (tag === 1) {
                reader.slice();
            } else if (tag === 2) {
                logNumber = reader.varint();
            } else if (tag === 9) {
                previousLog = reader.varint();
            } else if (tag === 3 || tag === 4) {
                reader.varint();
            } else if (tag === 5) {
                reader.varint();
                reader.slice();
            } else if (tag === 6) {
                reader.varint();
                tables.delete(reader.varint());
            } else if (tag === 7) {
                reader.varint();
                tables.add(reader.varint());
                reader.varint();
                reader.slice();
                reader.slice();
            } else {
                throw new Error("Unknown manifest tag " + tag + ".");
            }
        }
    }

    return { tables, logNumber, previousLog };
}

function numberOf(name) {
    const match = /^(\d+)\.(log|ldb|sst)$/i.exec(name);

    return match ? Number(match[1]) : null;
}

export function readDatabase(files) {
    const byName = new Map(files.map(file => [file.name, file.bytes]));
    const problems = [];
    let live = null;

    const current = byName.get("CURRENT");

    if (current) {
        const manifestName = new TextDecoder().decode(current).trim();
        const manifest = byName.get(manifestName);

        if (manifest) {
            try {
                live = liveFiles(manifest);
            } catch (problem) {
                problems.push("The MANIFEST file could not be read, so every file was used. " + problem.message);
            }
        }
    }

    const found = new Map();
    const names = [...byName.keys()].filter(name => numberOf(name) !== null).sort((left, right) => numberOf(left) - numberOf(right));

    for (const name of names) {
        const number = numberOf(name);
        const isLog = /\.log$/i.test(name);

        if (live) {
            if (isLog && number < live.logNumber && number !== live.previousLog) {
                continue;
            }

            if (!isLog && !live.tables.has(number)) {
                continue;
            }
        }

        try {
            if (isLog) {
                const damaged = readLog(byName.get(name), found);

                if (damaged > 0) {
                    problems.push(name + " has " + damaged + " damaged record(s).");
                }
            } else {
                readTable(byName.get(name), found);
            }
        } catch (problem) {
            problems.push(name + " could not be read. " + problem.message);
        }
    }

    const entries = new Map();
    const utf8 = new TextDecoder();

    for (const record of found.values()) {
        if (record.value) {
            entries.set(utf8.decode(record.key), record.value);
        }
    }

    return { entries, problems };
}

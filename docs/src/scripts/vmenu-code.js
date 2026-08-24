const GZIP_PREFIX = "VME1G:";
const PLAIN_PREFIX = "VME1P:";

export const CODE_PREFIXES = [GZIP_PREFIX, PLAIN_PREFIX];

function toBase64(bytes) {
    let binary = "";

    /* In slices, because spreading a whole profile into fromCharCode blows the stack. */
    for (let at = 0; at < bytes.length; at += 0x8000) {
        binary += String.fromCharCode.apply(null, bytes.subarray(at, at + 0x8000));
    }

    return btoa(binary);
}

function fromBase64(code) {
    const binary = atob(code);
    const bytes = new Uint8Array(binary.length);

    for (let at = 0; at < binary.length; at++) {
        bytes[at] = binary.charCodeAt(at);
    }

    return bytes;
}

async function through(bytes, transform) {
    const stream = new Blob([bytes]).stream().pipeThrough(transform);

    return new Uint8Array(await new Response(stream).arrayBuffer());
}

export async function encodeCode(plain) {
    const bytes = new TextEncoder().encode(plain);

    if (typeof CompressionStream === "function") {
        try {
            return GZIP_PREFIX + toBase64(await through(bytes, new CompressionStream("gzip")));
        } catch {
            /* Older browsers without the stream still get a code, just a much longer one. */
        }
    }

    return PLAIN_PREFIX + toBase64(bytes);
}

export async function decodeCode(code) {
    /* Stripped first: a code copied out of a chat message comes back wrapped in line breaks. */
    const cleaned = String(code || "").replace(/\s+/g, "");

    if (cleaned.length === 0) {
        return { error: "empty" };
    }

    const gzipped = cleaned.startsWith(GZIP_PREFIX);

    if (!gzipped && !cleaned.startsWith(PLAIN_PREFIX)) {
        return { error: "prefix" };
    }

    let bytes;

    try {
        /* Both prefixes are the same length, so one slice covers either. */
        bytes = fromBase64(cleaned.slice(GZIP_PREFIX.length));
    } catch {
        return { error: "base64" };
    }

    if (gzipped) {
        if (typeof DecompressionStream !== "function") {
            return { error: "gzip" };
        }

        try {
            bytes = await through(bytes, new DecompressionStream("gzip"));
        } catch {
            return { error: "gzip" };
        }
    }

    const plain = new TextDecoder().decode(bytes);

    return plain.length === 0 ? { error: "empty" } : { plain, cleaned };
}

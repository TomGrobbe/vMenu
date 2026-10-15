const utf8 = new TextDecoder();

export function unpack(bytes) {
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    let at = 0;

    function text(length) {
        const value = utf8.decode(bytes.subarray(at, at + length));

        at += length;

        return value;
    }

    function raw(length) {
        const value = bytes.slice(at, at + length);

        at += length;

        return value;
    }

    function list(length) {
        const items = [];

        for (let index = 0; index < length; index++) {
            items.push(next());
        }

        return items;
    }

    function map(length) {
        const object = {};

        for (let index = 0; index < length; index++) {
            const key = next();

            object[String(key)] = next();
        }

        return object;
    }

    function read(size, getter) {
        const value = view[getter](at);

        at += size;

        return value;
    }

    function next() {
        if (at >= bytes.length) {
            throw new Error("Unexpected end of MessagePack data.");
        }

        const tag = bytes[at++];

        if (tag <= 0x7f) return tag;
        if (tag >= 0xe0) return tag - 0x100;
        if ((tag & 0xe0) === 0xa0) return text(tag & 0x1f);
        if ((tag & 0xf0) === 0x90) return list(tag & 0x0f);
        if ((tag & 0xf0) === 0x80) return map(tag & 0x0f);

        switch (tag) {
            case 0xc0: return null;
            case 0xc2: return false;
            case 0xc3: return true;
            case 0xc4: return raw(read(1, "getUint8"));
            case 0xc5: return raw(read(2, "getUint16"));
            case 0xc6: return raw(read(4, "getUint32"));
            case 0xca: return read(4, "getFloat32");
            case 0xcb: return read(8, "getFloat64");
            case 0xcc: return read(1, "getUint8");
            case 0xcd: return read(2, "getUint16");
            case 0xce: return read(4, "getUint32");
            case 0xcf: return Number(read(8, "getBigUint64"));
            case 0xd0: return read(1, "getInt8");
            case 0xd1: return read(2, "getInt16");
            case 0xd2: return read(4, "getInt32");
            case 0xd3: return Number(read(8, "getBigInt64"));
            case 0xd9: return text(read(1, "getUint8"));
            case 0xda: return text(read(2, "getUint16"));
            case 0xdb: return text(read(4, "getUint32"));
            case 0xdc: return list(read(2, "getUint16"));
            case 0xdd: return list(read(4, "getUint32"));
            case 0xde: return map(read(2, "getUint16"));
            case 0xdf: return map(read(4, "getUint32"));
            default: throw new Error("Unsupported MessagePack tag 0x" + tag.toString(16) + ".");
        }
    }

    return next();
}

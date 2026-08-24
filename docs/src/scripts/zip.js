const CRC_TABLE = (() => {
    const table = new Uint32Array(256);

    for (let index = 0; index < 256; index++) {
        let value = index;

        for (let bit = 0; bit < 8; bit++) {
            value = value & 1 ? 0xedb88320 ^ (value >>> 1) : value >>> 1;
        }

        table[index] = value >>> 0;
    }

    return table;
})();

function crc32(bytes) {
    let crc = 0xffffffff;

    for (let at = 0; at < bytes.length; at++) {
        crc = CRC_TABLE[(crc ^ bytes[at]) & 0xff] ^ (crc >>> 8);
    }

    return (crc ^ 0xffffffff) >>> 0;
}

function dosTime(date) {
    return (date.getHours() << 11) | (date.getMinutes() << 5) | (date.getSeconds() >> 1);
}

function dosDate(date) {
    return ((date.getFullYear() - 1980) << 9) | ((date.getMonth() + 1) << 5) | date.getDate();
}

export function makeZip(files) {
    const encoder = new TextEncoder();
    const now = new Date();
    const time = dosTime(now);
    const date = dosDate(now);

    const bodies = [];
    const directory = [];
    let offset = 0;

    for (const file of files) {
        const name = encoder.encode(file.name);
        const body = encoder.encode(file.text);
        const crc = crc32(body);

        const local = new Uint8Array(30 + name.length);
        const localView = new DataView(local.buffer);

        localView.setUint32(0, 0x04034b50, true);
        localView.setUint16(4, 20, true);
        localView.setUint16(6, 0x0800, true);
        localView.setUint16(8, 0, true);
        localView.setUint16(10, time, true);
        localView.setUint16(12, date, true);
        localView.setUint32(14, crc, true);
        localView.setUint32(18, body.length, true);
        localView.setUint32(22, body.length, true);
        localView.setUint16(26, name.length, true);
        localView.setUint16(28, 0, true);
        local.set(name, 30);

        const record = new Uint8Array(46 + name.length);
        const recordView = new DataView(record.buffer);

        recordView.setUint32(0, 0x02014b50, true);
        recordView.setUint16(4, 20, true);
        recordView.setUint16(6, 20, true);
        recordView.setUint16(8, 0x0800, true);
        recordView.setUint16(10, 0, true);
        recordView.setUint16(12, time, true);
        recordView.setUint16(14, date, true);
        recordView.setUint32(16, crc, true);
        recordView.setUint32(20, body.length, true);
        recordView.setUint32(24, body.length, true);
        recordView.setUint16(28, name.length, true);
        recordView.setUint16(30, 0, true);
        recordView.setUint16(32, 0, true);
        recordView.setUint16(34, 0, true);
        recordView.setUint16(36, 0, true);
        recordView.setUint32(38, 0, true);
        recordView.setUint32(42, offset, true);
        record.set(name, 46);

        bodies.push(local, body);
        directory.push(record);

        offset += local.length + body.length;
    }

    const directorySize = directory.reduce((total, record) => total + record.length, 0);

    const end = new Uint8Array(22);
    const endView = new DataView(end.buffer);

    endView.setUint32(0, 0x06054b50, true);
    endView.setUint16(4, 0, true);
    endView.setUint16(6, 0, true);
    endView.setUint16(8, directory.length, true);
    endView.setUint16(10, directory.length, true);
    endView.setUint32(12, directorySize, true);
    endView.setUint32(16, offset, true);
    endView.setUint16(20, 0, true);

    return new Blob([...bodies, ...directory, end], { type: "application/zip" });
}

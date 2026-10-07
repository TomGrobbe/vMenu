import { GROUPS, OTHER_GROUP, matcherFor, nameOf, renameItem } from "./bundle.js";

export const ONLY_LEFT = "left";
export const ONLY_RIGHT = "right";
export const CHANGED = "changed";
export const SAME = "same";

const RENAMEABLE = new Set(GROUPS.filter(group => group.label !== "Settings").map(group => group.prefix));

const ORDER = [...GROUPS.map(group => group.label), OTHER_GROUP];

export function groupOf(key) {
    const group = matcherFor(key);

    return group ? group.label : OTHER_GROUP;
}

function valueOf(item) {
    return item.broken ? item.raw : JSON.parse(item.text);
}

function equal(left, right) {
    if (left === right) {
        return true;
    }

    if (typeof left !== "object" || typeof right !== "object" || left === null || right === null) {
        return false;
    }

    if (Array.isArray(left) !== Array.isArray(right)) {
        return false;
    }

    const leftKeys = Object.keys(left);
    const rightKeys = Object.keys(right);

    return leftKeys.length === rightKeys.length
        && leftKeys.every(key => Object.prototype.hasOwnProperty.call(right, key) && equal(left[key], right[key]));
}

export function sameItem(left, right) {
    return left.broken === right.broken
        && left.type === right.type
        && left.version === right.version
        && equal(valueOf(left), valueOf(right));
}

function flatten(value, path, out) {
    if (value !== null && typeof value === "object" && Object.keys(value).length > 0) {
        for (const key of Object.keys(value)) {
            flatten(value[key], Array.isArray(value) ? path + "[" + key + "]" : path ? path + "." + key : key, out);
        }
    } else {
        out.set(path || "(whole value)", JSON.stringify(value));
    }

    return out;
}

export function fieldChanges(left, right) {
    const changes = [];

    if (left.type !== right.type) {
        changes.push({ path: "(type)", left: left.type, right: right.type });
    }

    if (left.version !== right.version) {
        changes.push({ path: "(version)", left: String(left.version), right: String(right.version) });
    }

    if (left.broken || right.broken) {
        if (left.raw !== right.raw) {
            changes.push({ path: "(raw)", left: left.raw, right: right.raw });
        }

        return changes;
    }

    const leftFields = flatten(JSON.parse(left.text), "", new Map());
    const rightFields = flatten(JSON.parse(right.text), "", new Map());

    for (const [path, value] of leftFields) {
        if (!rightFields.has(path)) {
            changes.push({ path, left: value, right: undefined });
        } else if (rightFields.get(path) !== value) {
            changes.push({ path, left: value, right: rightFields.get(path) });
        }
    }

    for (const [path, value] of rightFields) {
        if (!leftFields.has(path)) {
            changes.push({ path, left: undefined, right: value });
        }
    }

    return changes;
}

export function canKeepBoth(row) {
    const group = matcherFor(row.key);

    return row.status === CHANGED && Boolean(group) && RENAMEABLE.has(group.prefix) && !row.left.broken && !row.right.broken;
}

export function compare(leftItems, rightItems) {
    const rows = new Map();

    for (const item of leftItems) {
        if (!rows.has(item.key)) {
            rows.set(item.key, { key: item.key, left: item, right: null });
        }
    }

    for (const item of rightItems) {
        const row = rows.get(item.key) || { key: item.key, left: null, right: null };

        if (!row.right) {
            row.right = item;
        }

        rows.set(item.key, row);
    }

    for (const row of rows.values()) {
        row.group = groupOf(row.key);
        row.name = nameOf(row.left || row.right);

        if (!row.right) {
            row.status = ONLY_LEFT;
            row.choice = "left";
        } else if (!row.left) {
            row.status = ONLY_RIGHT;
            row.choice = "right";
        } else if (sameItem(row.left, row.right)) {
            row.status = SAME;
            row.choice = "left";
        } else {
            row.status = CHANGED;
            row.choice = row.right.version > row.left.version ? "right" : "left";
            row.copyName = row.name + " (copy)";
            row.renameSide = "right";
        }
    }

    return [...rows.values()].sort((a, b) =>
        ORDER.indexOf(a.group) - ORDER.indexOf(b.group) || a.name.localeCompare(b.name));
}

export function olderKept(row) {
    if (row.status !== CHANGED || row.choice === "both") {
        return null;
    }

    const kept = row.choice === "left" ? row.left : row.right;
    const other = row.choice === "left" ? row.right : row.left;

    return kept.version < other.version ? { kept: kept.version, other: other.version } : null;
}

function copyItem(item) {
    return {
        ...item,
        entry: { ...item.entry },
        envelope: item.envelope ? { ...item.envelope } : null,
        removed: false,
        error: null,
    };
}

export function merge(rows) {
    const items = [];
    const copies = [];

    for (const row of rows) {
        if (row.choice === "both") {
            const renamed = row.renameSide === "left" ? row.left : row.right;

            items.push(copyItem(renamed === row.left ? row.right : row.left));
            copies.push({ row, item: copyItem(renamed) });
        } else if (row.choice === "left" || row.choice === "right") {
            items.push(copyItem(row[row.choice]));
        }
    }

    const errors = new Map();

    for (const { row, item } of copies) {
        items.push(item);

        const result = renameItem(items, item, row.copyName);

        if (result.error) {
            errors.set(row, result.error);
        }
    }

    return { items, errors };
}

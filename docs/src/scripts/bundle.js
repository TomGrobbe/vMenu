export const FORMAT = "vmenu.enhanced.kvp";
export const OWNED_PREFIX = "vmenu_";
export const OTHER_GROUP = "Everything else";

export const GROUPS = [
    { label: "Saved vehicles", prefix: "vmenu_vehicle_" },
    { label: "Vehicle categories", prefix: "vmenu_vehcategory_" },
    { label: "Saved peds", prefix: "vmenu_ped_" },
    { label: "Ped categories", prefix: "vmenu_pedcategory_" },
    { label: "Custom characters", prefix: "vmenu_mpchar_" },
    { label: "Character categories", prefix: "vmenu_mpcharcategory_" },
    { label: "Weapon loadouts", prefix: "vmenu_weaponloadout_" },
    { label: "Settings", prefix: "vmenu_default_" },
];

/* Longest first, so a key never lands in a group whose prefix is merely the start of another. */
const MATCHERS = [...GROUPS].sort((left, right) => right.prefix.length - left.prefix.length);

export function prop(object, name) {
    const wanted = name.toLowerCase();

    return Object.keys(object).find(key => key.toLowerCase() === wanted);
}

export function read(object, name) {
    const found = prop(object, name);

    return found === undefined ? undefined : object[found];
}

export function pretty(value) {
    return JSON.stringify(value === undefined ? null : value, null, 2);
}

export function readable(plain) {
    try {
        return JSON.stringify(JSON.parse(plain), null, 2);
    } catch {
        return plain;
    }
}

export function matcherFor(key) {
    return MATCHERS.find(group => key.startsWith(group.prefix));
}

export function nameOf(item) {
    const group = matcherFor(item.key);

    return group ? item.key.slice(group.prefix.length) : item.key;
}

export function readBundle(plain) {
    let bundle = null;

    try {
        bundle = JSON.parse(plain);
    } catch {
        return { error: "json" };
    }

    if (!bundle || typeof bundle !== "object" || Array.isArray(bundle)) {
        return { error: "json" };
    }

    if (read(bundle, "format") !== FORMAT) {
        return { error: "format" };
    }

    const entriesProp = prop(bundle, "entries");

    if (!entriesProp || !Array.isArray(bundle[entriesProp])) {
        return { error: "entries" };
    }

    return { bundle, entriesProp, items: bundle[entriesProp].map(toItem).filter(Boolean) };
}

function toItem(entry, index) {
    if (!entry || typeof entry !== "object" || Array.isArray(entry)) {
        return null;
    }

    const item = {
        id: index,
        entry,
        entryKeyProp: prop(entry, "key") || "key",
        entryRawProp: prop(entry, "raw") || "raw",
        key: String(read(entry, "key") || ""),
        raw: String(read(entry, "raw") || ""),
        envelope: null,
        envKeyProp: "key",
        envValueProp: "value",
        type: "",
        version: 0,
        text: "",
        error: null,
        removed: false,
        open: false,
        broken: false,
    };

    let envelope = null;

    try {
        envelope = JSON.parse(item.raw);
    } catch {
        envelope = null;
    }

    if (!envelope || typeof envelope !== "object" || Array.isArray(envelope)) {
        item.broken = true;

        return item;
    }

    item.envelope = envelope;
    item.envKeyProp = prop(envelope, "key") || "key";
    item.envValueProp = prop(envelope, "value") || "value";
    item.type = String(read(envelope, "type") || "");
    item.version = Number(read(envelope, "version") || 0);
    item.text = pretty(envelope[item.envValueProp]);

    return item;
}

export function bucket(items) {
    const buckets = new Map(GROUPS.map(group => [group.label, []]));

    buckets.set(OTHER_GROUP, []);

    for (const item of items) {
        const group = matcherFor(item.key);

        buckets.get(group ? group.label : OTHER_GROUP).push(item);
    }

    for (const [label, held] of buckets) {
        if (held.length === 0) {
            buckets.delete(label);
        }
    }

    return buckets;
}

export function validate(item) {
    try {
        JSON.parse(item.text);
        item.error = null;
    } catch (problem) {
        item.error = "That is not valid JSON. " + (problem.message || problem);
    }

    return item.error;
}

export function renameItem(items, item, wanted) {
    const clean = String(wanted || "").trim();

    if (clean.length === 0) {
        return { error: "A name cannot be empty." };
    }

    const group = matcherFor(item.key);
    const key = group ? group.prefix + clean : clean;

    if (!key.startsWith(OWNED_PREFIX)) {
        return { error: "vMenu only accepts keys starting with " + OWNED_PREFIX + ", so this one would be skipped." };
    }

    if (items.some(other => other !== item && !other.removed && other.key === key)) {
        return { error: "Something else in this code already has that name." };
    }

    const was = nameOf(item);

    item.key = key;

    if (item.envelope) {
        item.envelope[item.envKeyProp] = key;
    }

    renameInside(item, was, clean);

    return { name: clean };
}

/* The name lives in the payload as well as in the key, and vMenu shows that one in its menus. */
function renameInside(item, was, now) {
    try {
        const value = JSON.parse(item.text);

        if (!value || typeof value !== "object" || Array.isArray(value)) {
            return;
        }

        const nameProp = prop(value, "name");

        if (nameProp === undefined || value[nameProp] !== was) {
            return;
        }

        value[nameProp] = now;
        item.text = pretty(value);
    } catch {
        /* Mid edit the payload may not parse, and a stale name is better than losing the edit. */
    }
}

export function buildBundle(source, entriesProp, items) {
    const bundle = { ...source };
    const createdProp = prop(source, "createdAt") || "createdAt";

    bundle[createdProp] = new Date().toISOString();
    bundle[entriesProp] = items.filter(item => !item.removed).map(toEntry);

    return bundle;
}

function toEntry(item) {
    const entry = { ...item.entry };

    entry[item.entryKeyProp] = item.key;
    entry[item.entryRawProp] = item.broken ? item.raw : rawOf(item);

    return entry;
}

/* Rebuilt from the parsed envelope, so whatever a newer vMenu put in it survives the edit. */
function rawOf(item) {
    const envelope = { ...item.envelope };

    envelope[item.envKeyProp] = item.key;
    envelope[item.envValueProp] = JSON.parse(item.text);

    return JSON.stringify(envelope);
}

import { decodeCode, encodeCode } from "./vmenu-code.js";
import { makeZip } from "./zip.js";
import { buildBundle, pretty, read, readBundle, readable } from "./bundle.js";
import {
    CHANGED,
    ONLY_LEFT,
    ONLY_RIGHT,
    SAME,
    canKeepBoth,
    compare,
    fieldChanges,
    merge,
    olderKept,
} from "./compare.js";

const REASONS = {
    empty: "There is nothing in this box yet. Paste a code first.",
    prefix: "That does not look like a vMenu code. A real one starts with VME1.",
    base64: "That code is damaged or incomplete. Copy the whole thing again, from VME1 all the way to the end.",
    gzip: "That code could not be unpacked. It is usually incomplete, so copy the whole thing again.",
    json: "That code unpacked, but what came out of it was not readable.",
    format: "That unpacked fine, but it is not a vMenu data code.",
    entries: "That code does not have a list of saved items in it.",
};

const STATUS = {
    [ONLY_LEFT]: { label: "only left", tone: "vmt-badge-left" },
    [ONLY_RIGHT]: { label: "only right", tone: "vmt-badge-right" },
    [CHANGED]: { label: "different", tone: "vmt-badge-changed" },
    [SAME]: { label: "same", tone: "vmt-badge-same" },
};

const MAX_FIELDS = 300;
const MAX_SHOWN = 160;

const HELP = [
    "What is in this zip",
    "",
    "merged-code.txt",
    "    The merged code. Copy the whole line, open vMenu in game, go to",
    "    Misc Settings, Import & Export, pick Paste A Code, and paste it in.",
    "",
    "merged-code-readable.json",
    "    The same thing unpacked, so you can read it. vMenu does not want this one,",
    "    it only wants the code above.",
    "",
    "backup/left-code.txt and backup/right-code.txt",
    "    The two codes exactly as you pasted them into the page.",
    "    If the merge went wrong, paste one of these back into vMenu instead.",
    "",
    "backup/left-code-readable.json and backup/right-code-readable.json",
    "    The two originals, unpacked so you can read them.",
].join("\n");

const sides = {
    left: { code: "", plain: "", bundle: null, entriesProp: "entries", items: [] },
    right: { code: "", plain: "", bundle: null, entriesProp: "entries", items: [] },
};

let rows = [];
let merged = { items: [], errors: new Map() };

const els = {};
const rowEls = new Map();

export function setup() {
    const ids = [
        "load", "status", "result", "summary", "hide-same", "all-left", "all-right", "all-newest",
        "groups", "warn", "download", "copy", "out",
    ];

    for (const id of ids) {
        els[id] = document.getElementById("vmc-" + id);
    }

    if (!els.load) {
        return;
    }

    for (const side of ["left", "right"]) {
        const code = document.getElementById("vmc-code-" + side);
        const file = document.getElementById("vmc-file-" + side);

        els["code-" + side] = code;
        els["status-" + side] = document.getElementById("vmc-status-" + side);

        document.getElementById("vmc-pick-" + side).addEventListener("click", () => file.click());
        file.addEventListener("change", async event => {
            const picked = event.target.files && event.target.files[0];

            if (picked) {
                code.value = await picked.text();
                file.value = "";
            }
        });
    }

    els.load.addEventListener("click", load);
    els["hide-same"].addEventListener("change", render);
    els["all-left"].addEventListener("click", () => chooseAll(() => "left"));
    els["all-right"].addEventListener("click", () => chooseAll(() => "right"));
    els["all-newest"].addEventListener("click", () =>
        chooseAll(row => row.right.version > row.left.version ? "right" : "left"));
    els.download.addEventListener("click", download);
    els.copy.addEventListener("click", copy);
}

function el(tag, className, content) {
    const node = document.createElement(tag);

    if (className) {
        node.className = className;
    }

    if (content !== undefined) {
        node.textContent = content;
    }

    return node;
}

function say(target, message, tone) {
    target.textContent = message || "";
    target.className = tone ? "vmt-status vmt-" + tone : "vmt-status";
}

async function open(side) {
    const status = els["status-" + side];
    const decoded = await decodeCode(els["code-" + side].value);

    if (decoded.error) {
        say(status, REASONS[decoded.error] || REASONS.json, "bad");

        return false;
    }

    const opened = readBundle(decoded.plain);

    if (opened.error) {
        say(status, REASONS[opened.error] || REASONS.json, "bad");

        return false;
    }

    Object.assign(sides[side], {
        code: decoded.cleaned,
        plain: decoded.plain,
        bundle: opened.bundle,
        entriesProp: opened.entriesProp,
        items: opened.items,
    });

    const made = String(read(opened.bundle, "createdAt") || "");

    say(status, opened.items.length + " saved item(s)" + (made ? ", made on " + made : "") + ".", "good");

    return true;
}

async function load() {
    say(els.status, "Reading both codes.", null);

    const [left, right] = await Promise.all([open("left"), open("right")]);

    if (!left || !right) {
        els.result.hidden = true;
        say(els.status, "Both codes need to read correctly before they can be compared.", "bad");

        return;
    }

    rows = compare(sides.left.items, sides.right.items);

    els.result.hidden = false;
    say(els.status, "", null);

    render();
}

function chooseAll(pick) {
    for (const row of rows) {
        if (row.status === CHANGED) {
            row.choice = pick(row);
        }
    }

    render();
}

function count(status) {
    return rows.filter(row => row.status === status).length;
}

function render() {
    rowEls.clear();
    els.groups.replaceChildren();

    const hideSame = els["hide-same"].checked;
    const groups = new Map();

    for (const row of rows) {
        if (hideSame && row.status === SAME) {
            continue;
        }

        if (!groups.has(row.group)) {
            groups.set(row.group, []);
        }

        groups.get(row.group).push(row);
    }

    for (const [label, held] of groups) {
        els.groups.appendChild(renderGroup(label, held));
    }

    if (groups.size === 0) {
        els.groups.appendChild(el("div", "vmt-panel", "Both codes hold exactly the same items."));
    }

    refresh();
}

function renderGroup(label, held) {
    const box = el("details", "vmt-group");

    box.open = true;

    const head = el("summary", "vmt-group-head");

    head.append(el("span", "vmt-group-name", label), el("span", "vmt-count", String(held.length)));
    box.append(head);

    for (const row of held) {
        const node = renderRow(row);

        rowEls.set(row, node);
        box.append(node);
    }

    return box;
}

function updateRow(row) {
    const old = rowEls.get(row);

    if (old) {
        const node = renderRow(row);

        old.replaceWith(node);
        rowEls.set(row, node);
    }

    refresh();
}

function kept(row) {
    return row.choice !== "skip";
}

function renderRow(row) {
    const box = el("details", kept(row) ? "vmt-item" : "vmt-item vmt-gone");

    box.open = Boolean(row.open);

    const head = el("summary", "vmt-item-head");
    const status = STATUS[row.status];

    head.append(el("span", "vmt-item-name", row.name), el("span", "vmt-badge " + status.tone, status.label));
    head.append(el("span", "vmt-meta", describe(row)));

    if (row.status === CHANGED) {
        head.append(el("span", "vmt-meta", choiceLabel(row)));
    }

    if (olderKept(row)) {
        head.append(el("span", "vmt-badge vmt-badge-bad", "older version"));
    }

    if (!kept(row)) {
        head.append(el("span", "vmt-badge vmt-badge-gone", "left out"));
    }

    const body = el("div", "vmt-item-body");

    box.append(head, body);

    const fill = () => {
        if (body.childElementCount === 0) {
            body.append(...renderBody(row));
        }
    };

    if (box.open) {
        fill();
    }

    box.addEventListener("toggle", () => {
        row.open = box.open;

        if (box.open) {
            fill();
        }
    });

    return box;
}

function describe(row) {
    const item = row.left || row.right;
    const type = item.type || "unreadable";

    if (row.left && row.right && row.left.version !== row.right.version) {
        return type + ", version " + row.left.version + " left, " + row.right.version + " right";
    }

    return type + ", version " + item.version;
}

function choiceLabel(row) {
    if (row.choice === "both") {
        return "keeping both";
    }

    return "keeping " + row.choice;
}

function renderBody(row) {
    if (row.status === SAME) {
        return [el("p", "vmt-note", "Exactly the same in both codes, so it is kept once."), preview(row.left)];
    }

    if (row.status === ONLY_LEFT || row.status === ONLY_RIGHT) {
        return renderSingle(row);
    }

    return renderChanged(row);
}

function renderSingle(row) {
    const side = row.status === ONLY_LEFT ? "left" : "right";
    const other = side === "left" ? "right" : "left";
    const label = el("label", "vmt-check");
    const box = el("input");

    box.type = "checkbox";
    box.checked = kept(row);
    box.addEventListener("change", () => {
        row.choice = box.checked ? side : "skip";
        updateRow(row);
    });

    label.append(box, " Keep this in the merged code");

    return [
        el("p", "vmt-note", "Only in the " + side + " code, the " + other + " code does not have it."),
        label,
        preview(row[side]),
    ];
}

function renderChanged(row) {
    const parts = [];
    const choices = el("div", "vmt-row");
    const name = "vmc-choice-" + rows.indexOf(row);
    const options = [["left", "Keep left"], ["right", "Keep right"]];

    if (canKeepBoth(row)) {
        options.push(["both", "Keep both"]);
    }

    for (const [value, text] of options) {
        const label = el("label", "vmt-check");
        const radio = el("input");

        radio.type = "radio";
        radio.name = name;
        radio.value = value;
        radio.checked = row.choice === value;
        radio.addEventListener("change", () => {
            row.choice = value;
            updateRow(row);
        });

        label.append(radio, " " + text);
        choices.append(label);
    }

    parts.push(choices);

    const older = olderKept(row);

    if (older) {
        parts.push(el(
            "p",
            "vmt-warn",
            "The " + row.choice + " copy is version " + older.kept + ", the other one is version " + older.other
            + ". You are keeping the older one. Paste A Code will skip it if your computer already has the newer one."));
    }

    if (row.choice === "both") {
        parts.push(renderCopyName(row));
    }

    parts.push(renderFields(row));

    return parts;
}

function renderCopyName(row) {
    const wrap = el("div", "vmt-item-body vmt-flat");
    const line = el("div", "vmt-row");
    const side = el("select", "vmt-input vmt-select");

    for (const [value, text] of [["right", "Rename the right one"], ["left", "Rename the left one"]]) {
        const option = el("option", null, text);

        option.value = value;
        option.selected = row.renameSide === value;
        side.append(option);
    }

    side.addEventListener("change", () => {
        row.renameSide = side.value;
        refresh();
    });

    const input = el("input", "vmt-input");

    input.type = "text";
    input.value = row.copyName;
    input.spellcheck = false;
    input.addEventListener("input", () => {
        row.copyName = input.value;
        refresh();
    });

    const problem = el("div", "vmt-problem");

    row.problemEl = problem;
    problem.textContent = merged.errors.get(row) || "";

    line.append(side, el("label", "vmt-row-label", "to"), input);
    wrap.append(line, problem);

    return wrap;
}

function show(value) {
    if (value === undefined) {
        return "(missing)";
    }

    return value.length > MAX_SHOWN ? value.slice(0, MAX_SHOWN) + "..." : value;
}

function renderFields(row) {
    const changes = fieldChanges(row.left, row.right);
    const wrap = el("div", "vmt-diff");
    const table = el("table", "vmt-diff-table");
    const head = el("tr");

    head.append(el("th", null, "Field"), el("th", null, "Left"), el("th", null, "Right"));
    table.append(head);

    for (const change of changes.slice(0, MAX_FIELDS)) {
        const line = el("tr");
        const left = el("td", change.left === undefined ? "vmt-missing" : "vmt-differs", show(change.left));
        const right = el("td", change.right === undefined ? "vmt-missing" : "vmt-differs", show(change.right));

        left.title = change.left || "";
        right.title = change.right || "";

        line.append(el("td", "vmt-path", change.path), left, right);
        table.append(line);
    }

    wrap.append(table);

    if (changes.length > MAX_FIELDS) {
        wrap.append(el("p", "vmt-note", "And " + (changes.length - MAX_FIELDS) + " more differences."));
    }

    return wrap;
}

function preview(item) {
    const box = el("textarea", "vmt-editor");
    const text = item.broken ? item.raw : item.text;

    box.value = text;
    box.readOnly = true;
    box.spellcheck = false;
    box.rows = Math.min(16, Math.max(3, text.split("\n").length));

    return box;
}

function refresh() {
    merged = merge(rows);

    for (const row of rows) {
        if (row.problemEl) {
            row.problemEl.textContent = merged.errors.get(row) || "";
        }
    }

    renderSummary();

    const older = rows.filter(olderKept);

    els.warn.hidden = older.length === 0;
    els.warn.textContent = older.length === 0
        ? ""
        : older.length + " item(s) keep an older version than the other code has: "
            + older.map(row => row.name).join(", ") + ".";

    const failing = [...merged.errors.entries()];

    els.download.disabled = failing.length > 0 || merged.items.length === 0;
    els.copy.disabled = els.download.disabled;

    if (failing.length > 0) {
        say(els.out, "Fix the new name on " + failing[0][0].name + ". " + failing[0][1], "bad");
    } else if (merged.items.length === 0) {
        say(els.out, "Everything has been left out, so there is nothing to save.", "bad");
    } else {
        say(els.out, "", null);
    }
}

function renderSummary() {
    els.summary.replaceChildren();

    const chips = el("div", "vmt-chips");

    chips.append(
        el("span", "vmt-chip", "Only left: " + count(ONLY_LEFT)),
        el("span", "vmt-chip", "Only right: " + count(ONLY_RIGHT)),
        el("span", "vmt-chip", "Different: " + count(CHANGED)),
        el("span", "vmt-chip", "Same: " + count(SAME)),
        el("span", "vmt-chip vmt-chip-total", "In the merged code: " + merged.items.length));

    els.summary.append(chips);
}

function baseSide() {
    const leftVersion = Number(read(sides.left.bundle, "version") || 0);
    const rightVersion = Number(read(sides.right.bundle, "version") || 0);

    return rightVersion > leftVersion ? sides.right : sides.left;
}

async function mergedCode() {
    const base = baseSide();
    const bundle = buildBundle(base.bundle, base.entriesProp, merged.items);

    return { bundle, code: await encodeCode(JSON.stringify(bundle)) };
}

async function download() {
    if (els.download.disabled) {
        return;
    }

    say(els.out, "Packing your zip.", null);

    const made = await mergedCode();

    const blob = makeZip([
        { name: "merged-code.txt", text: made.code },
        { name: "merged-code-readable.json", text: pretty(made.bundle) },
        { name: "backup/left-code.txt", text: sides.left.code },
        { name: "backup/left-code-readable.json", text: readable(sides.left.plain) },
        { name: "backup/right-code.txt", text: sides.right.code },
        { name: "backup/right-code-readable.json", text: readable(sides.right.plain) },
        { name: "read-me-first.txt", text: HELP },
    ]);

    const link = el("a");
    const url = URL.createObjectURL(blob);

    link.href = url;
    link.download = "vmenu-data-merged-" + new Date().toISOString().slice(0, 10) + ".zip";

    document.body.append(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(url);

    say(els.out, "Downloaded. The zip holds the merged code and a backup of both codes you pasted in.", "good");
}

async function copy() {
    if (els.copy.disabled) {
        return;
    }

    const made = await mergedCode();

    try {
        await navigator.clipboard.writeText(made.code);

        say(els.out, "Copied. Paste it into vMenu under Misc Settings, Import & Export.", "good");
    } catch {
        say(els.out, "Copying was blocked by your browser. Use Download zip instead.", "bad");
    }
}

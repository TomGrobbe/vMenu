import { decodeCode, encodeCode } from "./vmenu-code.js";
import { makeZip } from "./zip.js";
import {
    bucket,
    buildBundle,
    nameOf,
    pretty,
    read,
    readBundle,
    readable,
    renameItem,
    validate,
} from "./bundle.js";

const REASONS = {
    empty: "There is nothing in the box yet. Paste your code first.",
    prefix: "That does not look like a vMenu code. A real one starts with VME1.",
    base64: "That code is damaged or incomplete. Copy the whole thing again, from VME1 all the way to the end.",
    gzip: "That code could not be unpacked. It is usually incomplete, so copy the whole thing again.",
    json: "That code unpacked, but what came out of it was not readable.",
    format: "That unpacked fine, but it is not a vMenu data code.",
    entries: "That code does not have a list of saved items in it.",
};

const HELP = [
    "What is in this zip",
    "",
    "new-code.txt",
    "    Your edited code. Copy the whole line, open vMenu in game, go to",
    "    Misc Settings, Import & Export, pick Paste A Code, and paste it in.",
    "",
    "new-code-readable.json",
    "    The same thing unpacked, so you can read it. vMenu does not want this one,",
    "    it only wants the code above.",
    "",
    "backup/original-code.txt",
    "    The code exactly as you pasted it into the page, before any edits.",
    "    If an edit went wrong, paste this one back into vMenu instead.",
    "",
    "backup/original-code-readable.json",
    "    The original, unpacked so you can read it.",
].join("\n");

const state = {
    originalCode: "",
    originalPlain: "",
    bundle: null,
    entriesProp: "entries",
    items: [],
};

let codeEl = null;
let loadEl = null;
let pickEl = null;
let fileEl = null;
let statusEl = null;
let resultEl = null;
let summaryEl = null;
let groupsEl = null;
let downloadEl = null;
let copyEl = null;
let outEl = null;

export function setup() {
    codeEl = document.getElementById("vmt-code");

    if (!codeEl) {
        return;
    }

    loadEl = document.getElementById("vmt-load");
    pickEl = document.getElementById("vmt-pick");
    fileEl = document.getElementById("vmt-file");
    statusEl = document.getElementById("vmt-status");
    resultEl = document.getElementById("vmt-result");
    summaryEl = document.getElementById("vmt-summary");
    groupsEl = document.getElementById("vmt-groups");
    downloadEl = document.getElementById("vmt-download");
    copyEl = document.getElementById("vmt-copy");
    outEl = document.getElementById("vmt-out");

    loadEl.addEventListener("click", load);
    pickEl.addEventListener("click", () => fileEl.click());
    fileEl.addEventListener("change", readFile);
    downloadEl.addEventListener("click", download);
    copyEl.addEventListener("click", copy);
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

async function readFile(event) {
    const file = event.target.files && event.target.files[0];

    if (!file) {
        return;
    }

    codeEl.value = await file.text();
    fileEl.value = "";

    load();
}

async function load() {
    say(statusEl, "Reading your code.", null);

    const decoded = await decodeCode(codeEl.value);

    if (decoded.error) {
        fail(REASONS[decoded.error] || REASONS.json);

        return;
    }

    const opened = readBundle(decoded.plain);

    if (opened.error) {
        fail(REASONS[opened.error] || REASONS.json);

        return;
    }

    state.originalCode = decoded.cleaned;
    state.originalPlain = decoded.plain;
    state.bundle = opened.bundle;
    state.entriesProp = opened.entriesProp;
    state.items = opened.items;

    resultEl.hidden = false;

    say(statusEl, "Read " + state.items.length + " saved item(s).", "good");
    render();
}

function fail(message) {
    resultEl.hidden = true;
    state.items = [];

    say(statusEl, message, "bad");
}

function render() {
    const buckets = bucket(state.items);

    renderSummary(buckets);

    groupsEl.replaceChildren();

    for (const [label, items] of buckets) {
        groupsEl.appendChild(renderGroup(label, items));
    }

    refresh();
}

function renderSummary(buckets) {
    summaryEl.replaceChildren();

    const made = String(read(state.bundle, "createdAt") || "");
    const kept = state.items.filter(item => !item.removed).length;

    summaryEl.appendChild(el("div", "vmt-made", made ? "This code was made on " + made + "." : "This code is not dated."));

    const chips = el("div", "vmt-chips");

    for (const [label, items] of buckets) {
        chips.appendChild(el("span", "vmt-chip", label + ": " + items.length));
    }

    chips.appendChild(el("span", "vmt-chip vmt-chip-total", "Kept in the new code: " + kept));

    summaryEl.appendChild(chips);
}

function renderGroup(label, items) {
    const box = el("details", "vmt-group");

    box.open = true;

    const head = el("summary", "vmt-group-head");

    head.append(el("span", "vmt-group-name", label), el("span", "vmt-count", String(items.length)));
    box.append(head);

    for (const item of items) {
        box.append(renderItem(item));
    }

    return box;
}

function renderItem(item) {
    const box = el("details", item.removed ? "vmt-item vmt-gone" : "vmt-item");

    box.open = item.open;
    box.addEventListener("toggle", () => {
        item.open = box.open;
    });

    const head = el("summary", "vmt-item-head");

    head.append(el("span", "vmt-item-name", nameOf(item)));

    if (item.type) {
        head.append(el("span", "vmt-meta", item.type + ", version " + item.version));
    }

    if (item.broken) {
        head.append(el("span", "vmt-badge vmt-badge-bad", "unreadable"));
    }

    if (item.removed) {
        head.append(el("span", "vmt-badge vmt-badge-gone", "deleted"));
    }

    box.append(head, item.broken ? renderBroken(item) : renderEditor(item));

    return box;
}

function renderBroken(item) {
    const body = el("div", "vmt-item-body");

    body.append(el(
        "p",
        "vmt-note",
        "vMenu cannot read this one and would skip it anyway. It is passed through untouched."));

    body.append(renderButtons(item, null));

    return body;
}

function renderEditor(item) {
    const body = el("div", "vmt-item-body");

    const nameRow = el("div", "vmt-row");
    const nameInput = el("input", "vmt-input");

    nameInput.type = "text";
    nameInput.value = nameOf(item);
    nameInput.spellcheck = false;

    const renameButton = el("button", "vmt-button", "Rename");

    renameButton.type = "button";
    renameButton.addEventListener("click", () => rename(item, nameInput.value));

    nameInput.addEventListener("keydown", event => {
        if (event.key === "Enter") {
            event.preventDefault();
            rename(item, nameInput.value);
        }
    });

    nameRow.append(el("label", "vmt-row-label", "Name"), nameInput, renameButton);

    const editor = el("textarea", "vmt-editor");

    editor.value = item.text;
    editor.spellcheck = false;
    editor.rows = Math.min(24, Math.max(4, item.text.split("\n").length));

    const problem = el("div", "vmt-problem");

    editor.addEventListener("input", () => {
        item.text = editor.value;

        problem.textContent = validate(item) || "";
        editor.classList.toggle("vmt-editor-bad", Boolean(item.error));

        refresh();
    });

    body.append(nameRow, editor, problem, renderButtons(item, editor));

    return body;
}

function renderButtons(item, editor) {
    const row = el("div", "vmt-row vmt-row-end");

    if (editor) {
        const tidy = el("button", "vmt-button", "Reformat JSON");

        tidy.type = "button";
        tidy.title = "Lays the JSON out again with even indentation. Nothing in it is changed.";
        tidy.addEventListener("click", () => {
            try {
                item.text = pretty(JSON.parse(editor.value));
                item.error = null;

                render();
            } catch {
                say(outEl, "That cannot be tidied up until the JSON is valid again.", "bad");
            }
        });

        row.append(tidy);
    }

    const toggle = el(
        "button",
        item.removed ? "vmt-button" : "vmt-button vmt-danger",
        item.removed ? "Put it back" : "Delete item");

    toggle.type = "button";
    toggle.title = item.removed
        ? "Puts this back into the new code."
        : "Removes the item from the new code.";
    toggle.addEventListener("click", () => {
        item.removed = !item.removed;

        render();
    });

    row.append(toggle);

    return row;
}

function rename(item, wanted) {
    const result = renameItem(state.items, item, wanted);

    if (result.error) {
        say(outEl, result.error, "bad");

        return;
    }

    render();
    say(outEl, "Renamed to " + result.name + ".", "good");
}

function problems() {
    return state.items.filter(item => !item.removed && item.error);
}

function refresh() {
    const broken = problems();
    const kept = state.items.filter(item => !item.removed).length;

    downloadEl.disabled = broken.length > 0 || kept === 0;
    copyEl.disabled = downloadEl.disabled;

    if (broken.length > 0) {
        say(outEl, "Fix the JSON on " + nameOf(broken[0]) + " before you download.", "bad");
    } else if (kept === 0) {
        say(outEl, "Everything has been taken out, so there is nothing left to save.", "bad");
    }
}

async function newCode() {
    const bundle = buildBundle(state.bundle, state.entriesProp, state.items);

    return { bundle, code: await encodeCode(JSON.stringify(bundle)) };
}

async function download() {
    if (problems().length > 0) {
        return;
    }

    say(outEl, "Packing your zip.", null);

    const made = await newCode();

    const blob = makeZip([
        { name: "new-code.txt", text: made.code },
        { name: "new-code-readable.json", text: JSON.stringify(made.bundle, null, 2) },
        { name: "backup/original-code.txt", text: state.originalCode },
        { name: "backup/original-code-readable.json", text: readable(state.originalPlain) },
        { name: "read-me-first.txt", text: HELP },
    ]);

    const link = el("a");
    const url = URL.createObjectURL(blob);

    link.href = url;
    link.download = "vmenu-data-" + new Date().toISOString().slice(0, 10) + ".zip";

    document.body.append(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(url);

    say(outEl, "Downloaded. The zip holds your new code and a backup of the one you pasted in.", "good");
}

async function copy() {
    if (problems().length > 0) {
        return;
    }

    const made = await newCode();

    try {
        await navigator.clipboard.writeText(made.code);

        say(outEl, "Copied. Paste it into vMenu under Misc Settings, Import & Export.", "good");
    } catch {
        codeEl.value = made.code;
        codeEl.focus();
        codeEl.select();

        say(outEl, "Copying was blocked, so the new code is in the box at the top instead.", "bad");
    }
}

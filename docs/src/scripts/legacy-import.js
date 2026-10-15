import pedModelsText from "../../../assets/shared/config/ped-models.json?raw";
import weaponsText from "../../../assets/shared/config/weapons.json?raw";
import weaponComponentsText from "../../../assets/shared/config/weapon-components.json?raw";

import { FORMAT } from "./bundle.js";
import { fromDrop, fromFiles } from "./kvs-files.js";
import { GROUP_ORDER, convert, findResources, makeLookups, parseCommentedJson } from "./legacy-convert.js";
import { readDatabase } from "./leveldb.js";
import { encodeCode } from "./vmenu-code.js";
import { makeZip } from "./zip.js";

const HELP = [
    "What is in this zip",
    "",
    "vmenu-code.txt",
    "    Your converted code. Copy the whole line, open vMenu Enhanced in game, go to",
    "    Misc Settings, Import & Export, pick Paste A Code, and paste it in.",
    "",
    "vmenu-code-readable.json",
    "    The same thing unpacked, so you can read it. vMenu does not want this one,",
    "    it only wants the code above.",
    "",
    "skipped.txt",
    "    Everything from the old vMenu that could not be brought over, and why.",
].join("\n");

const state = {
    entries: null,
    resources: [],
    items: [],
    lookups: null,
};

const ui = {};

export function setup() {
    for (const id of ["path", "copy-path", "path-status", "pick-folder", "pick-zip", "folder", "zip", "drop", "status",
        "result", "summary", "resource-row", "resource", "groups", "warn", "copy", "download", "out", "code"]) {
        ui[id] = document.getElementById("vmi-" + id);
    }

    if (!ui.drop) {
        return;
    }

    ui["copy-path"].addEventListener("click", copyPath);
    ui["pick-folder"].addEventListener("click", () => ui.folder.click());
    ui["pick-zip"].addEventListener("click", () => ui.zip.click());
    ui.folder.addEventListener("change", () => picked(ui.folder));
    ui.zip.addEventListener("change", () => picked(ui.zip));
    ui.resource.addEventListener("change", () => use(ui.resource.value));
    ui.copy.addEventListener("click", copy);
    ui.download.addEventListener("click", download);

    ui.drop.addEventListener("keydown", event => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            ui.folder.click();
        }
    });

    for (const name of ["dragenter", "dragover"]) {
        ui.drop.addEventListener(name, event => {
            event.preventDefault();
            ui.drop.classList.add("vmt-drop-active");
        });
    }

    ui.drop.addEventListener("dragleave", () => ui.drop.classList.remove("vmt-drop-active"));
    ui.drop.addEventListener("drop", event => {
        event.preventDefault();
        ui.drop.classList.remove("vmt-drop-active");
        open(() => fromDrop(event.dataTransfer));
    });
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

async function copyPath() {
    try {
        await navigator.clipboard.writeText(ui.path.textContent.trim());
        say(ui["path-status"], "Copied.", "good");
    } catch {
        window.getSelection().selectAllChildren(ui.path);
        say(ui["path-status"], "Press Ctrl+C to copy it.", null);
    }
}

function picked(input) {
    const files = [...(input.files || [])];

    input.value = "";

    if (files.length > 0) {
        open(() => fromFiles(files));
    }
}

function lookups() {
    if (!state.lookups) {
        const load = raw => {
            try {
                return parseCommentedJson(raw);
            } catch {
                return {};
            }
        };

        state.lookups = makeLookups({
            pedModels: load(pedModelsText),
            weapons: load(weaponsText),
            weaponComponents: load(weaponComponentsText),
        });
    }

    return state.lookups;
}

let reading = 0;

async function open(read) {
    const ticket = ++reading;

    ui.result.hidden = true;
    say(ui.status, "Reading your files.", null);

    try {
        await show(read, ticket);
    } catch {
        if (ticket === reading) {
            ui.result.hidden = true;
            say(ui.status, "Something in your files could not be converted. Close FiveM and its launcher completely and try again.", "bad");
        }
    }
}

async function show(read, ticket) {
    let database;

    try {
        database = await read();
    } catch (problem) {
        if (ticket !== reading) {
            return;
        }

        const message = String(problem && problem.message);

        if (message === "not-zip" || message === "broken-zip") {
            say(ui.status, "That zip could not be opened. Make a new one and try again.", "bad");
        } else if (problem instanceof ReferenceError && typeof DecompressionStream !== "function") {
            say(ui.status, "Your browser is too old to open zip files. Pick the folder instead, or use a newer browser.", "bad");
        } else {
            say(ui.status, "Those files could not be read. Close FiveM and its launcher completely and try again.", "bad");
        }

        return;
    }

    if (ticket !== reading) {
        return;
    }

    if (!database || database.files.length === 0) {
        say(ui.status, "No FiveM storage was found in there. Pick the kvs folder inside %AppData%\\CitizenFX, or a zip of it.", "bad");

        return;
    }

    const { entries, problems } = readDatabase(database.files);

    state.entries = entries;
    state.resources = findResources(entries);

    if (state.resources.length === 0) {
        say(ui.status, "The folder was read, but it has no old vMenu data in it.", "bad");

        return;
    }

    say(ui.status, "Read " + database.files.length + " file(s) from " + (database.folder || "the files you picked") + ".", "good");

    ui.warn.hidden = problems.length === 0;
    ui.warn.textContent = problems.length === 0
        ? ""
        : "Some files could not be read, so a few saves may be missing. This usually means FiveM was still running. Close FiveM and its launcher completely and pick the folder again.";

    ui.resource.replaceChildren(...state.resources.map(resource =>
        Object.assign(el("option", null, resource.name + " (" + resource.count + " saved items)"), { value: resource.name })));
    ui["resource-row"].hidden = state.resources.length < 2;

    use(state.resources[0].name);
}

function use(resource) {
    state.items = convert(state.entries, resource, lookups()).map((item, index) => ({ ...item, id: index, kept: !item.skipped }));

    ui.result.hidden = false;
    ui.code.hidden = true;

    render();
}

function render() {
    const converted = state.items.filter(item => !item.skipped);
    const skipped = state.items.length - converted.length;

    ui.summary.replaceChildren(el("div", "vmt-made", "Found " + converted.length + " item(s) that can be brought over" + (skipped ? ", and " + skipped + " that cannot." : ".")));

    const chips = el("div", "vmt-chips");

    for (const group of GROUP_ORDER) {
        const count = converted.filter(item => item.group === group).length;

        if (count > 0) {
            chips.append(el("span", "vmt-chip", group + ": " + count));
        }
    }

    ui.summary.append(chips);
    ui.groups.replaceChildren();

    for (const group of GROUP_ORDER) {
        const items = state.items.filter(item => item.group === group);

        if (items.length > 0) {
            ui.groups.append(renderGroup(group, items));
        }
    }

    refresh();
}

function renderGroup(label, items) {
    const box = el("details", "vmt-group");
    const head = el("summary", "vmt-group-head");
    const converted = items.filter(item => !item.skipped).length;

    box.open = label !== "Settings";
    head.append(el("span", "vmt-group-name", label), el("span", "vmt-count", String(converted)));
    box.append(head);

    for (const item of items) {
        box.append(renderItem(item));
    }

    return box;
}

function renderItem(item) {
    const box = el("details", "vmt-item");
    const head = el("summary", "vmt-item-head");

    head.append(el("span", "vmt-item-name", item.name));

    if (item.skipped) {
        head.append(el("span", "vmt-badge vmt-badge-bad", "skipped"));
        box.append(head, el("p", "vmt-reason", item.skipped));

        return box;
    }

    const keep = el("input");

    keep.type = "checkbox";
    keep.checked = item.kept;
    keep.title = "Bring this one over";
    keep.addEventListener("click", event => event.stopPropagation());
    keep.addEventListener("change", () => {
        item.kept = keep.checked;
        refresh();
    });

    head.prepend(keep);

    if (item.notes.length > 0) {
        head.append(el("span", "vmt-badge vmt-badge-changed", "note"));
    }

    const body = el("div", "vmt-item-body");

    for (const note of item.notes) {
        body.append(el("p", "vmt-note", note));
    }

    const view = el("textarea", "vmt-editor");
    const value = JSON.stringify(JSON.parse(item.raw).value, null, 2);

    view.value = value;
    view.readOnly = true;
    view.spellcheck = false;
    view.rows = Math.min(16, Math.max(2, value.split("\n").length));

    body.append(view);
    box.append(head, body);

    return box;
}

function kept() {
    const ticked = state.items.filter(item => !item.skipped && item.kept);
    const keys = new Set(ticked.map(item => item.key));

    return ticked.filter(item => !item.requires || item.requires.some(key => keys.has(key)));
}

function refresh() {
    const count = kept().length;

    ui.copy.disabled = count === 0;
    ui.download.disabled = count === 0;

    say(ui.out, count === 0 ? "Nothing is ticked, so there is nothing to bring over." : count + " item(s) will go in your code.", count === 0 ? "bad" : null);
}

function skippedReport() {
    const lines = state.items.filter(item => item.skipped).map(item => item.group + ": " + item.name + "\n    " + item.skipped);

    return lines.length > 0 ? lines.join("\n\n") : "Nothing was skipped.";
}

async function newCode() {
    const bundle = {
        format: FORMAT,
        version: 1,
        createdAt: new Date().toISOString(),
        entries: kept().map(item => ({ key: item.key, raw: item.raw })),
    };

    return { bundle, code: await encodeCode(JSON.stringify(bundle)) };
}

async function copy() {
    const made = await newCode();

    try {
        await navigator.clipboard.writeText(made.code);

        ui.code.hidden = true;
        say(ui.out, "Copied. Paste it into vMenu Enhanced under Misc Settings, Import & Export.", "good");
    } catch {
        ui.code.hidden = false;
        ui.code.value = made.code;
        ui.code.focus();
        ui.code.select();

        say(ui.out, "Copying was blocked, so the code is in the box above. Press Ctrl+C to copy it.", "bad");
    }
}

async function download() {
    say(ui.out, "Packing your zip.", null);

    const made = await newCode();
    const blob = makeZip([
        { name: "vmenu-code.txt", text: made.code },
        { name: "vmenu-code-readable.json", text: JSON.stringify(made.bundle, null, 2) },
        { name: "skipped.txt", text: skippedReport() },
        { name: "read-me-first.txt", text: HELP },
    ]);

    const link = el("a");
    const url = URL.createObjectURL(blob);

    link.href = url;
    link.download = "vmenu-old-import-" + new Date().toISOString().slice(0, 10) + ".zip";

    document.body.append(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(url);

    say(ui.out, "Downloaded. The zip holds your code and a list of what was skipped.", "good");
}

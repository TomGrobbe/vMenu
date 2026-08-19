"use strict";

(() => {
    const RESOURCE = typeof GetParentResourceName === "function" ? GetParentResourceName() : "vMenu.Enhanced";

    const GZIP_PREFIX = "VME1G:";
    const PLAIN_PREFIX = "VME1P:";

    /* Well under the 64K where a buffer limit would sit, on a path nothing here has ever exercised. */
    const CHUNK_LENGTH = 32768;

    const root = document.getElementById("transfer");

    let headEl = null;
    let summaryEl = null;
    let warningEl = null;
    let codeEl = null;
    let statusEl = null;
    let hintEl = null;
    let actionEl = null;
    let closeEl = null;

    let text = {};
    let token = 0;
    let mode = "export";
    let incoming = null;
    let arrived = 0;

    /*
       Must be valid JSON: the body is parsed before anything is dispatched. Nothing answers these,
       and each one holds a connection until it gives up, so the wait is short. The body is gone the
       moment fetch is called, so giving up on a reply that is never coming cannot unsend it.
    */
    function post(callback, value) {
        return fetch(`https://${RESOURCE}/${callback}`, {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=UTF-8" },
            body: JSON.stringify(value),
            signal: AbortSignal.timeout(400)
        }).catch(error => {
            if (error.name !== "AbortError" && error.name !== "TimeoutError") {
                say(`POST https://${RESOURCE}/${callback} failed: ${error}`, "bad");
            }
        });
    }

    function build() {
        if (headEl) {
            return;
        }

        const panel = document.createElement("div");
        panel.className = "panel";

        headEl = document.createElement("div");
        headEl.className = "head";

        const body = document.createElement("div");
        body.className = "body";

        summaryEl = document.createElement("div");
        summaryEl.className = "summary";

        warningEl = document.createElement("div");
        warningEl.className = "warning";

        codeEl = document.createElement("textarea");
        codeEl.className = "code";
        codeEl.spellcheck = false;
        codeEl.autocomplete = "off";

        statusEl = document.createElement("div");
        statusEl.className = "status";

        body.append(summaryEl, warningEl, codeEl, statusEl);

        const foot = document.createElement("div");
        foot.className = "foot";

        hintEl = document.createElement("div");
        hintEl.className = "hint";

        const buttons = document.createElement("div");
        buttons.className = "buttons";

        actionEl = document.createElement("button");
        actionEl.className = "primary";
        actionEl.addEventListener("click", act);

        closeEl = document.createElement("button");
        closeEl.addEventListener("click", close);

        buttons.append(actionEl, closeEl);
        foot.append(hintEl, buttons);

        panel.append(headEl, body, foot);
        root.appendChild(panel);
    }

    function say(message, tone) {
        statusEl.textContent = message || "";
        statusEl.className = tone ? `status ${tone}` : "status";
    }

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

    /*
       Stamped here rather than on the client, whose runtime has no working clock: DateTime dies in
       its own static constructor looking up leap seconds. Wrapped in a try because a code the
       client can read matters more than knowing when it was made.
    */
    function stamped(plain) {
        try {
            const bundle = JSON.parse(plain);

            bundle.createdAt = new Date().toISOString();

            return JSON.stringify(bundle);
        } catch {
            return plain;
        }
    }

    async function encode(plain) {
        const bytes = new TextEncoder().encode(plain);

        if (typeof CompressionStream === "function") {
            try {
                return GZIP_PREFIX + toBase64(await through(bytes, new CompressionStream("gzip")));
            } catch {
                /* Older builds without the stream still get a code, just a much longer one. */
            }
        }

        return PLAIN_PREFIX + toBase64(bytes);
    }

    async function decode(code) {
        /* Stripped first: a code copied out of a chat message comes back wrapped in line breaks. */
        const cleaned = code.replace(/\s+/g, "");

        if (cleaned.length === 0) {
            return { error: "empty" };
        }

        const gzipped = cleaned.startsWith(GZIP_PREFIX);

        if (!gzipped && !cleaned.startsWith(PLAIN_PREFIX)) {
            return { error: "prefix" };
        }

        let bytes;

        try {
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

        return plain.length === 0 ? { error: "empty" } : { plain };
    }

    /* Never between a surrogate pair: half of one is not valid UTF-8 once it crosses to the client. */
    function slices(plain) {
        const parts = [];
        let at = 0;

        while (at < plain.length) {
            let end = Math.min(at + CHUNK_LENGTH, plain.length);

            if (end < plain.length && plain.charCodeAt(end - 1) >= 0xd800 && plain.charCodeAt(end - 1) <= 0xdbff) {
                end--;
            }

            parts.push(plain.slice(at, end));
            at = end;
        }

        return parts;
    }

    function reasonText(error) {
        if (error === "empty") {
            return text.emptyCode;
        }

        return error === "prefix" ? text.notACode : text.badCode;
    }

    async function act() {
        if (mode === "export") {
            copy();
            return;
        }

        say(text.working, null);

        const result = await decode(codeEl.value);

        if (result.error) {
            say(reasonText(result.error), "bad");
            post("vMenuTransferFailed", { token: token, reason: result.error });

            return;
        }

        const parts = slices(result.plain);

        for (let index = 0; index < parts.length; index++) {
            post("vMenuTransferImport", { token: token, index: index, count: parts.length, text: parts[index] });
        }
    }

    function copy() {
        codeEl.focus();
        codeEl.select();

        if (navigator.clipboard && typeof navigator.clipboard.writeText === "function") {
            navigator.clipboard.writeText(codeEl.value)
                .then(() => say(text.copied, "good"))
                .catch(() => fallbackCopy());

            return;
        }

        fallbackCopy();
    }

    function fallbackCopy() {
        let copied = false;

        try {
            copied = document.execCommand("copy");
        } catch {
            copied = false;
        }

        say(copied ? text.copied : text.copyFailed, copied ? "good" : "bad");
    }

    function close() {
        const closing = token;

        hide();
        post("vMenuTransferClose", { token: closing });
    }

    function hide() {
        root.classList.remove("shown");
        codeEl.value = "";
        incoming = null;
        arrived = 0;
        say("", null);
    }

    async function receive(data) {
        if (!incoming || data.token !== token || data.index < 0 || data.index >= incoming.length) {
            return;
        }

        if (incoming[data.index] !== null) {
            return;
        }

        incoming[data.index] = data.text || "";
        arrived++;

        if (arrived < incoming.length) {
            return;
        }

        const joined = incoming.join("");

        incoming = null;

        codeEl.value = await encode(stamped(joined));
        codeEl.focus();
        codeEl.select();
        say("", null);
    }

    function open(data) {
        build();

        token = data.token || 0;
        mode = data.mode === "import" ? "import" : "export";
        text = data;

        const count = mode === "export" ? Math.max(data.chunks || 0, 0) : 0;

        incoming = count > 0 ? new Array(count).fill(null) : null;
        arrived = 0;

        headEl.textContent = data.title || "";
        summaryEl.textContent = data.summary || "";
        hintEl.textContent = data.hint || "";
        codeEl.placeholder = data.placeholder || "";
        codeEl.readOnly = mode === "export";
        codeEl.value = "";

        warningEl.textContent = data.warning || "";
        warningEl.classList.toggle("shown", Boolean(data.warning));

        actionEl.textContent = mode === "export" ? data.copy : data.confirm;
        closeEl.textContent = data.close || "";

        say(mode === "export" && count > 0 ? data.working : "", null);

        root.classList.add("shown");
        codeEl.focus();

        post("vMenuTransferReady", { token: token });
    }

    /* On the window rather than the box: the keys have to work whether or not it has focus. */
    window.addEventListener("keydown", event => {
        if (!root.classList.contains("shown")) {
            return;
        }

        if (event.key === "Escape") {
            event.preventDefault();
            close();

            return;
        }

        if (event.key === "Enter" && mode === "import") {
            event.preventDefault();
            act();
        }
    });

    window.addEventListener("message", event => {
        let data = event.data;

        if (typeof data === "string") {
            try {
                data = JSON.parse(data);
            } catch {
                return;
            }
        }

        if (!data || typeof data !== "object") {
            return;
        }

        if (data.type === "transfer_open") {
            open(data);
        } else if (data.type === "transfer_chunk") {
            receive(data);
        } else if (data.type === "transfer_close") {
            hide();
        }
    });
})();

"use strict";

(() => {
    const SOURCE = "vMenu Enhanced";
    const MAX_SHOWN = 3;
    const DEFAULT_DURATION = 8500;

    /* Long enough for `.leaving` to finish; the row is only removed once it has faded out. */
    const EXIT_MS = 200;

    const ICONS = {
        info: '<circle cx="12" cy="12" r="9"/><path d="M12 11.2v5"/><path d="M12 7.6h.01"/>',
        success: '<circle cx="12" cy="12" r="9"/><path d="M7.9 12.3l2.8 2.8 5.4-5.6"/>',
        warning: '<path d="M12 3.4l9.2 16.2H2.8z"/><path d="M12 9.4v4.4"/><path d="M12 16.9h.01"/>',
        error: '<circle cx="12" cy="12" r="9"/><path d="M9.2 9.2l5.6 5.6"/><path d="M14.8 9.2l-5.6 5.6"/>',
    };

    /* GTA's own single-letter text colours, matched to this panel rather than to the game's HUD. */
    const COLOURS = {
        r: "#ef6a60",
        g: "#58cf90",
        b: "#4d94f7",
        y: "#edb44b",
        o: "#e8894a",
        p: "#b57ae8",
        c: "#9b9b9b",
        m: "#6f6f6f",
        u: "#101010",
        w: "#ffffff",
    };

    const listEl = document.getElementById("toasts");
    const queued = [];

    let shown = 0;

    /*
       Written per message rather than per frame: the client only recalculates where the map ends
       when it has something to say, so this is the one moment the anchor can have changed.
    */
    function place(anchor) {
        if (!anchor || typeof anchor.left !== "number" || typeof anchor.bottom !== "number" || typeof anchor.width !== "number") {
            return;
        }

        // Flush with the minimap's own edges, so the two read as one column.
        listEl.style.left = `${(anchor.left * 100).toFixed(3)}%`;
        listEl.style.width = `${(anchor.width * 100).toFixed(3)}%`;
        listEl.style.bottom = `calc(${(anchor.bottom * 100).toFixed(3)}% + 0.5rem)`;
    }

    /*
       Translates GTA's text tokens to markup. The strings these come from are also drawn by MenuAPI
       as item descriptions, where the game renders the tokens itself, so they stay in the strings and
       are dealt with here instead. Anything not understood — a HUD colour name, a game placeholder —
       is dropped rather than printed as-is.
    */
    function markup(text) {
        const token = /~([a-z_0-9]*)~/gi;
        const fragment = document.createDocumentFragment();

        let colour = null;
        let bold = false;
        let at = 0;
        let match;

        const emit = value => {
            if (value.length === 0) {
                return;
            }

            if (!colour && !bold) {
                fragment.appendChild(document.createTextNode(value));
                return;
            }

            const span = document.createElement("span");
            span.textContent = value;

            if (colour) {
                span.style.color = colour;
            }

            if (bold) {
                span.style.fontWeight = "600";
            }

            fragment.appendChild(span);
        };

        while ((match = token.exec(text)) !== null) {
            emit(text.slice(at, match.index));
            at = token.lastIndex;

            const name = match[1].toLowerCase();

            if (name === "n") {
                fragment.appendChild(document.createElement("br"));
            } else if (name === "s") {
                colour = null;
                bold = false;
            } else if (name === "h") {
                bold = !bold;
            } else if (COLOURS[name]) {
                colour = COLOURS[name];
            }
        }

        emit(text.slice(at));

        return fragment;
    }

    // Built through a wrapper because innerHTML on a bare <svg> is not reliable.
    function iconFor(style) {
        const wrapper = document.createElement("div");

        wrapper.innerHTML = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${ICONS[style]}</svg>`;

        return wrapper.firstElementChild;
    }

    function dismiss(toast) {
        if (toast.classList.contains("leaving")) {
            return;
        }

        toast.classList.add("leaving");

        setTimeout(() => {
            toast.remove();
            shown--;

            const next = queued.shift();

            if (next) {
                show(next);
            }
        }, EXIT_MS);
    }

    function show(notification) {
        shown++;

        const toast = document.createElement("div");
        toast.className = `toast ${notification.style}`;

        const source = document.createElement("span");
        source.textContent = SOURCE;

        const bar = document.createElement("div");
        bar.className = "bar";
        bar.append(iconFor(notification.style), source);

        const body = document.createElement("div");
        body.className = "body";
        body.appendChild(markup(notification.text));

        const progress = document.createElement("span");
        progress.className = "progress";
        progress.style.animationDuration = `${notification.duration}ms`;

        toast.append(bar, body, progress);
        listEl.appendChild(toast);

        setTimeout(() => dismiss(toast), notification.duration);
    }

    function notify(data) {
        const style = ICONS[data.style] ? data.style : "info";
        const duration = data.duration > 0 ? data.duration : DEFAULT_DURATION;
        const text = String(data.text ?? "");

        if (text.length === 0) {
            return;
        }

        place(data.anchor);

        const notification = { style, duration, text };

        // Over the cap the message waits rather than being dropped, so nothing is silently lost.
        if (shown >= MAX_SHOWN) {
            queued.push(notification);
            return;
        }

        show(notification);
    }

    window.addEventListener("message", event => {
        let data = event.data;

        if (typeof data === "string") {
            try {
                data = JSON.parse(data);
            } catch {
                return;
            }
        }

        if (data && typeof data === "object" && data.type === "notify") {
            notify(data);
        }
    });
})();

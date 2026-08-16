"use strict";

(() => {
    const TITLE = "STAFF NEEDED";
    const DEFAULT_DURATION = 12000;

    /* Long enough for `.leaving` to finish; the banner is only removed once it has faded out. */
    const EXIT_MS = 200;

    /* Handed back to a banner the pause menu froze, so there is time to read it once the game returns. */
    const PAUSE_GRACE_MS = 2000;

    const WARNING_ICON = '<path d="M12 3.4l9.2 16.2H2.8z"/><path d="M12 9.4v4.4"/><path d="M12 16.9h.01"/>';

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

    const listEl = document.getElementById("staff-alerts");

    /* Keyed by the alert id, which is how the server closes one it has already sent. */
    const tracked = new Map();

    let paused = false;

    /*
       Translates GTA's text tokens to markup, the same way the notification stack does. The strings
       these come from are also drawn by MenuAPI, where the game renders the tokens itself, so they
       stay in the strings and are dealt with here instead. Anything not understood is dropped rather
       than printed as-is.
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
            } else if (Object.hasOwn(COLOURS, name)) {
                colour = COLOURS[name];
            }
        }

        emit(text.slice(at));

        return fragment;
    }

    // Built through a wrapper because innerHTML on a bare <svg> is not reliable. The only markup this
    // file ever parses, and it is a constant.
    function icon() {
        const wrapper = document.createElement("div");

        wrapper.innerHTML = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${WARNING_ICON}</svg>`;

        return wrapper.firstElementChild;
    }

    /*
       Starts a banner's countdown with `remaining` of its duration left to run. The bar's keyframe
       always spans the whole duration and is wound forward with a negative delay, so one coming back
       from a pause picks up at the width its remaining time deserves rather than snapping to full.
    */
    function run(entry, remaining) {
        clearTimeout(entry.timer);

        entry.remaining = remaining;
        entry.startedAt = Date.now();

        entry.progress.style.animationDuration = `${entry.duration}ms`;
        entry.progress.style.animationDelay = `${-(entry.duration - remaining)}ms`;
        entry.progress.style.animationPlayState = "running";

        entry.timer = setTimeout(() => dismiss(entry.id), remaining);
    }

    function freeze(entry) {
        clearTimeout(entry.timer);

        entry.remaining = Math.max(0, entry.remaining - (Date.now() - entry.startedAt));
        entry.progress.style.animationPlayState = "paused";
    }

    function thaw(entry) {
        run(entry, entry.remaining + PAUSE_GRACE_MS);
    }

    /* The pause state is shared with the notification stack, which already broadcasts it. */
    function setPaused(next) {
        if (next === paused) {
            return;
        }

        paused = next;
        listEl.classList.toggle("paused", paused);

        for (const entry of tracked.values()) {
            if (paused) {
                freeze(entry);
            } else {
                thaw(entry);
            }
        }
    }

    function dismiss(id) {
        const entry = tracked.get(id);

        if (!entry) {
            return;
        }

        clearTimeout(entry.timer);
        tracked.delete(id);

        entry.element.classList.add("leaving");

        setTimeout(() => entry.element.remove(), EXIT_MS);
    }

    function show(data) {
        const id = data.id;

        if (typeof id !== "number" || typeof data.text !== "string" || data.text.length === 0) {
            return;
        }

        /* The same alert arriving twice replaces itself rather than stacking up. */
        if (tracked.has(id)) {
            dismiss(id);
        }

        const duration = typeof data.duration === "number" && data.duration > 0
            ? data.duration
            : DEFAULT_DURATION;

        const element = document.createElement("div");
        element.className = "staff-alert";

        const bar = document.createElement("div");
        bar.className = "bar";
        bar.appendChild(icon());
        bar.appendChild(document.createTextNode(TITLE));

        const body = document.createElement("div");
        body.className = "body";
        body.appendChild(markup(data.text));

        const progress = document.createElement("div");
        progress.className = "progress";

        element.appendChild(bar);
        element.appendChild(body);
        element.appendChild(progress);

        listEl.appendChild(element);

        const entry = { id, element, progress, duration, remaining: duration, startedAt: Date.now(), timer: 0 };

        tracked.set(id, entry);

        if (paused) {
            entry.progress.style.animationDuration = `${duration}ms`;
            entry.progress.style.animationPlayState = "paused";
        } else {
            run(entry, duration);
        }
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

        if (!data || typeof data !== "object") {
            return;
        }

        if (data.type === "notify_pause") {
            setPaused(data.paused === true);

            return;
        }

        if (data.type === "staff_alert") {
            show(data);

            return;
        }

        if (data.type === "staff_alert_close") {
            dismiss(data.id);

            return;
        }

        /* Everything at once, for the staff member who cleared their own screen with /dismiss. */
        if (data.type === "staff_alert_clear") {
            for (const id of [...tracked.keys()]) {
                dismiss(id);
            }
        }
    });
})();

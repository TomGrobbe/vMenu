"use strict";

(() => {
    const SOURCE = "vMenu Enhanced";
    const MAX_SHOWN = 3;
    const DEFAULT_DURATION = 8500;

    /* Long enough for `.leaving` to finish; the row is only removed once it has faded out. */
    const EXIT_MS = 200;

    /* Handed back to a row the pause menu froze, so there is time to read it once the game returns. */
    const PAUSE_GRACE_MS = 2000;

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

    /* Rows on screen and rows still waiting, together, so a repeat can find either of them. */
    const tracked = new Map();
    const queued = [];

    let shown = 0;
    let paused = false;

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
            } else if (Object.hasOwn(COLOURS, name)) {
                colour = COLOURS[name];
            }
        }

        emit(text.slice(at));

        return fragment;
    }

    // Built through a wrapper because innerHTML on a bare <svg> is not reliable. The only markup this
    // file ever parses, and every piece of it is a constant from ICONS.
    function iconFor(style) {
        const wrapper = document.createElement("div");

        wrapper.innerHTML = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${ICONS[style]}</svg>`;

        return wrapper.firstElementChild;
    }

    /* Two messages are the same one only when every part the player can see matches. */
    // Joined on a character no message can contain, so two different messages cannot share a key.
    function keyFor(style, footer, text) {
        return [style, footer, text].join("\u0000");
    }

    // A CSS animation only plays again once the element has been through a layout without it.
    function replay(element) {
        element.style.animation = "none";
        void element.offsetWidth;
        element.style.animation = "";
    }

    function count(entry) {
        entry.badge.textContent = `x${entry.repeats}`;
        replay(entry.badge);
    }

    /*
       Starts, or restarts, a row's countdown with `remaining` of its duration left to run. The bar's
       keyframe always spans the whole duration and is wound forward with a negative delay, so a row
       coming back from a pause picks up at the width its remaining time deserves rather than
       snapping back to full.
    */
    function run(entry, remaining) {
        clearTimeout(entry.timer);

        entry.remaining = remaining;

        replay(entry.progress);
        entry.progress.style.animationDuration = `${entry.duration}ms`;
        entry.progress.style.animationDelay = `-${entry.duration - remaining}ms`;

        if (paused) {
            entry.progress.style.animationPlayState = "paused";
            entry.timer = 0;

            return;
        }

        entry.endsAt = performance.now() + remaining;
        entry.timer = setTimeout(() => dismiss(entry), remaining);
    }

    /* Keeps what is left of a row's time instead of spending it behind the pause menu. */
    function freeze(entry) {
        clearTimeout(entry.timer);

        entry.timer = 0;
        entry.remaining = Math.max(0, entry.endsAt - performance.now());
        entry.progress.style.animationPlayState = "paused";
    }

    /* Never past the time the row was given, so pausing over and over cannot keep it on screen. */
    function thaw(entry) {
        run(entry, Math.min(entry.remaining + PAUSE_GRACE_MS, entry.duration));
    }

    function setPaused(next) {
        if (next === paused) {
            return;
        }

        paused = next;

        // Blurred along with the world the pause menu draws over.
        listEl.classList.toggle("paused", paused);

        for (const entry of tracked.values()) {
            // A row still waiting its turn has no time to hold; it is started when it reaches the screen.
            if (!entry.toast) {
                continue;
            }

            if (paused) {
                freeze(entry);
            } else {
                thaw(entry);
            }
        }
    }

    function dismiss(entry) {
        clearTimeout(entry.timer);

        // Untracked before it has finished fading, so a repeat arriving now opens a fresh row rather
        // than reviving one that is already on its way out.
        if (tracked.get(entry.key) === entry) {
            tracked.delete(entry.key);
        }

        entry.toast.classList.add("leaving");

        setTimeout(() => {
            entry.toast.remove();
            shown--;

            const next = queued.shift();

            if (next) {
                show(next);
            }
        }, EXIT_MS);
    }

    function show(entry) {
        shown++;

        const toast = document.createElement("div");
        toast.className = `toast ${entry.style}`;

        const source = document.createElement("span");
        source.className = "source";
        source.textContent = SOURCE;

        const badge = document.createElement("span");
        badge.className = "count";

        const bar = document.createElement("div");
        bar.className = "bar";
        bar.append(iconFor(entry.style), source, badge);

        const body = document.createElement("div");
        body.className = "body";
        body.appendChild(markup(entry.text));

        const progress = document.createElement("span");
        progress.className = "progress";

        toast.append(bar, body);

        if (entry.footer) {
            const footer = document.createElement("div");
            footer.className = "footer";
            footer.textContent = `From: ${entry.footer}`;

            toast.appendChild(footer);
        }

        toast.appendChild(progress);
        listEl.appendChild(toast);

        entry.toast = toast;
        entry.badge = badge;
        entry.progress = progress;

        run(entry, entry.duration);

        // Carried over from the wait, where a row can pick up repeats before it ever reaches the screen.
        if (entry.repeats > 1) {
            badge.textContent = `x${entry.repeats}`;
        }
    }

    function notify(data) {
        // Own properties only. A style of "constructor" finds one on the prototype and would otherwise
        // pass for a real style, putting whatever that returns into the markup the icon is built from.
        const style = Object.hasOwn(ICONS, data.style) ? data.style : "info";
        const duration = data.duration > 0 ? data.duration : DEFAULT_DURATION;
        const text = String(data.text ?? "");
        const footer = String(data.footer ?? "");

        if (text.length === 0) {
            return;
        }

        place(data.anchor);

        const key = keyFor(style, footer, text);
        const repeat = tracked.get(key);

        if (repeat) {
            repeat.repeats++;

            // Nothing to redraw while it is still waiting; it is built with the count it has by then.
            if (repeat.toast) {
                count(repeat);

                // Back on a full timer, so a repeat keeps the message on screen instead of ending it sooner.
                run(repeat, repeat.duration);
            }

            return;
        }

        const entry = {
            key,
            style,
            duration,
            text,
            footer,
            repeats: 1,
            toast: null,
            badge: null,
            progress: null,
            timer: 0,
            remaining: duration,
            endsAt: 0,
        };

        tracked.set(key, entry);

        // Over the cap the message waits rather than being dropped, so nothing is silently lost.
        if (shown >= MAX_SHOWN) {
            queued.push(entry);
            return;
        }

        show(entry);
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

        if (data.type === "notify") {
            // Carried on the message itself, so one arriving during a pause is frozen from the start.
            setPaused(data.paused === true);

            notify(data);
        }
    });
})();

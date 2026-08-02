"use strict";

(() => {
    const RESOURCE = typeof GetParentResourceName === "function" ? GetParentResourceName() : "vMenu.Enhanced";
    const MAX_ROWS = 10;

    const ICONS = {
        car: '<path d="M4 14l1.9-4.6A2 2 0 017.8 8h8.4a2 2 0 011.9 1.4L20 14"/><path d="M2.5 14h19v3.5h-19z"/><circle cx="7" cy="17.5" r="1.6"/><circle cx="17" cy="17.5" r="1.6"/>',
        van: '<path d="M2.5 6.5h11.5v11H2.5z"/><path d="M14 10.5h3.6l3.9 3.4v3.6H14z"/><circle cx="7" cy="17.5" r="1.6"/><circle cx="17.6" cy="17.5" r="1.6"/>',
        truck: '<path d="M2 6h12.5v11.5H2z"/><path d="M14.5 9.5h4l3.5 3.5v4.5h-7.5z"/><circle cx="6.5" cy="17.5" r="1.7"/><circle cx="18" cy="17.5" r="1.7"/>',
        motorcycle: '<circle cx="5.6" cy="16.4" r="3.4" stroke-width="2.6"/><circle cx="18.4" cy="16.4" r="3.4" stroke-width="2.6"/><path d="M5.6 16.4l2.8-4.2h5.6l3.4 2.6"/><path d="M8.4 12.2l1.4-2.6h4.4l.8 2.6z"/><path d="M14.6 9.6l3 2.2"/>',
        bicycle: '<circle cx="5.6" cy="16.4" r="3.9" stroke-width="1.1"/><circle cx="18.4" cy="16.4" r="3.9" stroke-width="1.1"/><path d="M5.6 16.4l4.6-7h4.2l3.9 7"/><path d="M10.2 9.4l4.2 7"/><path d="M8.6 9.4h3.2"/>',
        quad: '<path d="M7.6 9.6h8.8l1.2 5.4H6.4z"/><path d="M9.6 9.6V7.2h4.8v2.4"/><ellipse cx="4.6" cy="15.8" rx="2.4" ry="3.4" stroke-width="2.4"/><ellipse cx="19.4" cy="15.8" rx="2.4" ry="3.4" stroke-width="2.4"/><path d="M6.8 15h10.4"/>',
        boat: '<path d="M3 15.5h18l-2.4 4.2H5.4z"/><path d="M12 3.5v10"/><path d="M12.8 5.5l5.4 5.2h-5.4"/>',
        heli: '<path d="M3.5 5.5h17"/><path d="M12 5.5v3.2"/><path d="M7 8.7h6.6a4 4 0 014 4v2.4H7z"/><path d="M14.4 11.8h6.4"/><path d="M6 18.5h9.4"/>',
        plane: '<path d="M12 2.8l1.9 6.9 7.6 3v2.1l-7.6-1.1-.9 4.8 2.7 1.9v1.3L12 20.6l-3.7 1.1v-1.3l2.7-1.9-.9-4.8-7.6 1.1v-2.1l7.6-3z"/>',
        train: '<path d="M6 3.5h12v11H6z"/><path d="M9 6.5h6v4H9z"/><path d="M8 14.5l-2 4.5M16 14.5l2 4.5"/><circle cx="9.2" cy="17" r="1.2"/><circle cx="14.8" cy="17" r="1.2"/>',
        submarine: '<ellipse cx="10.5" cy="14" rx="7.5" ry="4.2"/><path d="M10.5 9.8V6.5h2.4v3.3"/><path d="M18 12.2l3.6-2.4v8.4L18 15.8"/>',
    };

    const scrim = document.getElementById("scrim");
    const titleEl = document.getElementById("title");
    const inputEl = document.getElementById("input");
    const listEl = document.getElementById("suggestions");
    const footerEl = document.getElementById("footer");

    let suggestions = [];
    let matches = [];
    let active = -1;
    let pressed = null;
    let noMatchesText = "";

    /* Must be valid JSON: the body is parsed before anything is dispatched. Nothing answers
       these, so they are dropped after a moment, or they would use up every connection the
       browser allows per host. */
    function post(callback, value) {
        return fetch(`https://${RESOURCE}/${callback}`, {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=UTF-8" },
            body: JSON.stringify(value === undefined || value === null ? "" : String(value)),
            signal: AbortSignal.timeout(2000)
        }).catch(error => {
            if (error.name !== "AbortError" && error.name !== "TimeoutError") {
                footerEl.textContent = `POST https://${RESOURCE}/${callback} failed: ${error}`;
            }
        });
    }

    /* Levenshtein distance, abandoned as -1 once it cannot come in under `max`. */
    function editDistance(a, b, max) {
        if (Math.abs(a.length - b.length) > max) {
            return -1;
        }

        let previous = new Array(b.length + 1);
        let current = new Array(b.length + 1);

        for (let j = 0; j <= b.length; j++) {
            previous[j] = j;
        }

        for (let i = 1; i <= a.length; i++) {
            current[0] = i;
            let best = current[0];

            for (let j = 1; j <= b.length; j++) {
                const cost = a[i - 1] === b[j - 1] ? 0 : 1;
                current[j] = Math.min(current[j - 1] + 1, previous[j] + 1, previous[j - 1] + cost);
                best = Math.min(best, current[j]);
            }

            if (best > max) {
                return -1;
            }

            const swap = previous;
            previous = current;
            current = swap;
        }

        return previous[b.length] > max ? -1 : previous[b.length];
    }

    /* Characters in order but not adjacent, "ader" hits "adder". Scored by how far in the last
       one landed, so tight early matches rank first. */
    function subsequenceSpread(candidate, query) {
        let at = -1;

        for (const character of query) {
            at = candidate.indexOf(character, at + 1);

            if (at < 0) {
                return -1;
            }
        }

        return at - query.length + 1;
    }

    /* Guesses are only shown when nothing solid matched, or another letter can widen the list. */
    const SOLID_MATCH = 500;

    /* Bigger is better, -1 drops it. The tiers matter, not the numbers. */
    function score(candidate, query) {
        const value = candidate.toLowerCase();

        if (value === query) {
            return 1000;
        }

        if (value.startsWith(query)) {
            return 900 - value.length;
        }

        const at = value.indexOf(query);

        if (at >= 0) {
            return 800 - at;
        }

        const spread = subsequenceSpread(value, query);

        if (spread >= 0) {
            return Math.max(SOLID_MATCH, 600 - spread);
        }

        const distance = editDistance(value, query, query.length > 4 ? 2 : 1);

        return distance >= 0 ? 400 - distance * 50 : -1;
    }

    function iconFor(key) {
        const shape = ICONS[key];

        if (!shape) {
            return null;
        }

        // Built through a wrapper because innerHTML on a bare <svg> is not reliable.
        const wrapper = document.createElement("div");

        wrapper.innerHTML = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">${shape}</svg>`;

        return wrapper.firstElementChild;
    }

    function render() {
        listEl.innerHTML = "";
        let activeRow = null;

        if (matches.length === 0) {
            if (inputEl.value.trim().length > 0 && suggestions.length > 0 && noMatchesText) {
                const empty = document.createElement("li");
                empty.textContent = noMatchesText;
                empty.style.cursor = "default";
                empty.style.color = "var(--muted)";
                listEl.appendChild(empty);
            }

            return;
        }

        matches.forEach((match, index) => {
            const row = document.createElement("li");

            if (index === active) {
                row.classList.add("active");
                activeRow = row;
            }

            const icon = iconFor(match.i);

            if (icon) {
                row.appendChild(icon);
            }

            const names = document.createElement("span");
            names.className = "names";

            const model = document.createElement("span");
            model.className = "model";
            model.textContent = match.v;
            names.appendChild(model);

            if (match.d) {
                const vehicleClass = document.createElement("span");
                vehicleClass.className = "class";
                vehicleClass.textContent = match.d;
                names.appendChild(vehicleClass);
            }

            const label = document.createElement("span");
            label.className = "label";
            label.textContent = match.l;

            row.append(names, label);

            /* On release, not press: the menu's grace period starts when the button comes up. */
            row.addEventListener("mousedown", event => {
                event.preventDefault();
                pressed = match;
            });

            row.addEventListener("mouseup", event => {
                event.preventDefault();

                if (pressed === match) {
                    submit(match.v);
                }
            });

            listEl.appendChild(row);
        });

        /* The list is taller than the box it sits in, so the keyboard has to drag it along. */
        if (activeRow) {
            activeRow.scrollIntoView({ block: "nearest" });
        } else {
            listEl.scrollTop = 0;
        }
    }

    function refresh() {
        const query = inputEl.value.trim().toLowerCase();

        active = -1;

        if (query.length === 0 || suggestions.length === 0) {
            matches = [];
            render();
            return;
        }

        const scored = [];
        let solid = false;

        for (const suggestion of suggestions) {
            const best = Math.max(score(suggestion.v, query), score(suggestion.l, query));

            if (best > 0) {
                scored.push({ suggestion, best });
                solid = solid || best >= SOLID_MATCH;
            }
        }

        const kept = solid ? scored.filter(entry => entry.best >= SOLID_MATCH) : scored;

        kept.sort((left, right) => right.best - left.best || left.suggestion.v.localeCompare(right.suggestion.v));

        matches = kept.slice(0, MAX_ROWS).map(entry => entry.suggestion);

        render();
    }

    function move(delta) {
        if (matches.length === 0) {
            return;
        }

        // Index -1 is "nothing picked, submit what was typed", and the list wraps through it.
        const positions = matches.length + 1;

        active = ((active + 1 + delta) % positions + positions) % positions - 1;

        render();
    }

    function submit(value) {
        hide();
        post("vMenuPromptSubmit", value);
    }

    function cancel() {
        hide();
        post("vMenuPromptCancel", "");
    }

    function hide() {
        scrim.classList.remove("shown");
        inputEl.value = "";
        suggestions = [];
        matches = [];
        active = -1;
        pressed = null;
        render();
    }

    function open(data) {
        suggestions = Array.isArray(data.suggestions) ? data.suggestions : [];
        titleEl.textContent = data.title || "";
        footerEl.textContent = data.hint || "";
        noMatchesText = data.noMatches || "";
        inputEl.placeholder = data.placeholder || "";
        inputEl.maxLength = data.maxLength > 0 ? data.maxLength : 255;
        inputEl.value = data.value || "";

        scrim.classList.add("shown");
        inputEl.focus();
        inputEl.select();

        refresh();

        post("vMenuPromptReady", "");
    }

    inputEl.addEventListener("input", refresh);

    /* Clicking anywhere must not leave the box unfocused. */
    scrim.addEventListener("mousedown", event => {
        event.preventDefault();
        inputEl.focus();
    });

    window.addEventListener("mouseup", () => {
        pressed = null;
    });

    /* On the window rather than the box: the keys have to work whether or not it has focus. */
    window.addEventListener("keydown", event => {
        if (!scrim.classList.contains("shown")) {
            return;
        }

        switch (event.key) {
            case "Escape":
                event.preventDefault();
                cancel();
                break;
            case "Enter":
                event.preventDefault();
                submit(active >= 0 ? matches[active].v : inputEl.value.trim());
                break;
            case "ArrowDown":
                event.preventDefault();
                move(1);
                break;
            case "ArrowUp":
                event.preventDefault();
                move(-1);
                break;
            case "Tab":
                event.preventDefault();

                if (matches.length > 0) {
                    inputEl.value = matches[active >= 0 ? active : 0].v;
                    refresh();
                }

                break;
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

        if (data.type === "open") {
            open(data);
        } else if (data.type === "close") {
            hide();
        }
    });
})();

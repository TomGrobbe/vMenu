"use strict";

(() => {
    const boxEl = document.getElementById("forecast");

    const SVG = "http://www.w3.org/2000/svg";

    const CLOUD_MID = '<path d="M7.5 18h9.2a3.6 3.6 0 0 0 .3-7.2 5.2 5.2 0 0 0-9.9-1A3.6 3.6 0 0 0 7.5 18z"/>';

    const CLOUD_HIGH = '<path d="M7.5 15.5h9.2a3.5 3.5 0 0 0 .3-7 5.1 5.1 0 0 0-9.8-1 3.5 3.5 0 0 0 .3 8z"/>';

    const SUN_RAYS =
        '<path d="M12 2v2.2M12 19.8V22M2 12h2.2M19.8 12H22' +
        'M4.9 4.9l1.6 1.6M17.5 17.5l1.6 1.6M19.1 4.9l-1.6 1.6M6.5 17.5l-1.6 1.6"/>';

    const ICONS = {
        sunny: '<circle cx="12" cy="12" r="4.3"/>' + SUN_RAYS,
        clear: '<circle cx="12" cy="12" r="4.6"/><path d="M12 2.6v1.6M12 19.8v1.6M2.6 12h1.6M19.8 12h1.6"/>',
        clouds: '<circle cx="16.4" cy="7" r="2.8"/>' + CLOUD_MID,
        overcast: '<path d="M6 9.6a4.8 4.8 0 0 1 9-1.4"/>' + CLOUD_MID,
        clearing:
            '<circle cx="16.8" cy="6.8" r="2.8"/><path d="M16.8 1.8v1.4M21.8 6.8h-1.4M20.3 3.3l-1 1"/>' + CLOUD_MID,
        rain: CLOUD_HIGH + '<path d="M9.4 18.2l-.9 3M13 18.6l-1 3.4M16.6 18.2l-.9 3"/>',
        thunder: CLOUD_HIGH + '<path d="M13.6 17.6l-3.4 3.6h3l-1.2 2.4"/>',
        smog: '<circle cx="12" cy="7.4" r="3.4"/><path d="M3.6 14.4h16.8M5.6 18h12.8M8 21.4h8"/>',
        fog: '<path d="M3.6 8.6h16.8M3.6 12.4h16.8M5.6 16.2h14.8M8 20h8"/>',
        snow: CLOUD_HIGH + '<path d="M9.4 19.2h.01M12.8 21.4h.01M16.4 19.2h.01"/>',
        blizzard:
            CLOUD_HIGH +
            '<path d="M8.6 19.2h.01M12.2 21.4h.01M15.8 19.2h.01"/>' +
            '<path d="M2.4 9h3.2M2.4 12.4h2.2M18.6 20.6h3M20 22.8h2"/>',
        halloween: '<path d="M17.6 2.6a6.2 6.2 0 1 0 3.8 7.6 5 5 0 0 1-3.8-7.6z"/>' + CLOUD_MID,
    };

    function svg(paths) {
        const element = document.createElementNS(SVG, "svg");

        element.setAttribute("viewBox", "0 0 24 24");
        element.setAttribute("fill", "none");
        element.setAttribute("stroke", "currentColor");
        element.setAttribute("stroke-width", "1.5");
        element.setAttribute("stroke-linecap", "round");
        element.setAttribute("stroke-linejoin", "round");
        element.innerHTML = paths;

        return element;
    }

    function weatherIcon(name) {
        return svg(ICONS[name] || ICONS.clear);
    }

    function moonIcon(lit, waxing) {
        const fraction = Math.min(Math.max(lit, 0), 100) / 100;
        const shift = (waxing ? -1 : 1) * 18 * fraction;

        const element = svg(
            '<defs><clipPath id="moon-disc"><circle cx="12" cy="12" r="9"/></clipPath></defs>' +
                '<circle cx="12" cy="12" r="9" fill="rgba(242, 242, 242, 0.85)" stroke="none"/>' +
                '<circle cx="' +
                (12 + shift).toFixed(2) +
                '" cy="12" r="9" fill="rgba(12, 14, 19, 0.94)" stroke="none" clip-path="url(#moon-disc)"/>' +
                '<circle cx="12" cy="12" r="9" fill="none" stroke="rgba(255, 255, 255, 0.24)"/>'
        );

        element.setAttribute("stroke-width", "1");

        return element;
    }

    function duration(seconds) {
        const total = Math.max(0, Math.round(seconds));

        if (total < 60) {
            return `${total}s`;
        }

        if (total < 3600) {
            const minutes = Math.floor(total / 60);
            const rest = total % 60;

            return rest === 0 ? `${minutes}m` : `${minutes}m ${rest}s`;
        }

        const hours = Math.floor(total / 3600);
        const minutes = Math.floor((total % 3600) / 60);

        return minutes === 0 ? `${hours}h` : `${hours}h ${minutes}m`;
    }

    function element(tag, className, text) {
        const created = document.createElement(tag);

        created.className = className;

        if (text !== undefined) {
            created.textContent = text;
        }

        return created;
    }

    function head(data) {
        const row = element("div", "head");

        row.appendChild(element("span", "name", data.title));

        const moon = element("div", "moon");

        moon.appendChild(moonIcon(data.moonLit, data.moonWaxing));
        moon.appendChild(element("span", "phase", data.moonName));
        moon.appendChild(element("span", "lit", `${data.moonLit}%`));

        row.appendChild(moon);

        return row;
    }

    function now(data) {
        const row = element("div", "now");

        row.appendChild(weatherIcon(data.currentIcon));

        const text = element("div", "text");

        text.appendChild(element("span", "label", data.nowLabel));
        text.appendChild(element("span", "value", data.currentName));

        if (data.currentForSeconds >= 0) {
            text.appendChild(element("span", "remaining", duration(data.currentForSeconds)));
        }

        row.appendChild(text);

        return row;
    }

    function upcoming(data) {
        const list = element("div", "upcoming");

        list.appendChild(element("div", "label", data.nextLabel));

        for (const entry of data.upcoming) {
            const row = element("div", "row");

            row.appendChild(element("span", "when", duration(entry.inSeconds)));
            row.appendChild(weatherIcon(entry.icon));
            row.appendChild(element("span", "what", entry.name));
            row.appendChild(element("span", "lasts", duration(entry.forSeconds)));

            list.appendChild(row);
        }

        return list;
    }

    function render(data) {
        boxEl.textContent = "";
        boxEl.appendChild(head(data));
        boxEl.appendChild(now(data));

        if (data.note) {
            boxEl.appendChild(element("div", "note", data.note));
        }

        if (Array.isArray(data.upcoming) && data.upcoming.length > 0) {
            boxEl.appendChild(upcoming(data));
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

        if (!data || typeof data !== "object" || data.type !== "forecast") {
            return;
        }

        if (!data.visible) {
            boxEl.classList.remove("shown");
            boxEl.textContent = "";

            return;
        }

        render(data);

        boxEl.classList.add("shown");
    });
})();

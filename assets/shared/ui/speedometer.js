"use strict";

(() => {
    const boxEl = document.getElementById("speedometer");

    const RAIL_CEILING_KMH = 300;

    const BUTTONS_CLEARANCE_REM = 1.15;

    let placed = null;
    let watching = null;

    const HEALTH_BARS = [
        { key: "engine", label: "ENG" },
        { key: "body", label: "BDY" },
        { key: "tank", label: "TNK" },
    ];

    function readout(value, unit) {
        const element = document.createElement("div");
        element.className = "readout";

        const number = document.createElement("span");
        number.className = "value";
        number.textContent = String(value);

        const label = document.createElement("span");
        label.className = "unit";
        label.textContent = unit;

        element.append(number, label);

        return element;
    }

    function rail(fraction) {
        const track = document.createElement("div");
        track.className = "rail";

        const fill = document.createElement("span");
        fill.style.width = `${Math.min(Math.max(fraction, 0), 1) * 100}%`;

        track.appendChild(fill);

        return track;
    }

    function healthTone(percent) {
        if (percent > 75) {
            return "good";
        }

        if (percent > 50) {
            return "fair";
        }

        return percent > 25 ? "poor" : "bad";
    }

    function healthBar(label, percent) {
        const row = document.createElement("div");
        row.className = `health ${healthTone(percent)}`;

        const name = document.createElement("span");
        name.className = "health-label";
        name.textContent = label;

        const track = document.createElement("div");
        track.className = "health-rail";

        const fill = document.createElement("span");
        fill.style.width = `${Math.min(Math.max(percent, 0), 100)}%`;

        track.appendChild(fill);
        row.append(name, track);

        return row;
    }

    function renderSpeed(data) {
        const kmh = typeof data.kmh === "number" ? data.kmh : null;
        const mph = typeof data.mph === "number" ? data.mph : null;

        if (kmh === null && mph === null) {
            return false;
        }

        if (kmh !== null) {
            boxEl.appendChild(readout(kmh, "km/h"));
        }

        if (mph !== null) {
            boxEl.appendChild(readout(mph, "mph"));
        }

        const reference = kmh !== null ? kmh : mph * 1.609344;

        boxEl.appendChild(rail(reference / RAIL_CEILING_KMH));

        return true;
    }

    function renderHealth(data) {
        const bars = HEALTH_BARS
            .filter(bar => typeof data[bar.key] === "number")
            .map(bar => healthBar(bar.label, data[bar.key]));

        if (bars.length === 0) {
            return false;
        }

        const group = document.createElement("div");
        group.className = "health-group";

        bars.forEach(bar => group.appendChild(bar));
        boxEl.appendChild(group);

        return true;
    }

    // Either half can be switched off on its own, so the panel shows whichever of the two arrived.
    function render(data) {
        boxEl.textContent = "";

        const speed = renderSpeed(data);
        const health = renderHealth(data);

        return speed || health;
    }

    // The client only knows whether hints are up, not how many rows other resources stacked on top.
    // Measured on the outer bar because its rows slide into place, and a leaving row is already out of it.
    function buttonsArea() {
        const bar = document.getElementById("menuapi-buttons");

        if (!bar || bar.hidden) {
            return null;
        }

        const rect = bar.getBoundingClientRect();

        return rect.height > 0 ? rect : null;
    }

    function watchButtons() {
        if (watching) {
            return;
        }

        const bar = document.getElementById("menuapi-buttons");

        if (!bar) {
            return;
        }

        watching = new ResizeObserver(() => apply());
        watching.observe(bar);
    }

    function apply() {
        if (!placed) {
            return;
        }

        const centered = placed.side === "center";

        boxEl.classList.toggle("center", centered);
        boxEl.style.right = centered ? "" : `${(placed.right * 100).toFixed(3)}%`;

        let bottom = placed.bottom * window.innerHeight;
        const area = boxEl.classList.contains("shown") ? buttonsArea() : null;

        if (area) {
            const box = boxEl.getBoundingClientRect();

            if (box.left < area.right && box.right > area.left) {
                const rem = parseFloat(getComputedStyle(document.documentElement).fontSize) || 16;

                bottom = Math.max(bottom, window.innerHeight - area.top + rem * BUTTONS_CLEARANCE_REM);
            }
        }

        boxEl.style.bottom = `${((bottom / window.innerHeight) * 100).toFixed(3)}%`;
    }

    function place(side, right, bottom) {
        if (typeof right !== "number" || typeof bottom !== "number") {
            return;
        }

        placed = { side, right, bottom };

        watchButtons();
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

        if (!data || typeof data !== "object" || data.type !== "speedometer") {
            return;
        }

        if (!data.visible) {
            boxEl.classList.remove("shown");
            boxEl.textContent = "";

            return;
        }

        place(data.side, data.right, data.bottom);

        boxEl.classList.toggle("shown", render(data));

        apply();
    });
})();

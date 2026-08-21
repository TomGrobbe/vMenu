"use strict";

(() => {
    const boxEl = document.getElementById("speedometer");

    const RAIL_CEILING_KMH = 300;

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

    function render(data) {
        boxEl.textContent = "";

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

    function place(side, right, bottom) {
        if (typeof right !== "number" || typeof bottom !== "number") {
            return;
        }

        const centered = side === "center";

        boxEl.classList.toggle("center", centered);

        boxEl.style.bottom = `${(bottom * 100).toFixed(3)}%`;
        boxEl.style.right = centered ? "" : `${(right * 100).toFixed(3)}%`;
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
    });
})();

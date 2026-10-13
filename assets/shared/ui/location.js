"use strict";

(() => {
    const locationEl = document.getElementById("location");
    const coordinatesEl = document.getElementById("coordinates");

    function element(tag, className, text) {
        const created = document.createElement(tag);

        created.className = className;

        if (text !== undefined) {
            created.textContent = text;
        }

        return created;
    }

    function renderLocation(data) {
        locationEl.textContent = "";

        const street = element("div", "street");

        if (data.near && data.nearLabel) {
            street.appendChild(element("span", "near", data.nearLabel));
        }

        street.appendChild(element("span", "name", String(data.street ?? "")));
        locationEl.appendChild(street);

        if (data.crossing) {
            locationEl.appendChild(element("div", "crossing", `/ ${data.crossing}`));
        }

        const foot = element("div", "foot");

        foot.appendChild(element("span", "zone", String(data.zone ?? "")));
        foot.appendChild(element("span", "compass", String(data.compass ?? "")));
        locationEl.appendChild(foot);
    }

    function renderCoordinates(data) {
        coordinatesEl.textContent = "";

        const rows = [
            ["X", data.x],
            ["Y", data.y],
            ["Z", data.z],
            [data.headingLabel, data.heading],
        ];

        for (const [label, figure] of rows) {
            coordinatesEl.appendChild(element("span", "axis", String(label ?? "")));
            coordinatesEl.appendChild(element("span", "figure", String(figure ?? "")));
        }

        if (typeof data.inset === "number") {
            const inset = `${(data.inset * 100).toFixed(3)}%`;
            const onRight = data.side === "right";

            coordinatesEl.style.top = inset;
            coordinatesEl.style.left = onRight ? "auto" : inset;
            coordinatesEl.style.right = onRight ? inset : "auto";
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

        if (data.type === "location") {
            if (!data.visible) {
                locationEl.classList.remove("shown");
                locationEl.textContent = "";

                return;
            }

            renderLocation(data);
            locationEl.classList.add("shown");

            return;
        }

        if (data.type !== "coordinates") {
            return;
        }

        if (!data.visible) {
            coordinatesEl.classList.remove("shown");
            coordinatesEl.textContent = "";

            return;
        }

        renderCoordinates(data);
        coordinatesEl.classList.add("shown");
    });
})();

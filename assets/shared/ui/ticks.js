"use strict";

(() => {
    const listEl = document.getElementById("ticks");

    // Grouped by source rather than one flat list, because vMenu's loops and MenuAPI's are two
    // separate schedulers and a running count that mixed them would not say anything useful.
    function group(ticks) {
        const groups = new Map();

        for (const tick of ticks) {
            const source = String(tick.source ?? "vMenu");

            if (!groups.has(source)) {
                groups.set(source, []);
            }

            groups.get(source).push(tick);
        }

        // Sorted by name, because registration order is whatever the client happened to start first
        // and it changes as loops are added, so a name is the only way to find one twice in the same place.
        for (const rows of groups.values()) {
            rows.sort((left, right) => String(left.name ?? "").localeCompare(String(right.name ?? "")));
        }

        return groups;
    }

    function column(title) {
        const element = document.createElement("div");
        element.className = "group";

        const head = document.createElement("div");
        head.className = "head";
        head.textContent = title;

        element.appendChild(head);

        return element;
    }

    /*
       How much room a column has, taken from where the panel actually sits rather than from a copy of
       the offset in the stylesheet, with the same gap left at the bottom as the stylesheet leaves at
       the top.
    */
    function available() {
        return window.innerHeight - (listEl.getBoundingClientRect().top * 2);
    }

    /*
       Splits a column that is taller than the screen into as many columns as it takes. Measured on
       screen rather than counted, because how many rows fit depends on the resolution, and a long
       name wraps to a row taller than the rest.
    */
    function fit(first, source) {
        const rows = [...first.querySelectorAll(".row")];

        // Every height is read before anything moves, so this costs one layout pass rather than one per row.
        const heights = rows.map(row => row.offsetHeight);
        const room = available() - first.querySelector(".head").offsetHeight;

        let current = first;
        let used = 0;

        rows.forEach((row, index) => {
            // The used check keeps a row per column on a screen too short for even one, which is the
            // one case where nothing can be done and the column scrolls instead.
            if (used > 0 && used + heights[index] > room) {
                current = listEl.insertBefore(column(`${source} · continued`), current.nextSibling);
                used = 0;
            }

            if (row.parentElement !== current) {
                current.appendChild(row);
            }

            used += heights[index];
        });
    }

    function render(ticks) {
        listEl.textContent = "";

        const columns = [];

        for (const [source, rows] of group(ticks)) {
            const running = rows.filter(tick => tick.running).length;

            const element = column(`${source} · ${running}/${rows.length} running`);

            columns.push([element, source]);

            for (const tick of rows) {
                const row = document.createElement("div");
                row.className = tick.running ? "row running" : "row";

                const dot = document.createElement("span");
                dot.className = "dot";

                const name = document.createElement("span");
                name.className = "name";
                name.textContent = String(tick.name ?? "");

                const rate = document.createElement("span");
                rate.className = "rate";
                rate.textContent = String(tick.rate ?? "");

                row.append(dot, name, rate);
                element.appendChild(row);
            }

            listEl.appendChild(element);
        }

        // After every column is on screen, because splitting one needs heights that only exist once
        // the browser has laid it out.
        for (const [element, source] of columns) {
            fit(element, source);
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

        if (!data || typeof data !== "object" || data.type !== "ticks") {
            return;
        }

        if (!data.visible) {
            listEl.classList.remove("shown");
            listEl.textContent = "";

            return;
        }

        // Shown before the rows are built, not after: a hidden panel measures zero, and the columns
        // are split from what they measure.
        listEl.classList.toggle("left", data.side === "left");
        listEl.classList.toggle("paused", data.paused === true);
        listEl.classList.add("shown");

        render(Array.isArray(data.ticks) ? data.ticks : []);
    });
})();

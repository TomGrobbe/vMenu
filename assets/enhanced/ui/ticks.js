"use strict";

(() => {
    const listEl = document.getElementById("ticks");

    function render(ticks) {
        listEl.textContent = "";

        const running = ticks.filter(tick => tick.running).length;

        const head = document.createElement("div");
        head.className = "head";
        head.textContent = `Ticks · ${running}/${ticks.length} running`;

        listEl.appendChild(head);

        for (const tick of ticks) {
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
            listEl.appendChild(row);
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

        render(Array.isArray(data.ticks) ? data.ticks : []);

        listEl.classList.toggle("left", data.side === "left");
        listEl.classList.add("shown");
    });
})();

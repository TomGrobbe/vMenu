"use strict";

(() => {
    const boxEl = document.getElementById("hud-left");
    const infoEl = document.getElementById("hud-info");

    function syncInfo() {
        let any = false;

        for (const child of infoEl.children) {
            if (child.classList.contains("shown")) {
                any = true;

                break;
            }
        }

        infoEl.classList.toggle("shown", any);
    }

    const watcher = new MutationObserver(syncInfo);

    for (const child of infoEl.children) {
        watcher.observe(child, { attributes: true, attributeFilter: ["class"] });
    }

    syncInfo();

    function place(anchor) {
        if (!anchor || typeof anchor.left !== "number" || typeof anchor.bottom !== "number" || typeof anchor.width !== "number") {
            return;
        }

        // Flush with the minimap's own edges, so the two read as one column.
        boxEl.style.left = `${(anchor.left * 100).toFixed(3)}%`;
        boxEl.style.width = `${(anchor.width * 100).toFixed(3)}%`;
        boxEl.style.bottom = `calc(${(anchor.bottom * 100).toFixed(3)}% + 0.5rem)`;
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

        if (data && typeof data === "object" && data.anchor) {
            place(data.anchor);
        }
    });
})();

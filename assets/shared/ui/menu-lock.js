"use strict";

(() => {
    const GAP_PX = 6;

    const ICON = '<rect x="5" y="10.5" width="14" height="10" rx="2"/><path d="M8 10.5V7.5a4 4 0 0 1 8 0v3"/>';

    const root = document.getElementById("menu-lock");
    const menuEl = document.getElementById("menuapi");

    let titleEl = null;
    let bodyEl = null;
    let acceptEl = null;
    let cancelEl = null;
    let fillEl = null;
    let following = false;

    function build() {
        if (titleEl) {
            return;
        }

        const bar = document.createElement("div");
        bar.className = "bar";

        // Built through a wrapper because innerHTML on a bare <svg> is not reliable. ICON is a constant.
        const wrapper = document.createElement("div");
        wrapper.innerHTML = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${ICON}</svg>`;

        titleEl = document.createElement("span");
        bar.append(wrapper.firstElementChild, titleEl);

        bodyEl = document.createElement("div");
        bodyEl.className = "body";

        const actions = document.createElement("div");
        actions.className = "actions";

        acceptEl = document.createElement("span");
        acceptEl.className = "action danger";

        cancelEl = document.createElement("span");
        cancelEl.className = "action";

        actions.append(acceptEl, cancelEl);

        const track = document.createElement("div");
        track.className = "track";

        fillEl = document.createElement("div");
        fillEl.className = "fill";
        track.append(fillEl);

        root.append(bar, bodyEl, actions, track);
    }

    // The sentence comes translated with a {key} slot, so the key lands wherever the language puts it.
    function sentence(node, template, keyName) {
        const key = document.createElement("span");
        key.className = "key";
        key.textContent = keyName || "";

        const at = (template || "").indexOf("{key}");

        if (at < 0) {
            node.replaceChildren(key, document.createTextNode(` ${template || ""}`));

            return;
        }

        node.replaceChildren(
            document.createTextNode(template.slice(0, at)),
            key,
            document.createTextNode(template.slice(at + 5)));
    }

    // The menu grows and shrinks with its description, so the card is moved along with it every frame.
    function follow() {
        if (!following) {
            return;
        }

        if (!menuEl.hidden) {
            const rect = menuEl.getBoundingClientRect();

            root.style.left = `${rect.left}px`;
            root.style.top = `${rect.bottom + GAP_PX}px`;
            root.style.width = `${rect.width}px`;
        }

        requestAnimationFrame(follow);
    }

    // A CSS animation only plays again once the element has been through a layout without it.
    function restart(durationMs) {
        fillEl.style.setProperty("--duration", `${durationMs}ms`);
        fillEl.style.animation = "none";
        void fillEl.offsetWidth;
        fillEl.style.animation = "";
    }

    function show(data) {
        build();

        root.classList.toggle("holding", data.state === "holding");
        root.classList.toggle("confirm", data.state === "confirm");

        titleEl.textContent = data.title || "";
        bodyEl.textContent = data.text || "";
        sentence(acceptEl, data.accept, data.acceptKey);
        sentence(cancelEl, data.cancel, data.cancelKey);

        const radius = getComputedStyle(menuEl).getPropertyValue("--menuapi-radius").trim();

        if (radius) {
            root.style.setProperty("--radius", radius);
        }

        restart(typeof data.durationMs === "number" ? data.durationMs : 5000);

        if (!root.classList.contains("shown")) {
            root.classList.add("shown");
        }

        if (!following) {
            following = true;
            follow();
        }
    }

    function hide() {
        following = false;
        root.classList.remove("shown", "holding", "confirm");
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

        if (!data || typeof data !== "object" || data.type !== "menuLock") {
            return;
        }

        if (data.state === "holding" || data.state === "confirm") {
            show(data);
        } else {
            hide();
        }
    });
})();

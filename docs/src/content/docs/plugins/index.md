---
title: "Plugins"
description: "What vMenu Enhanced plugins are, what they look like in game, and where to go next as a player, a server owner or a developer."
---

A plugin is a **separate resource that adds its own menus inside vMenu**. It lives in its own folder in your server's resources, and vMenu does not need to know it exists beforehand. When the plugin starts it introduces itself, hands over a description of the menu it would like, and vMenu draws that menu inside itself.

Nothing about a plugin involves editing vMenu. You do not touch the source, you do not rebuild it, and updating vMenu does not undo anything a plugin did.

## What a player sees

Plugins show up in two places:

- **A "Plugins" entry on the main menu**, near the bottom. One row per plugin, and underneath each row is that plugin's own menu. It is hidden entirely when a server runs no plugins.
- **A "Plugin Actions" entry inside a player's page in Online Players**, for plugins that add things you can do to another player.

Everything a plugin adds uses vMenu's own permissions system. A missing or locked row means the server owner has not given you that permission. A plugin cannot hand itself permissions, and it cannot give you access to anything of vMenu itself.

## Where to go from here

- **Installing one?** See [Installing plugins](/vmenu/enhanced/plugins/installing/).
- **Writing one?** See [Making a plugin](/vmenu/enhanced/plugins/developing/). There is a complete working example repository to copy from, and two NuGet packages that do the talking for you. Only C# is officially supported for now. Lua and JS plugins are possible in theory, but there is no support or guidance for them yet.

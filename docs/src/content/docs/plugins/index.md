---
title: "Plugins"
description: "What vMenu Enhanced plugins are, what they look like in game, and where to go next as a player, a server owner or a developer."
---

A plugin is a **separate resource that adds its own menus inside vMenu**. It is written by somebody else, it lives in its own folder in your server's resources, and vMenu does not need to know it exists beforehand. When the plugin starts it introduces itself to vMenu, hands over a description of the menu it would like, and vMenu draws that menu inside itself.

Nothing about a plugin involves editing vMenu. You do not edit the source code, you do not rebuild it, and updating vMenu does not undo anything a plugin did.

## What a player sees

If a server you play on runs plugins, they show up in two places.

**A "Plugins" entry on vMenu's main menu (near the bottom).** Open it and you get one row per plugin, named by whoever wrote it. Underneath each row is that plugin's own menu, with whatever it decided to put in there. Buttons, checkboxes, sliders, submenus, the same kinds of rows vMenu itself uses.

**A "Plugin Actions" entry inside a player's page in Online Players.** Some plugins add things you can do to another player, and those appear there.

The Plugins entry is hidden entirely when a server runs no plugins, so if you never see it, there are none.

Everything a plugin adds uses the same permissions system as vMenu. If a row is missing or greyed out with a lock on it, the server owner has not given you that permission, exactly as with vMenu's own features. A plugin cannot hand itself permissions, and it cannot give you access to anything of vMenu itself.

## Where to go from here

**Running a server and want to install one?** See [Installing plugins](/vmenu/enhanced/plugins/installing/). It covers where the folder goes, what the start order needs to be (nothing, as it turns out), and how the permissions and settings a plugin brings reach you.

**Writing one?** See [Making a plugin](/vmenu/enhanced/plugins/developing/). There is a complete working example repository to copy from, and two NuGet packages that do the talking for you. Right now we only officially support C# plugins, however this will change in the future. In theory you can already create your own Lua and JS plugins for vMenu, but there is no support or guidance for this at this point.

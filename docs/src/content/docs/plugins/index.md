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

## Official plugins

These are made and kept up to date alongside vMenu itself. Each one is a real plugin you can install
as it is, and each is also worth reading as an example of how a plugin is put together.

| Plugin | What it does |
| --- | --- |
| [Routing Buckets](https://github.com/TomGrobbe/vMenu.RoutingBucketsPlugin) | Allows you to manage routing buckets on your server. |
| [Theme Picker](https://github.com/TomGrobbe/vMenu.ThemePicker) | Let's each player choose a theme for vMenu themselves, rather than being forced to use the server theme. |
| [Custom Themes](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin) | Allows you to easily add new custom themes to vMenu. |
| [Example Plugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin) | Example plugin, do not use on public servers, use this as a tempalte if you want to make your own plugins. |

Every one of them is GPL-3.0-or-later, same as vMenu, and each publishes a ready to drop in zip on its
own releases page.

## Where to go from here

- **Installing one?** See [Installing plugins](/vmenu/enhanced/plugins/installing/).
- **Writing one?** See [Making a plugin](/vmenu/enhanced/plugins/developing/). There are the three plugins above to copy from, and two NuGet packages that do the talking for you. C# is the supported way to build a menu. A resource in another language can still register themes, which is a single event and what the Custom Themes plugin does, but menus are C# for now.

---
title: "Installing on FiveM Legacy"
description: "How to install vMenu Enhanced on a regular FiveM server, the one built on the original GTA V, with a txAdmin recipe or by hand."
---

This page is for the regular FiveM, built on the original GTA V, which we call FiveM Legacy. Running FiveM Enhanced instead? Then you want [Installing on FiveM Enhanced](/vmenu/enhanced/installing/fivem-enhanced/). vMenu Enhanced has exactly the same menus and features on both.

## Before you start

- **This is not the original vMenu.** It has its own permission names and does not read anything the original vMenu saved. Never run both at the same time. The original vMenu, v3.9.2 and older, is no longer updated, but its docs are still [here](/vmenu/legacy/).
- **Keep your server artifacts up to date.** vMenu sends things like update checks and webhooks through a small Node.js helper, and that needs the Node.js version 22 that current artifacts come with. If your console says the net bridge never answered, update your artifacts.

## The quick way, with txAdmin

txAdmin is the control panel that comes with every FiveM server. Its "recipes" set up a whole new server for you. The vMenu recipe gives you vMenu Enhanced, ready made permissions and settings, and the [routing buckets](https://github.com/TomGrobbe/vMenu.RoutingBucketsPlugin), [theme picker](https://github.com/TomGrobbe/vMenu.ThemePicker) and [custom themes](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin) plugins. The admin you set up in txAdmin can use the whole menu right away.

Pick the vMenu recipe in txAdmin's setup screen. If it is not listed, choose "Remote URL Template" and paste this link, which always points to the newest release:

```
https://github.com/TomGrobbe/vMenu/releases/latest/download/txadmin-vmenu-legacy-recipe.yaml
```

That's it, you can skip the rest of this page. The `server.cfg` txAdmin makes for you explains how to set up the plugins' permissions.

## The manual way

### 1. Download and unpack it

Download `vMenu-Enhanced-Legacy-v<version>.zip` from the [latest release](https://github.com/TomGrobbe/vMenu/releases/latest) and unpack it into your `resources` folder. Only the zip with "Legacy" in its name works here. The other one refuses to start and tells you so in the console.

:::danger[The folder name matters]
Name the folder exactly `vMenu.Enhanced`, capital letters and all, even though your server is not FiveM Enhanced. Every permission and setting name is built from it, and vMenu refuses to start under any other name.
:::

### 2. Add the first lines to your server.cfg

```ini
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

The two `exec` lines load your permissions and settings, which you make in the next step. The `add_filesystem_permission` line lets vMenu write into its own folder. All three have to come before `ensure`, otherwise nobody can open the menu and vMenu cannot save anything.

Using an [integration](/vmenu/enhanced/integrations/) with role sync, where your integration decides who is in which permission group? Then vMenu also needs these two lines, which the example `permissions.cfg` already has at the top:

```ini
add_ace resource.vMenu.Enhanced command.add_principal allow
add_ace resource.vMenu.Enhanced command.remove_principal allow
```

### 3. Make your permissions and settings files

Open `resources/vMenu.Enhanced/config/`. Copy `permissions.cfg.example` and `configuration.cfg.example`, and remove `.example` from the names of the copies. Then edit the copies:

- `permissions.cfg` decides who may use which part of the menu. It uses "ace" permissions, which is FiveM's way of saying "this player or group is allowed to do this".
- `configuration.cfg` decides how vMenu behaves and what is turned on.

vMenu never changes your copies. The `.example` files are rewritten every time vMenu starts, so they always list every permission and setting there is. Keeping your copies up to date is up to you: after every vMenu update, check the changelog or compare your copies with the `.example` files, and add any new permissions or settings you want yourself.

Just playing with friends? This one line in `permissions.cfg` gets you going:

```ini
add_ace builtin.everyone "vMenu.Enhanced.Everything" allow
```

:::danger[This gives everyone everything]
Only use that line on a whitelisted server that random people cannot join.
:::

### You're done

Restart your server, join, and press `M` to open the menu. See [Key Bindings](/vmenu/enhanced/key-bindings/) for everything else vMenu puts on a key.

## Plugins on FiveM Legacy

Plugins written in Lua, JavaScript or TypeScript work on both versions, including every official plugin. A C# plugin needs a build made for FiveM Legacy, see [C# setup](/vmenu/enhanced/plugins/csharp/).

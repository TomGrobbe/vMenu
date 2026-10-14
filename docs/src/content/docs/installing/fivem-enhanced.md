---
title: "Installing on FiveM Enhanced"
description: "How to install vMenu Enhanced on a FiveM Enhanced server, with a txAdmin recipe or by hand."
---

This page is for **FiveM Enhanced**, the version of FiveM built on GTA V Enhanced. Running the regular FiveM instead? Then you want [Installing on FiveM Legacy](/vmenu/enhanced/installing/fivem-legacy/).

## The quick way, with txAdmin

txAdmin is the control panel that comes with every FiveM server. Its "recipes" set up a whole new server for you. The vMenu Enhanced recipe gives you vMenu Enhanced, ready made permissions and settings, and the [routing buckets](https://github.com/TomGrobbe/vMenu.RoutingBucketsPlugin), [theme picker](https://github.com/TomGrobbe/vMenu.ThemePicker) and [custom themes](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin) plugins. The admin you set up in txAdmin can use the whole menu right away. FiveM Legacy has [its own recipe](/vmenu/enhanced/installing/fivem-legacy/#the-quick-way-with-txadmin), and the original vMenu no longer has one.

Pick the vMenu Enhanced recipe in txAdmin's setup screen. If it is not listed, choose "Remote URL Template" and paste this link, which always points to the newest release:

```
https://github.com/TomGrobbe/vMenu/releases/latest/download/txadmin-vmenu-enhanced-recipe.yaml
```

That's it, you can skip the rest of this page. The `server.cfg` txAdmin makes for you explains how to set up the plugins' permissions.

## The manual way

### 1. Download and unpack it

Download `vMenu-Enhanced-v<version>.zip` from the [latest release](https://github.com/TomGrobbe/vMenu/releases/latest) and unpack it into your `resources` folder. Leave the zip with "Legacy" in its name alone, that one is for regular FiveM servers.

:::danger[The folder name matters]
Name the folder exactly `vMenu.Enhanced`, capital letters and all. Every permission and setting name is built from it, and vMenu refuses to start under any other name.
:::

### 2. Add the first lines to your server.cfg

```ini
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

The two `exec` lines load your permissions and settings, which you make in the next step. FiveM Enhanced blocks resources from writing to disk, and the `add_filesystem_permission` line lets vMenu write into its own folder. All three have to come before `ensure`, otherwise nobody can open the menu and vMenu cannot save anything.

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

Restart your server, join, and press `M` to open the menu. See [Key Bindings](/vmenu/enhanced/key-bindings/) for everything else vMenu puts on a key. Running your server on Linux? Have a look at [Running on Linux](/vmenu/enhanced/running-on-linux/) too.

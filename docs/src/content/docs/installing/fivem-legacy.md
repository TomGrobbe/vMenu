---
title: "Installing on FiveM Legacy"
description: "How to install vMenu Enhanced on a regular FiveM server, the one built on the original GTA V."
---

This page is for servers that run the regular FiveM, built on the original GTA V. We call it FiveM Legacy here, to tell it apart from FiveM Enhanced. Running FiveM Enhanced instead? Then you want [Installing on FiveM Enhanced](/vmenu/enhanced/installing/fivem-enhanced/).

vMenu Enhanced on FiveM Legacy has exactly the same menus and features as on FiveM Enhanced. There is no txAdmin recipe for it, so you install it by hand.

## Before you start

**This is not the original vMenu.** vMenu Enhanced is a separate resource with its own permission names, and it does not read anything the original vMenu saved, like saved vehicles, peds or settings. You can keep your old `vMenu` folder until you are ready to switch, but never run both at the same time.

**Use the latest game build.** vMenu uses parts of the game that older builds do not have. Add `sv_enforceGameBuild` with the newest build number to your `server.cfg`.

**Keep your server up to date.** vMenu sends everything it does over the internet, like update checks, webhook logging and integrations, through a small Node.js script that ships with it. Node.js is the JavaScript engine FiveM runs, and vMenu needs the version 22 your server artifacts come with nowadays. If your console warns that the net bridge never answered, update your server artifacts first.

## 1. Download and unpack it

Download `vMenu-Enhanced-Legacy-v<version>.zip` from the [latest release](https://github.com/TomGrobbe/vMenu/releases/latest) and unpack it into your server's `resources` folder. Make sure you take the zip with "Legacy" in its name. The other one is for FiveM Enhanced, and the other way round does not work either: put this one on a FiveM Enhanced server and vMenu refuses to start, with a message in the console telling you which zip to get.

:::danger[The folder name matters]
Name the folder exactly `vMenu.Enhanced`, capital letters and all, even though your server is not FiveM Enhanced. Every permission and setting name is built from it, so nothing works under a different name. vMenu checks this itself and refuses to start if the name is wrong.
:::

## 2. Add the first lines to your server.cfg

```ini
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

The first line allows vMenu to write into its own folder, which it needs for its example config files. Both names in it are resource names: the one being granted access, and the one whose folder it may write to.

:::danger[Order matters]
The permission has to come before `ensure`. The other way round and vMenu cannot save anything, and you get filesystem errors in your console.
:::

If you use an [integration](/vmenu/enhanced/integrations/) with role sync, vMenu also needs these two lines. Role sync means your integration decides which permission groups a player is in, and vMenu adds them to or removes them from those groups for you. FiveM only lets a resource do that when you allow it, which is what these lines do. They are already at the top of the example `permissions.cfg`, so if you copy that file you have them.

```ini
add_ace resource.vMenu.Enhanced command.add_principal allow
add_ace resource.vMenu.Enhanced command.remove_principal allow
```

## 3. Find the example permissions file

The zip comes with an example permissions file, so you do not have to start your server first. You will find it here:

```
resources/vMenu.Enhanced/config/permissions.cfg.example
```

An "ace" permission is FiveM's way of saying "this player or group is allowed to do this thing". This file is where you decide who gets access to which parts of the menu.

vMenu also rewrites this file every time it starts. That way it always lists everything your server knows about, including anything you added yourself, like your own vehicle or weapon categories.

## 4. Make your own copy and edit it

Copy that file and name the copy `permissions.cfg`, so both sit next to each other. The `.example` file is left alone on purpose, so a future update can refresh it without wiping your settings.

If you only play with friends and want to get going quickly, this single line is all you need in there:

```ini
add_ace builtin.everyone "vMenu.Enhanced.Everything" allow
```

:::danger[This gives everyone everything]
That line hands every player full access to every feature. Only use it on a whitelisted server that random people cannot join.
:::

## 5. Load your permissions file

Add an `exec` line above what you already added:

```ini
exec @vMenu.Enhanced/config/permissions.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

Without it your permissions are never loaded and nobody can open the menu.

## 6. Optional but recommended, the configuration file

`configuration.cfg.example` sits in the same folder and holds settings rather than permissions, so things like how vMenu behaves and what is turned on. It works the same way: copy it, call the copy `configuration.cfg`, edit it, and add one more `exec` line:

```ini
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

## You're done

Restart your server, join, and press `M` to open the menu. For everything else vMenu puts on a key, see [Key Bindings](/vmenu/enhanced/key-bindings/).

## Plugins on FiveM Legacy

Plugins written in Lua, JavaScript or TypeScript work on both versions without any change. A plugin written in C# needs a build made for FiveM Legacy, so check that the plugin you want offers one. If you make C# plugins yourself, [C# setup](/vmenu/enhanced/plugins/csharp/) explains how to build for both.

---
title: "Getting Started"
description: "Temporary installation instructions for the vMenu Enhanced alpha on FiveM Enhanced."
---

:::caution[Temporary instructions]
vMenu Enhanced is still in early development, so be prepared for things not to work properly. These instructions will be replaced with proper documentation once the project settles down.
:::

You need a FiveM **Enhanced** server.

## 1. Download and unpack it

Grab the latest pre-release zip from the [GitHub releases page](https://github.com/TomGrobbe/vMenu/releases/) and unpack it into your server's `resources` folder.

:::danger[The folder name matters]
Name the folder exactly `vMenu.Enhanced`, capital letters and all. Every permission and setting name is built from it, so nothing works under a different name. vMenu checks this itself and refuses to start if the name is wrong.
:::

## 2. Add the first lines to your server.cfg

```ini
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

FiveM Enhanced blocks resources from writing to disk unless you allow it, and vMenu writes its example config files into its own folder. Both names in that first line are resource names: the one being granted access, and the one whose folder it may write to.

:::danger[Order matters]
The permission has to come before `ensure`. The other way round and vMenu cannot save anything, and you get filesystem errors in your console.
:::

## 3. Start your server once

Start it, wait for vMenu to boot, then shut it down. This first run exists so vMenu can create its example files. You will now have:

```
resources/vMenu.Enhanced/config/permissions.cfg.example
```

An "ace" permission is FiveM's way of saying "this player or group is allowed to do this thing". This file is where you decide who gets access to which parts of the menu.

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

## 6. Optional, the configuration file

`configuration.cfg.example` sits in the same folder and holds settings rather than permissions, so things like how vMenu behaves and what is turned on. It works the same way: copy it, call the copy `configuration.cfg`, edit it, and add one more `exec` line:

```ini
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

## You're done

Restart your server, join, and press `M` to open the menu. For everything else vMenu puts on a key, see [Key Bindings](/vmenu/enhanced/key-bindings/).

## Something not working?

Come say hi on the [Discord](https://vespura.com/discord), or take a look at the [GitHub repository](https://github.com/TomGrobbe/vMenu/).

If you are running the current stable version for FiveM Legacy, use the [vMenu Legacy documentation](/vmenu/legacy/) instead.

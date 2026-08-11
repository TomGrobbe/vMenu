---
title: "Getting Started"
description: "Temporary installation instructions for the vMenu Enhanced alpha on FiveM Enhanced."
---

:::caution[Temporary instructions]
vMenu Enhanced is still very much in early development, so be prepared for things to not work properly.
These are temporary installation instructions to get you going. They will be replaced with proper
documentation once the project settles down.
:::

## Requirements

- A FiveM **Enhanced** server.

## Installation

### 1. Download the release

Grab the latest pre-release zip from the [GitHub releases page](https://github.com/TomGrobbe/vMenu/releases/).

### 2. Add it to your resources

Unpack it into your server's `resources` folder and name the folder `vMenu.Enhanced`.

:::danger[The folder name matters]
Call it exactly `vMenu.Enhanced`, capital letters and all. The commands further down refer to the
resource by that name, and so do all of the permission and setting names, so nothing works under a
different name.

vMenu Enhanced checks this itself. If the folder is called anything else, it refuses to start and
prints an error in your server console telling you what to rename it to.
:::

### 3. Add the first lines to your server.cfg

Open your `server.cfg` and add this:

```cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

The first line is a permission grant. vMenu Enhanced writes files into its own resource folder,
because that is how it creates the example config files for you. FiveM Enhanced blocks resources from
writing to disk unless you explicitly allow it, so you have to hand out that permission yourself.

Both names in that command are resource names. The first one is the resource being granted access
(vMenu Enhanced), and the second one is the resource whose folder it is allowed to write to (its own).

The second line, `ensure`, is what actually starts the resource.

:::danger[Order matters]
The permission has to be set before the resource starts. If `ensure vMenu.Enhanced` comes first,
vMenu Enhanced will not be able to save anything and you will see filesystem errors in your server
console.
:::

### 4. Start your server once

Start your server, wait for vMenu to boot, then shut it down again. This first run exists purely so
vMenu Enhanced can create its example files for you.

### 5. Find the example permissions file

After that first run you will see a new file here:

```
resources/vMenu.Enhanced/config/permissions.cfg.example
```

An "ace" permission is FiveM's way of saying "this player or group is allowed to do this thing". This
file is where you decide who gets access to which parts of the menu.

### 6. Make your own copy

Copy that file and name the copy `permissions.cfg`, so you end up with both files sitting next to each
other. The `.example` file is left alone on purpose. Keeping your own copy separate means a future
update can refresh the example without wiping out your settings.

### 7. Edit your permissions

Open `permissions.cfg` and set it up however you like.

If you just want to get going quickly and only play with friends, all you need in there is this single
line:

```cfg
add_ace builtin.everyone "vMenu.Enhanced.Everything" allow
```

:::danger[This gives everyone everything]
That line hands every single player on your server full access to every feature in the menu. Only use
it if you are playing with friends on a whitelisted server, meaning a server that random people cannot
join.
:::

### 8. Load the permissions file

Go back to your `server.cfg` and update what you added earlier so it now looks like this:

```cfg
exec @vMenu.Enhanced/config/permissions.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

The `exec` line tells your server to read your permissions file. Without it, your permissions are never
loaded and nobody will be able to open the menu.

### 9. Optional, set up the configuration file

There is a second example file in that same folder called `configuration.cfg.example`. This one holds
settings rather than permissions, so things like how vMenu behaves and what is turned on.

It works exactly the same way. Make a copy, call it `configuration.cfg`, edit it how you like, and add
one more `exec` line to your `server.cfg`:

```cfg
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
ensure vMenu.Enhanced
```

## You're done

Restart your server and join it.

Press `M` to open the menu. For everything else vMenu puts on a key, and for how you and your players
can change any of it, see [Key Bindings](/vmenu/enhanced/key-bindings/).

## Something not working?

Come say hi on the [Discord](https://vespura.com/discord), or take a look at the
[GitHub repository](https://github.com/TomGrobbe/vMenu/).

If you are running the current stable version for FiveM Legacy, use the
[vMenu Legacy documentation](/vmenu/legacy/) instead.

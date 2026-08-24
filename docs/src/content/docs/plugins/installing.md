---
title: "Installing plugins"
description: "How a server owner installs a vMenu Enhanced plugin, and how a plugin's permissions and settings reach you."
---

This page is for server owners. If you want to know what plugins are first, start at the [plugins overview](/vmenu/enhanced/plugins/).

:::tip[Instructions can vary per plugin]
These are the common steps. Always follow the plugin creator's own instructions where they differ.
:::

## Installing one

A plugin is an ordinary resource, so you install it like any other.

1. Put its folder in your server's `resources` folder.
2. Add `ensure <the folder name>` to your `server.cfg`.
3. Start the server, or start the resource by hand.

vMenu finds and registers the plugin on its own. To control who may use it, see [Permissions and settings](#permissions-and-settings) below.

:::tip[Use a category folder]
Put your plugins in a folder like `[vmenu-plugins]` and you can start them all with one line: `ensure [vmenu-plugins]`. Optional, but it keeps your `server.cfg` tidy.
:::

## Start order does not matter

Start vMenu or your plugins in any order, and restart either one whenever you like.

- **Restart vMenu** and every plugin notices it came back and re-introduces itself. Their menus reappear on their own.
- **Restart a plugin** and vMenu drops that plugin's menus while it is gone, then builds them again when it returns.

If a player has a plugin's menu open when that plugin stops, vMenu closes it for them.

The one thing that does matter is vMenu's own folder name, which has to be exactly `vMenu.Enhanced`. Plugin folders have no naming restrictions.

## Permissions and settings

A plugin can bring **permissions**, deciding who may use its features, and **settings**, which are convars you set with `setr`.

Do not write those by hand. Start the plugin once with vMenu running and vMenu writes a pair of template files for you, named after the plugin's resource:

```
vMenu.Enhanced/config/plugins/<resource name>.permissions.cfg.example
vMenu.Enhanced/config/plugins/<resource name>.configuration.cfg.example
```

You always get both, even from a plugin that only has one of the two. The empty one contains a comment saying so. Each file explains at the top which plugin it came from.

:::caution[The plugins folder has to stay]
`vMenu.Enhanced/config/plugins/` ships with vMenu. Nothing in FiveM lets a resource create a folder, so if you delete it, no plugin templates appear at all.
:::

### Using them

Exactly like vMenu's own config files. Copy the file, take `.example` off the copy, edit the copy, and exec the copy from your `server.cfg`.

Here is the whole shape of it, for a plugin called `CoolPlugin`:

```ini
# The plugin's own permissions and settings, one pair of lines per plugin
exec @vMenu.Enhanced/config/plugins/CoolPlugin.permissions.cfg
exec @vMenu.Enhanced/config/plugins/CoolPlugin.configuration.cfg

# vMenu's own permissions and settings
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg

# Lets vMenu write those template files into its own folder
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced

# Start vMenu and the plugin. These two may be in either order
ensure vMenu.Enhanced
ensure CoolPlugin
```

Every `exec` line belongs above the `ensure` lines, because those lines are what put the permissions and settings into the server, and vMenu reads them from the server rather than from the files.

Never edit the `.example` files themselves. They are rewritten from scratch every time the plugin registers, so your changes would not survive. Nothing a plugin brings is ever written into vMenu's own `permissions.cfg` or `configuration.cfg`.

:::tip[Recommended: give every plugin its own config files]
You can paste a plugin's lines into vMenu's own configs and skip the extra `exec` lines. It works, but please do not.

With its own files, switching a plugin off is stopping the resource and commenting out two lines. With everything mixed together you have to hunt through your main configs for that plugin's lines every time, and it is easy to miss one and then wonder why something broke when you turn it back on.

Separate files also mean you can copy one plugin's setup to another server without dragging your vMenu settings along.
:::

### What the permission names look like

A plugin's permissions live under vMenu's own tree, in a section named after the plugin's resource:

```
vMenu.Enhanced.Plugins.CoolPlugin.Greet
```

Characters that are not letters, digits or underscores become underscores, so `Cool.Plugin` becomes `Cool_Plugin`. The generated template already has the right names, so this only matters if you write lines yourself.

Two shortcuts:

- `vMenu.Enhanced.Plugins.CoolPlugin.All` grants everything that one plugin has.
- `vMenu.Enhanced.Plugins.All` grants everything **every** plugin has, present and future. Handy for a staff group, worth thinking twice about for everybody else, since it covers plugins you install later.

Permissions the author marked as staff only are suggested to `group.admin` in the template rather than `builtin.everyone`.

### Settings

Settings are plain replicated convars:

```ini
setr vMenu.Enhanced.Plugins.CoolPlugin.Enabled true
```

Change one while the server runs and vMenu picks it up live, the same as its own settings.

## Turning a plugin off, or removing it

To stop one temporarily, stop the resource. Its menus disappear immediately. To also stop its permissions and settings applying, put a `#` in front of its two `exec` lines and restart.

To remove one for good:

1. Take its `ensure` line out of `server.cfg` and delete its resource folder.
2. Delete every file in `vMenu.Enhanced/config/plugins/` whose name starts with that resource's name.
3. Take its `exec` lines out of `server.cfg`.

## When something does not work

**No template files appear.** The plugin has to run at least once with vMenu running. If they still do not appear, look for a `[Plugins]` line in your server console saying it could not write the file. That usually means vMenu is not allowed to write to itself, which you fix by adding this above the line that starts vMenu:

```ini
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
```

**The Plugins entry does not show up in the menu.** It is hidden while no plugin is registered. Check the console for a `[Plugins] Registered ...` line. If it is missing, the plugin never introduced itself, which is a problem on the plugin's side.

**The plugin registered but its menu is empty or missing rows.** That is permissions or configuration. Exec the plugin's `permissions.cfg` and run `vmenu_refresh_permissions` in the server console.

**Two plugins clash.** vMenu identifies a plugin by its resource name, so two resources whose names sanitize to the same identity cannot both register. The second is refused with a message naming the first. Rename one.

**I need help with a plugin.** Contact the plugin creator first. If you cannot reach them, you can ask in [Vespura's Discord](https://vespura.com/discord), but that is community support only.

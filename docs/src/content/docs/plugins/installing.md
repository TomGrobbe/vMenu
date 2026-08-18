---
title: "Installing plugins"
description: "How a server owner installs a vMenu Enhanced plugin, why the start order does not matter, and how a plugin's permissions and settings reach you."
---

This page is for server owners. If you want to know what plugins are first, start at the [plugins overview](/vmenu/enhanced/plugins/).

:::tip[Note: instructions per plugin may vary]

While these are the common instructions for installing plugins. Some plugins may require additional setup.

Always be sure to follow the specific installation instructions given by the plugin creator if available.

:::

## Installing one

A plugin is an ordinary resource, so you install it like any other.

1. Put its folder in your server's `resources` folder.
2. Add `ensure <the folder name>` to your `server.cfg`.
3. Start the server, or start the resource by hand.

:::tip[Use resource categories]

I recommend making a category folder first like `[vmenu-plugins]` and placing your plugins inside there

That way you can start all plugin resources in one go: `ensure [vmenu-plugins]`

This is optional, but it will make your server.cfg cleaner.

:::

While your plugin is now installed, you may want to configure the permissions and configuration options that come with your plugin.
For that, see the section about **Permissions and settings** below.

After installing the plugins and vMenu, vMenu will find and register the plugins in the menu on its own.

## The start order does not matter

You can start your plugin(s) or vMenu first, it doesn't matter. The plugin will still be registered even if you start it while the server is already running.

The same goes for restarts, in both directions:

- **Restart vMenu** while plugins are running, and every plugin notices vMenu came back and re-introduces itself. Their menus reappear on their own.
- **Restart a plugin** while vMenu is running, and vMenu drops that plugin's menus the moment it stops and builds them again when it comes back.

If a player happens to have a plugin's menu open when that plugin stops or restarts, vMenu closes the menu for them rather than leaving them in one that no longer exists.

The one thing that does matter is the folder name of vMenu itself, which has to be exactly `vMenu.Enhanced`, **plugin folders have no naming restrictions**.

## Permissions and settings

A plugin can bring two things you control: **permissions**, deciding who may use its features, and **settings**, which are convars you set with `setr`.

You shouldn't write those by hand. Start the plugin once with vMenu running, and vMenu writes a pair of template files for you, both named after the plugin's resource:

```
vMenu.Enhanced/config/plugins/<resource name>.permissions.cfg.example
vMenu.Enhanced/config/plugins/<resource name>.configuration.cfg.example
```

So a plugin in a resource called `CoolPlugin` gives you `CoolPlugin.permissions.cfg.example` and `CoolPlugin.configuration.cfg.example`. 
You always get both, even from a plugin that only has one of the two. 
If a plugin has no configuration options for example, the file will contain a comment explaining that there are no configuration options.

:::caution[The plugins folder has to stay]
`vMenu.Enhanced/config/plugins/` ships with vMenu and contains a README explaining itself. 
vMenu can write files into that folder but nothing in FiveM lets a resource create a folder, so if you delete it, it cannot be recreated and no plugin templates appear at all.
:::

### Using them

Exactly like vMenu's own two config files. Copy the file, take `.example` off the copy's name, edit the copy, and execute the copy from your `server.cfg`.

Here is the whole shape of it, for a plugin called `CoolPlugin`:

```
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

Every `exec` line belongs above the `ensure` line that starts vMenu, because those lines are what put the permissions and settings into the server, and vMenu reads them from the server as it starts (not from the files directly).

The two `ensure` lines themselves do not have a required order, as long as they both appear below the `exec` lines and `add_filesystem_permission`. 
This is also explained above in [the start order does not matter](#the-start-order-does-not-matter).

Never edit the `.example` files themselves. 
They are rewritten from scratch every time the plugin registers, which is every time it or vMenu starts, so your changes would be gone by morning. 
Edit the copy, not the original.

Nothing a plugin brings is ever written into vMenu's own `permissions.cfg` or `configuration.cfg`.
Those two stay about vMenu.

:::tip[Recommended: give every plugin its own config files]
Nothing stops you from pasting a plugin's lines into vMenu's own `permissions.cfg` and `configuration.cfg` and skipping the extra `exec` lines. 
It works. But please do not do it.

The reason is that maintenance becomes difficult the more plugins you have. Especially when you want to disable or remove a plugin, whether for one evening or for good. 

With its own files you stop the resource and take out its two `exec` lines, and that is the entire job. 
With everything mixed into vMenu's own configs you have to go hunting through them for that plugin's lines every single time you switch it off, and put them all back when you want it again. 
It's easy to forget lines and then end up wondering why something isn't working anymore when you turn it back on.

Keeping them apart also means you can hand somebody your whole `plugins` folder, or copy one plugin's setup to another server, without dragging any of your vMenu settings along with it.
:::

Each generated file explains itself at the top, including which plugin it came from, so a file you find in that folder months later never leaves you guessing.

### What the permission names look like

A plugin's permissions live under vMenu's own tree, in a section named after the plugin's resource:

```
vMenu.Enhanced.Plugins.CoolPlugin.Greet
```

Characters that are not letters, digits or underscores are turned into underscores, so a resource called `Cool.Plugin` becomes `Cool_Plugin` in the permission name. 
The generated template already has the right names in it, so this only matters if you are writing lines yourself.

Two shortcuts are worth knowing:

- `vMenu.Enhanced.Plugins.CoolPlugin.All` grants everything that one plugin has.
- `vMenu.Enhanced.Plugins.All` grants everything **every** plugin has, present and future. 
  It is listed in vMenu's own `permissions.cfg.example`.
  Handy for a staff group, and something to think about before handing it to everybody, because it also covers plugins you install later.

Permissions marked as staff only by the plugin's author are suggested to `group.admin` in the template rather than to `builtin.everyone`.

### Settings

Settings are plain replicated convars, so they look like this:

```
setr vMenu.Enhanced.Plugins.CoolPlugin.Enabled true
```

Change one while the server runs and vMenu picks it up live, the same way its own settings work. 
A plugin usually uses them to switch parts of its menu on and off.

## Turning a plugin off, or removing it

To stop a plugin temporarily, stop the resource. Its menus disappear immediately.

If you also want its permissions and settings to stop applying, put a `#` in front of its two `exec` lines and restart the server. 
This is the payoff for having kept them in their own files, it is two characters instead of a search through your main configs, and taking the `#` back off is how you undo it.

To remove one for good:

1. Take its `ensure` line out of `server.cfg` and delete its resource folder.
2. Delete every file in `vMenu.Enhanced/config/plugins/` whose name starts with that resource's name.
3. Take its `exec` lines back out of your `server.cfg`.

There is nothing else to clean up.

## When something does not work

**No template files appear.** The plugin has to run at least once with vMenu running, so start both and look again. 
If they still do not appear, check your server console for a line from `[Plugins]` saying it could not write the file. 
That usually means vMenu is not allowed to write to itself, which you fix by adding this to your `server.cfg` above the line that starts vMenu:

```
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced
```

### The Plugins entry does not show up in the menu.

It is hidden while no plugin is registered. Check the server console for a `[Plugins] Registered ...` line for your plugin. 

If it is not there, the plugin never introduced itself, which is a problem on the plugin's side rather than vMenu's.

### The plugin registered but its menu is empty or missing rows.

That is permissions or configuration. Rows a player may not use are either greyed out with a lock or hidden completely, depending on what the plugin's author chose.
Execute the plugin's `permissions.cfg` and run `vmenu_refresh_permissions` in the server console.

### Two plugins clash.

vMenu identifies a plugin by its resource name, so two resources whose names sanitize to the same identity cannot both register. 
The second one is refused with a message naming the first. 
Rename one of them.

### I need help configuring a plugin or a plugin doesn't work at all
Please contact the plugin creator first. They should be able to help you or figure out if there may be a bug in it.
If you can't contact them for whatever reason, you may ask about it in Vespura's discord if you want, but there's only community support there.
No (official) support guaranteed.

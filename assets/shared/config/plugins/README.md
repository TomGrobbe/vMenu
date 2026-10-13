# Do not delete this folder

This is where vMenu puts the permission and setting templates that **plugins** bring along. A plugin is a separate resource somebody wrote that adds its own menus inside vMenu, so its permissions and its settings are not vMenu's own and do not belong in vMenu's `permissions.cfg` and `configuration.cfg`.

vMenu can write files into this folder, but only if it exists, so if you delete this one it cannot make a new one and every plugin's templates will fail to appear.

## What ends up in here

Every plugin that registers gets two files, both named after the resource it lives in:

```
<resource name>.permissions.cfg.example
<resource name>.configuration.cfg.example
```

So a plugin in a resource called `vMenu.ExamplePlugin` gives you `vMenu.ExamplePlugin.permissions.cfg.example` and `vMenu.ExamplePlugin.configuration.cfg.example`.

They are rewritten every time that plugin registers, which is every time it or vMenu starts. 
Nothing is written while the plugin is not running, so start the plugin first and then come and look. 
Never edit the `.example` files themselves, your changes are wiped on the next start.

## Using them

The same way as vMenu's own two config files. 
Copy the file, take the `.example` off the copy's name, edit the copy, and execute it from your `server.cfg` above the line that starts vMenu and it's plugins:

```
# Execute Plugin Configs
exec @vMenu.Enhanced/config/plugins/vMenu.ExamplePlugin.permissions.cfg
exec @vMenu.Enhanced/config/plugins/vMenu.ExamplePlugin.configuration.cfg

# Execute vMenu Enhanced Configs
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg

# Give vMenu Enhanced permission to write files into it's own folder
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced

# Start vMenu Enhanced and plugins, starting order does not matter
ensure vMenu.Enhanced
ensure vMenu.ExamplePlugin
```

I recommend that you keep your plugin's configuration and permissions files separate from your main vMenu configs.
The reason for this is that if you ever want to disable a plugin, you can simply stop that plugin and remove the `exec` lines for those configs.
If you put your permissions and configuration inside your main configs instead, then you'll have to go in there and edit them each time you want to enable or disable your plugin.

Each file explains itself at the top, including which plugin it came from.

## Removing a plugin

Delete every file in here whose name starts with that resource's name, and take its `exec` lines back out of your `server.cfg`. Nothing of that plugin is left anywhere else in vMenu's config.

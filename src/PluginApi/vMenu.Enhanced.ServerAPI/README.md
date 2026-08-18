# vMenu.Enhanced.ServerAPI

The server side plugin API for vMenu Enhanced, the version of vMenu that runs on FiveM Enhanced (GTA V Enhanced).

Your plugin's server script uses this package to declare the permissions and convar settings your plugin brings. vMenu registers the permissions in its own permission system, under a scope named after your resource, and writes both the permissions and the settings into `config/plugins/` inside the vMenu resource, as `<your resource name>.permissions.cfg.example` and `<your resource name>.configuration.cfg.example`. That way owners manage your plugin exactly like they manage vMenu itself, with ACE permissions and `setr` convars, while everything your plugin brought sits under its own name and can be deleted along with it.

```csharp
var declaration = new ServerPluginDeclaration("My Plugin")
    .AddPermission("Greet", "Lets someone use the greet button.")
    .AddPermission("Poke", "Lets someone poke other players.", staffOnly: true)
    .AddBoolSetting("Enabled", true, "Turns my plugin on or off.");

var result = await VMenuServer.RegisterAsync(declaration);
```

After registering you can check a player's permission by its short name with `VMenuServer.IsPlayerAllowed(source, "Greet")`. Always check on the server before doing anything that matters, because anything a client sends can be forged.

The package version always matches the vMenu Enhanced release it belongs to, so pin it to the vMenu version your server runs. The client half of your plugin uses the matching `vMenu.Enhanced.ClientAPI` package. The plugin documentation is at [docs.vespura.com](https://docs.vespura.com/vmenu/enhanced/plugins/).

## License

This package is licensed under the **GNU General Public License v3.0 or later**, `GPL-3.0-or-later`, which is the same license vMenu Enhanced itself uses. The full text ships inside the package as `LICENSE.md`, and also lives in the [vMenu repository](https://github.com/TomGrobbe/vMenu/blob/enhanced/LICENSE.md).

**That license comes along with your plugin.** A plugin built on this package is a work based on vMenu, so your plugin has to be licensed `GPL-3.0-or-later` as well. You cannot build on this and then put your own closed license on the result. In practice:

- **Running it on your own server, and nowhere else, asks nothing of you.** Using software is not distributing it. Build whatever you like and keep it to yourself.
- **The moment you hand your plugin to somebody else, you owe them the source.** Giving it away, selling it, listing it on a store, sending it to a friend who runs another server, all of that is distribution, and every one of those means the complete source code goes with it, under this same license.
- **You are allowed to charge money.** The GPL has no problem with being paid. What it does not allow is taking the money and keeping the source to yourself, and whoever paid you is then free to pass both on to other people.
- **So if your plugin will ever run on a server that is not yours, plan on open sourcing it.** That is not an extra rule on top of the license, it is what the license already asks of you.

This is a plain language summary, not legal advice. The license text is what actually counts, so read it if the details matter to your situation.

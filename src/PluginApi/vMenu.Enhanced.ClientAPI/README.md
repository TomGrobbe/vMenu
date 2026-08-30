# vMenu.Enhanced.ClientAPI

The client side plugin API for vMenu Enhanced, the version of vMenu that runs on FiveM Enhanced (GTA V Enhanced).

A plugin is a normal FiveM resource of your own. With this package, your resource's client script can declare menus that appear inside vMenu, under a Plugins entry on its main menu. You describe your menu with a typed builder, and the package talks to vMenu over events for you. You never touch vMenu's code, and vMenu never needs to know about your resource ahead of time.

Everything vMenu's own menus can do is available to you. Buttons, checkboxes, lists, sliders, dynamic lists, submenus, confirm items and separators. A checkbox can be marked as persisted, in which case the player's choice is saved in your resource's own key value store and the box reopens the way they left it. Your texts can be translated: you hand the plugin your translation tables per language, English is the required fallback, and your menu follows whatever language the player picked in vMenu. Items can be gated on permissions and on convar settings, both scoped to your plugin, and they refresh live when the server owner changes something. You can also show notifications through vMenu, ask the player for text through vMenu's input box, and read the list of vMenu's themes to put that one player's menus in a different look for the rest of their session.

A quick taste:

```csharp
var plugin = VMenuPlugin.Create("My Plugin");

plugin.Translations.Add("en", new Dictionary<string, string>
{
    ["greet"] = "Say hello",
});

var enabled = plugin.Settings.Bool("Enabled", true, "Turns my plugin on or off.");

var greet = plugin.RootMenu.AddButton(Text.Key("greet"));
greet.Gate = PluginGate.Permission("Greet") & PluginGate.Setting(enabled);
greet.Selected += () => plugin.Notify(NotifyStyle.Success, "Hello!");

await plugin.ConnectAsync();
```

Your resource's server script declares the permissions and settings through the matching `vMenu.Enhanced.ServerAPI` package. vMenu writes them into `config/plugins/` as `<your resource name>.permissions.cfg.example` and `<your resource name>.configuration.cfg.example`, which the server owner copies and executes.

The package version always matches the vMenu Enhanced release it belongs to, so pin it to the vMenu version your server runs. A full example resource lives in the [vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin) repository, and the plugin documentation is at [docs.vespura.com](https://docs.vespura.com/vmenu/enhanced/plugins/).

## License

This package is licensed under the **GNU General Public License v3.0 or later**, `GPL-3.0-or-later`, which is the same license vMenu Enhanced itself uses. The full text ships inside the package as `LICENSE.md`, and also lives in the [vMenu repository](https://github.com/TomGrobbe/vMenu/blob/enhanced/LICENSE.md).

**That license comes along with your plugin.** A plugin built on this package is a work based on vMenu, so your plugin has to be licensed `GPL-3.0-or-later` as well. You cannot build on this and then put your own closed license on the result. In practice:

- **Running it on your own server, and nowhere else, asks nothing of you.** Using software is not distributing it. Build whatever you like and keep it to yourself.
- **The moment you hand your plugin to somebody else, you owe them the source.** Giving it away, selling it, listing it on a store, sending it to a friend who runs another server, all of that is distribution, and every one of those means the complete source code goes with it, under this same license.
- **You are allowed to charge money.** The GPL has no problem with being paid. What it does not allow is taking the money and keeping the source to yourself, and whoever paid you is then free to pass both on to other people.
- **So if your plugin will ever run on a server that is not yours, plan on open sourcing it.** That is not an extra rule on top of the license, it is what the license already asks of you.

This is a plain language summary, not legal advice. The license text is what actually counts, so read it if the details matter to your situation.

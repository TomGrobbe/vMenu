---
title: "Making a plugin"
description: "How to write a vMenu Enhanced plugin in C#: the packages, the project layout, the menu API, permissions and settings."
---

A plugin is a normal FiveM resource of your own that asks vMenu to draw a menu on its behalf. C# is the only officially supported language for menus, Lua and JavaScript will follow later. 
A look at what a tiny JavaScript plugin could look like can be found in the custom themes plugin below.

:::tip[Start from an example]
[vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin) is a complete working plugin that uses every kind of row once. Copy from it.

[vMenu.RoutingBucketsPlugin](https://github.com/TomGrobbe/vMenu.RoutingBucketsPlugin) is a more advanced plugin. This plugin includes server and client side code, both sides talking to each other, server side permission checks on everything, live state pushed out to the menu (live menu updates), rows in [Online Players](#player-actions), and a saved config file of its own.

For theme related plugins, see [vMenu.ThemePicker](https://github.com/TomGrobbe/vMenu.ThemePicker),
and [vMenu.CustomThemesPlugin](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin), and read [registering themes](#adding-themes-of-your-own) to see how you can make your own themes for your server.

All four are listed on the [plugins page](/vmenu/enhanced/plugins/).
:::

:::caution[Read the license section]
Plugins are GPL-3.0-or-later, the same as vMenu. See [License](#license) at the bottom.
:::

## How it fits together

Your resource has two halves, and both talk to vMenu over events. Neither side references the other's assemblies.

The **client** half describes the menu: rows, translations, and which rows are hidden or locked. The **server** half declares the permissions and settings the server owner controls. Use the same names on both sides and vMenu does all the checking for you.

| Package | Used by |
| --- | --- |
| `vMenu.Enhanced.ClientAPI` | your client script |
| `vMenu.Enhanced.ServerAPI` | your server script |

Pin both to the vMenu Enhanced version your server runs. They bring the CitizenFX assemblies with them, so you do not reference those yourself.

## Project layout

Keep the two halves in separate output folders. Each package ships its own copy of `CitizenFX.Base.dll` and `CitizenFX.FiveM.Shared.dll`, and two files with the same name cannot share a folder.

```
MyPlugin/
    fxmanifest.lua
    client/     the client assembly and everything it depends on
    server/     the server assembly and everything it depends on
```

Every client side DLL has to be listed in `files`, because players download those rather than reading them off the server's disk:

```lua
fx_version 'cerulean'
games { 'gta5' }

files {
    'client/CitizenFX.Base.dll',
    'client/CitizenFX.FiveM.Shared.dll',
    'client/CitizenFX.FiveM.Client.dll',
    'client/MessagePack.dll',
    'client/MessagePack.Annotations.dll',
    'client/Microsoft.NET.StringTools.dll',
    'client/vMenu.Enhanced.PluginContracts.dll',
    'client/vMenu.Enhanced.ClientAPI.dll',
}

client_script 'client/MyPlugin.Client.dll'
server_script 'server/MyPlugin.Server.dll'
```

Add a package reference, check for a new DLL in your client output, add a line for it. Miss one and players get an assembly load error the moment they open the menu. The server half needs no `files` entries.

## The smallest plugin that works

Client:

```csharp
using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Script;
using vMenu.Enhanced.ClientAPI;

public sealed class Main : IScript
{
    public async void Initialize()
    {
        var plugin = VMenuPlugin.Create("My Plugin");

        var greet = plugin.RootMenu.AddButton("Say hello");
        greet.Description = "Shows a notification, to prove this thing works.";
        greet.Selected += () => plugin.Notify(NotifyStyle.Success, "Hello!");

        var enabled = plugin.Settings.Bool("Enabled", true, "Turns my plugin on or off.");
        greet.Gate = PluginGate.Permission("Greet") & PluginGate.Setting(enabled);
        greet.HideWhenLocked = true;

        var result = await plugin.ConnectAsync();
        API.Log.Info($"[My Plugin] Registered with vMenu: {result.Accepted}.");
    }
}
```

Server:

```csharp
using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Script;
using vMenu.Enhanced.ServerAPI;

public sealed class Main : IScript
{
    public async void Initialize()
    {
        var declaration = new ServerPluginDeclaration("My Plugin")
            .AddPermission("Greet", "Lets someone use the greet button.")
            .AddBoolSetting("Enabled", true, "Turns my plugin on or off.");

        var result = await VMenuServer.RegisterAsync(declaration);
        API.Log.Info($"[My Plugin] Registered with vMenu: {result.Accepted}.");
    }
}
```

:::caution[Call at least one native in Initialize]
An `Initialize` that calls no FiveM code fails to load. This looks like a FiveM Enhanced bug. A logging statement is enough to work around it.
:::

The gate is the point of declaring the permission and the setting. Without it the button is always there for everybody and the owner's config file does nothing. With it, the button only works for a player who has `Greet` while `Enabled` is on, and it follows along by itself when either changes. Read a value directly with `enabled.Value` when you want the value rather than a gate.

`ConnectAsync` and `RegisterAsync` never throw. A refusal comes back with `Accepted` false and a reason in `Errors`, and anything vMenu accepted but was unhappy about is in `Warnings`. Both are logged for you too.

## Start order and reconnecting

Your plugin can start before or after vMenu and registers either way. If vMenu restarts, the packages re-register everything you declared. If your plugin restarts, vMenu drops your menus while you are gone and rebuilds them when you return. `IsConnected` tells you where you stand, and the `Disconnected` and `RegistrationAnswered` events fire on the way through.

## Rows

Everything vMenu's own menus can do is available:

```csharp
var menu = plugin.RootMenu;

menu.AddButton("A button");
menu.AddConfirmButton("Something destructive");        // asks for a second press
menu.AddCheckbox("A checkbox", initiallyChecked: true);
menu.AddList("A list", new Text[] { "One", "Two" });
menu.AddConfirmList("A list that asks first", options);
menu.AddSlider("A slider", min: 0, max: 10, position: 5);
menu.AddDynamicList("A list built as you scroll", "start");
menu.AddSeparator("A heading");

var sub = menu.AddSubmenu("A submenu", subtitle: "My Plugin");
sub.Menu.AddButton("A row inside it");
```

Each returns an object you can change at any time, before or after connecting: `Text`, `Description`, `Label`, `LockedDescription`, `Visible`, `Enabled`, `Gate`, `SetIcons`, plus whatever that row type has of its own. Changes after connecting update the live menu.

:::tip[Write a description for every row]
`Description` is the line at the bottom of the screen while a row is highlighted. A menu where half the rows explain themselves and half say nothing looks unfinished.
:::

A checkbox can remember itself between sessions with `persist: true`, which stores the choice in your own resource's key value store. Pass a stable `id` with it, because automatic ids follow creation order and reordering your code would hand a saved value to the wrong box.

A menu's `subtitle` is the bar under the banner. Leave it out and vMenu uses the menu's title instead, so it is never empty.

### Adding rows later

You can connect first and build afterwards. Your row under Plugins appears as soon as your menu has something in it, so a plugin that only contributes player actions never advertises an empty menu.

Wrap many changes in a batch and vMenu repaints once instead of once per change:

```csharp
using (plugin.BeginBatch())
{
    foreach (var thing in things)
    {
        menu.AddButton(thing.Name);
    }
}
```

Batches nest safely. Only the outermost one sends, so a helper that batches internally cannot cut your batch short.

## Text and translations

Every piece of text is a `Text` object, either a literal or a key into your own translation tables:

```csharp
plugin.Translations.Add("en", new Dictionary<string, string>
{
    ["greet"] = "Greet {name}",
});

menu.AddButton(Text.Key("greet", ("name", Text.Literal("world"))));
```

A plain string is a literal, so translating is always deliberate. Keys are looked up in the player's current vMenu language and then in your English table, which is why an `en` table is required as soon as you provide any. Your keys are yours alone, so they never collide with vMenu's or another plugin's. When the player changes language, your menu follows.

## Permissions and settings

Declare both on the server, use them on the client.

```csharp
// Server
new ServerPluginDeclaration("My Plugin")
    .AddPermission("Greet", "Lets someone use the greet button.")
    .AddPermission("Poke", "Lets someone poke other players.", staffOnly: true)
    .AddBoolSetting("Enabled", true, "Turns my plugin on or off.");
```

```csharp
// Client
var enabled = plugin.Settings.Bool("Enabled", true, "Turns my plugin on or off.");

greet.Gate = PluginGate.Permission("Greet") & PluginGate.Setting(enabled);
```

Gates combine with `&` and `|`, are evaluated live, and refresh by themselves when the owner changes a permission or setting. `HideWhenLocked` decides whether a row the player may not use is greyed out with a lock or gone entirely.

Names are short and vMenu scopes them to your resource, so `Greet` becomes `vMenu.Enhanced.Plugins.MyPlugin.Greet`. Only letters, digits and underscores are allowed.

Settings come in `Bool`, `Int`, `Float` and `String`. Declare them on both sides. The server half puts them in the owner's template, the client half is what you gate on and read from.

For anything that matters, check on the server before you act:

```csharp
if (VMenuServer.IsPlayerAllowed(source, "Poke")) { ... }
```

Use this rather than FiveM's `IsPlayerAceAllowed`, because it checks parent permissions too. An owner who granted the whole plugin instead of each individual permission then works out of the box.

### What the server owner ends up with

You never ship a config file. vMenu writes one for the owner out of what you declared, and rewrites it every time your plugin registers. Both files land in `vMenu.Enhanced/config/plugins/`, named after your resource:

```
vMenu.Enhanced/config/plugins/MyPlugin.permissions.cfg.example
vMenu.Enhanced/config/plugins/MyPlugin.configuration.cfg.example
```

The owner copies each file, drops the `.example`, edits the copy, and execs it from `server.cfg`. [Installing plugins](/vmenu/enhanced/plugins/installing/) walks them through it.

For your own README:

- Link to [installing plugins](/vmenu/enhanced/plugins/installing/), or write your own instructions if yours differ.
- Tell owners to exec your two files as their own lines rather than pasting your lines into vMenu's configs. Separate files are what makes your plugin removable in one step later.
- Tell them your resource name matters, since every permission and both file names are built from it.
- Give people a way to contact you. We cannot support your plugin, that is your job.

Write a real `description` for every permission and setting. It becomes the comment above that line in the owner's generated file, and it may be the only explanation they ever get.

## Player actions

The one place a plugin reaches outside its own submenu is the **Plugin Actions** submenu inside every player's page in **Online Players**:

```csharp
var poke = plugin.PlayerActions.AddButton("Poke");
poke.Description = "Pokes this player.";
poke.Gate = PluginGate.Permission("Poke");
poke.Selected += target => plugin.Notify(NotifyStyle.Info, $"You poked {target.Name}.");
```

The same rows serve every player, and the selected one is handed to your callback with their name and server id. vMenu adds a line naming your resource to these descriptions, so players can see which resource added it.

## Talking to the player

```csharp
plugin.Notify(NotifyStyle.Success, "That worked.");

if (await plugin.GetTextAsync("What is your name?", maxLength: 32) is { } name)
{
    // they typed something
}
```

Notifications appear in vMenu's notification area, credited to your plugin. `GetTextAsync` opens vMenu's input box and returns null when the player cancels. It can ask several questions in a row, and refuses when something else is already using the box.

## The menu's theme

vMenu can draw its menus in a few different looks, called themes. The server owner picks one for everybody with a convar, and a plugin can put one player in a different one:

```csharp
plugin.Themes.Changed += () =>
{
    foreach (var theme in plugin.Themes.Available)
    {
        // theme.Id is what you send back, theme.Name is what a player reads,
        // theme.IsCurrent says whether it is the one on screen.
    }
};

plugin.Themes.Set("dark");   // this player sees the dark theme from now on
plugin.Themes.Reset();       // back to the theme the server picked
```

vMenu sends the list right after your plugin registers and again on every change, whoever caused it, so build your rows from the `Changed` event rather than straight after connecting. `Available` is empty until that first message arrives, and stays empty against a vMenu too old to know about themes.

A theme set this way beats the server's convar, and it belongs to that one player. vMenu saves nothing, so reconnecting puts the server's choice back, and `Reset` does the same without the wait. Remembering a choice is your plugin's job: keep the id in your own resource's key value store and set it again on the first list vMenu sends you, which is what the Theme Picker plugin does. No permission and no setting gates any of this, because it changes nothing except what that player sees.

### Adding themes of your own

A resource can also hand vMenu new themes, so a server does not have to edit vMenu's own files to get a look of its own. This one needs no C# and no plugin registration at all, it is a single event, so a plain JavaScript or Lua resource can do it:

```js
emit("vMenu.Enhanced:Plugins:RegisterThemes", JSON.stringify({
    themes: [
        { id: "reddead", name: "Red Dead", css: "themes/red-dead.css", banner: "default" },
    ],
}));
```

- **id** is what a convar or a plugin names the theme by. Letters, digits, dashes, underscores and dots, and never one of vMenu's own names.
- **name** is what a player reads, falling back to the id.
- **css** is a path inside your own resource, or a full `https://cfx-nui-<resource>/` url naming another one. vMenu's menu page loads it from there, so nothing is copied into vMenu. Addresses out on the internet are refused.
- **banner** is the picture on top of the menu. One of vMenu's own, `default`, `dark`, `cartoon`, or `none` for the plain Grand Theft Auto one, or a `.png`, `.jpg` or `.webp` of your own, given the same way as the stylesheet. An image needs a MenuAPI new enough to load banners out of another resource, and falls back to vMenu's default banner otherwise.

Every file the stylesheet uses has to be listed in your `fxmanifest.lua` under `files`, or the game has no copy of it to serve. Fonts and images inside the stylesheet are resolved relative to the stylesheet itself.

Send the whole set in one go: a second registration replaces everything that resource registered before, and its themes are dropped the moment it stops. vMenu answers on `vMenu.Enhanced:Plugins:<your resource>:ThemesRegistered` with `accepted`, `errors` and `warnings`, which is where a refused theme tells you why. Send it once when your resource starts, and again whenever vMenu says `vMenu.Enhanced:Plugins:Ready`, which is what it says when it restarts.

Once registered, a theme is a theme like any other. It shows up for plugins reading `Themes.Available`, and the `vMenu.Enhanced.MenuAppearance.Skin` convar accepts its id. If you would rather not write any of this yourself, the [Custom Themes plugin](https://github.com/TomGrobbe/vMenu.CustomThemesPlugin) is exactly this, driven by a JSON file you edit.

## Rules to keep to

- **Your resource name is your identity.** Permissions, settings and both template files are named after it. Two resources whose names sanitize to the same identity cannot both register, the second is refused.
- **Limits.** A menu tree may hold 2000 items and nest 8 levels deep. Rows added after connecting count towards the same 2000. Past that, rows are skipped with a warning.
- **Never trust the client.** The client half decides what a menu looks like, nothing more. Anything that changes the world belongs behind a server side permission check.
- **vMenu owns the menu.** You describe what you want, vMenu decides how and when it is drawn. That is what keeps your plugin working across vMenu updates.
- **Update with vMenu.** A breaking change on vMenu's side can stop an old plugin from loading.

## License

vMenu and both NuGet packages are **GPL-3.0-or-later**, and so is anything you build with them. **You must use that same license for your plugin.** A plugin built on those packages is a work based on vMenu, so you cannot put a closed license on the result.

- **Running it only on your own servers asks nothing of you.** Using software is not distributing it, so your source may stay private.
- **Handing it to anybody else means handing them the source too.** Giving it away, selling it, listing it on a store, or sending it to someone running another server are all distribution.
- **You may charge money for it.** What you may not do is keep the source to yourself, and whoever paid you is then free to pass both on.

This is a plain language summary, not legal advice. The [license text](https://github.com/TomGrobbe/vMenu/blob/enhanced/LICENSE.md) is what counts.

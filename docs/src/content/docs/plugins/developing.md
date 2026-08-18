---
title: "Making a plugin"
description: "How to write a vMenu Enhanced plugin in C#: the two NuGet packages, the project layout, the menu API, permissions and settings, and the rules to keep to."
---

This page is for developers. A plugin is a normal FiveM resource of your own, written in C#, that asks vMenu to draw a menu on its behalf.
C# is currently the only officially supported language to make a plugin in, but this will change in the future, allowing you to also make plugins using both Lua and JavaScript.

:::tip[Start from the example]
There is a complete, working plugin to copy from at [vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin). It uses every kind of row once, with a comment next to each, and its build produces a resource you can drop straight onto a server. Everything below is on show in there.
:::

## For licensing, see the bottom of this page!
[here](#license)

## How it fits together

Your resource has two halves and both talk to vMenu over events. You never reference vMenu's own assemblies and vMenu never references yours.

The **client** half describes the menu. Rows, translations, and which rows are hidden or locked. The **server** half declares the permissions and settings the server owner controls. As soon as permissions and configuration options have been defined in your server side, and you use them in your client side, they will automatically work and vMenu will do all the config and permissions checking for you.

Two NuGet packages do the talking:

| Package | Used by |
| --- | --- |
| `vMenu.Enhanced.ClientAPI` | your client script |
| `vMenu.Enhanced.ServerAPI` | your server script |

Their version always matches the vMenu Enhanced release they belong to, so pin them to the vMenu version your server runs. Both bring the CitizenFX assemblies with them, so you do not reference those yourself.

## Project layout

Keep the two halves in separate output folders. The client and server packages each ship their own copy of `CitizenFX.Base.dll` and `CitizenFX.FiveM.Shared.dll`, and they can not be in the same folder together because they have the same names.

```
MyPlugin/
    fxmanifest.lua
    client/     the client assembly and everything it depends on
    server/     the server assembly and everything it depends on
```

The manifest names both halves, and every client side DLL has to be listed in `files`, because a client assembly and its dependencies are downloaded by each player rather than read off the server's disk:

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
    'client/Newtonsoft.Json.dll',
    'client/vMenu.Enhanced.PluginContracts.dll',
    'client/vMenu.Enhanced.ClientAPI.dll',
}

client_script 'client/MyPlugin.Client.dll'
server_script 'server/MyPlugin.Server.dll'
```

Every time you add a package reference, check whether a new DLL turned up in your client output and add a line for it. 
Forget one and players get an assembly load error the moment they open the menu. 
The server side needs no `files` entries, it loads from disk.

## The smallest plugin that works

The client half:

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

        var result = await plugin.ConnectAsync();

        // NOTE!
        // An Initialize function of any class that inherits from IScript, that does not call any FiveM code, will FAIL to load.
        // This is most likely a bug in FiveM Enhanced right now, but just add a logging statement and you're good.
        API.Log.Info($"[My Plugin] Registered with vMenu: {result.Accepted}.");
    }
}
```

And the server half:

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

        // NOTE!
        // An Initialize function of any class that inherits from IScript, that does not call any FiveM code, will FAIL to load.
        // This is most likely a bug in FiveM Enhanced right now, but just add a logging statement and you're good.
        API.Log.Info($"[My Plugin] Registered with vMenu: {result.Accepted}.");
    }
}
```

`ConnectAsync` and `RegisterAsync` never throw. A refusal comes back as a result with `Accepted` set to false and a reason in `Errors`, and anything vMenu accepted but was unhappy about is in `Warnings`. Both are logged for you as well.

## Start order and reconnecting

Your plugin can start before or after vMenu and it registers either way.

Restarts are handled in both directions too. If vMenu restarts, the packages re-register everything you declared, including changes you made since. 
If your plugin restarts, vMenu drops your menus while you are gone and rebuilds them when you come back. 
`IsConnected` tells you where you stand, and the `Disconnected` and `RegistrationAnswered` events fire on the way through.

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

Each returns an object whose properties you can set at any time, before or after connecting: `Text`, `Description`, `Label`, `LockedDescription`, `Visible`, `Enabled`, `Gate`, `SetIcons`, plus whatever that kind of row has of its own. Setting one after connecting updates the live menu.

:::tip[Write a description for every row]
`Description` is the line vMenu shows at the bottom of the screen while a row is highlighted. Write one for every row you add. 
A menu where half the rows explain themselves and the other half say nothing looks unfinished, and players will probably notice.
:::

A checkbox can remember itself between sessions with `persist: true`, which stores the player's choice in your resource's own key value store. Pass a stable `id` along with it, because the automatic ids follow creation order and reordering your code would otherwise hand a saved value to the wrong box.

Menus take a `subtitle`, the bar under the banner. Leave it out and vMenu falls back to the menu's own title, so it is never empty. Leaving it empty would cause issues, that's why there's an automatic fallback.

### Adding rows later

You can connect first and build afterwards. Your row under Plugins appears as soon as your menu has something in it, and stays hidden while it is empty, so a plugin that only contributes player actions never advertises an empty menu.

Changing many things at once? Wrap it in a batch, and vMenu repaints once instead of once per change:

```csharp
using (plugin.BeginBatch())
{
    foreach (var thing in things)
    {
        menu.AddButton(thing.Name);
    }
}
```

Batches nest safely. If something you call inside a batch opens one of its own, only the outermost one sends, so a helper that batches internally cannot cut your batch short.

## Text and translations

Every piece of text is a `Text` object, which is either a literal or a key into your own translation tables:

```csharp
plugin.Translations.Add("en", new Dictionary<string, string>
{
    ["greet"] = "Greet {name}",
});

menu.AddButton(Text.Key("greet", ("name", Text.Literal("world"))));
```

A plain string is a literal, so translating is always the deliberate act. Keys are looked up in the player's current vMenu language, then in your English table, which is why an `en` table is required as soon as you provide any. Your keys are yours alone, so `greet` never collides with vMenu's or with another plugin's. When the player changes language, your menu follows along without you doing anything.

## Permissions and settings

Declare both on the server, and use them on the client.

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

Gates combine with `&` and `|`, they are evaluated live, and they refresh by themselves when a server owner changes a permission or a setting.
`HideWhenLocked` decides whether a row the player may not use is greyed out with a lock or gone entirely.

Names are short, and vMenu scopes them to your resource for you, so `Greet` becomes `vMenu.Enhanced.Plugins.MyPlugin.Greet`. Only letters, digits and underscores are allowed in the short name.

Settings come in `Bool`, `Int`, `Float` and `String`. Declare them on both sides: the server half is what puts them in the template a server owner reads, the client half is what your menu gates on and reads values from.

For anything that matters, check on the server before you act on it:

```csharp
if (VMenuServer.IsPlayerAllowed(source, "Poke")) { ... }
```

If you use `VMenuServer.IsPlayerAllowed` instead of FiveM's native `IsPlayerAceAllowed`, you get the benefit of parent permissions being checked as well. For example, if a server owner granted the whole plugin instead of each individual permission, checking with vMenu's function makes it work out of the box. Checking with IsPlayerAceAllowed would require you to manually check all parent permisisons as well.

### What the server owner ends up with

You never ship a config file. vMenu writes one for the owner out of what you declared, and rewrites it every time your plugin registers, so what they read is always what your running plugin actually has. Both files land in the one shared folder `vMenu.Enhanced/config/plugins/`, named after your resource:

```
vMenu.Enhanced/config/plugins/MyPlugin.permissions.cfg.example
vMenu.Enhanced/config/plugins/MyPlugin.configuration.cfg.example
```

The owner copies each file, drops the `.example`, edits the copy, and execs the copy from `server.cfg`. 
That is their job, not yours, and [installing plugins](/vmenu/enhanced/plugins/installing/) walks them through it.

Please provide your own instructions with your plugin if they differ from a typical installation. And link back to [installing plugins](/vmenu/enhanced/plugins/installing/) in your documentation if you have nothing specific to add. That way people always know where to find installation instructions.
Please also add a way for server owners to contact you regarding your plugin. We cannot provide support for your plugin, that's your responsibility.

Two things worth doing for them in your own plugin's README:

- **Tell them to exec your two files as their own lines**, rather than pasting your permission and setting lines into vMenu's own `permissions.cfg` and `configuration.cfg`. Both work, but separate files are what makes your plugin removable in one step later, and it is the owner's future self who pays if it is not. The [recommendation on the installing page](/vmenu/enhanced/plugins/installing/) explains the whole reasoning, so linking there is enough.
- **Tell them your resource name matters**, because every permission and both file names are built from it. If you expect anybody to rename your folder, say what that changes.

Also write a real `description` for every permission and setting you declare. The description is the comment that appears above that line in the owner's generated file, and it is the only explanation they will get of what the thing does (unless you write your own documentation elsewhere). 

## Player actions

The one place a plugin reaches outside its own submenu is the **Plugin Actions** submenu inside every player's page in vMenu's **Online Players** menu:

```csharp
var poke = plugin.PlayerActions.AddButton("Poke");
poke.Description = "Pokes this player.";
poke.Gate = PluginGate.Permission("Poke");
poke.Selected += target => plugin.Notify(NotifyStyle.Info, $"You poked {target.Name}.");
```

The same rows serve every player, and the one that was selected is handed to your callback with their name and server id. 
vMenu appends a line naming your resource to the description of these rows, so a player can always see which resource added it.

## Talking to the player and getting their input

```csharp
plugin.Notify(NotifyStyle.Success, "That worked.");

if (await plugin.GetTextAsync("What is your name?", maxLength: 32) is { } name)
{
    // they typed something
}
```

Notifications are shown in vMenu's own notification area, credited to your plugin. 
`GetTextAsync` opens vMenu's input box and returns null when the player cancels. 
It can ask several questions in a row, and it refuses when something else is already using the input box.

## Rules to keep to

### Your resource name is your identity
Permissions, settings and both of your template files are named after it, so renaming the resource renames all of those. 
Two resources whose names sanitize to the same identity cannot both register, the second is refused.

### Limits
A plugin's menu tree may hold 2000 items and nest 8 levels deep. 
Rows you add after connecting count towards that same 2000, and a submenu you add is measured from the root like everything else, so a plugin that keeps adding rows to a live menu eventually has them skipped with a warning rather than growing without end. 
Ids may only contain letters, digits and underscores.

### Never trust the client.
The client half of your plugin decides what a menu looks like, nothing more. Anything that changes the world belongs behind a server side permission check.

### vMenu owns the menu.
Your plugin describes what it wants, vMenu decides how it is drawn and when. 
That is what lets your plugin keep working across vMenu updates without you touching it.

### Updates
Please update your resource whenever a new vMenu version becomes available.
Failing to update to the latest version may mean that your plugin will fail to load in case of any breaking changes on vMenu's side.

### License
vMenu uses the **GPL-3.0-or-later** license, and so do the two NuGet packages you build a plugin with. Their full license text ships inside the packages themselves.

**You must use that same license for your plugins.**
A plugin built on those packages is a work based on vMenu, so you cannot put your own closed license on the result.

What that means in practice:

- **Only running it on your own servers asks nothing of you.** Using software is not distributing it, so your source may stay private.
- **Handing your plugin to anybody else means handing them the source with it.** Giving it away, selling it, listing it on a store, or sending it to someone who runs another server are all distribution, and every one of them means the complete source code goes along, under this same license.
- **You are allowed to charge money for it.** The GPL has nothing against being paid. What it does not allow is taking the money and keeping the source to yourself, and whoever paid you is then free to pass both on to other people.
- **So if your plugin will ever run on a server that is not yours, plan on open sourcing it.** That is not an extra rule stacked on top of the license, it is what the license already asks of you.

This is a plain language summary rather than legal advice. The [license text](https://github.com/TomGrobbe/vMenu/blob/enhanced/LICENSE.md) is what actually counts, so read it if the details matter to your situation.
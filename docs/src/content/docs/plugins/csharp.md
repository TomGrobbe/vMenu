---
title: "C# setup"
description: "Setting up a vMenu Enhanced plugin in C#: the NuGet packages, the project layout and the manifest."
---

The C# plugin API is two NuGet packages. The rest of the API is on [Making a plugin](/vmenu/enhanced/plugins/developing/). The [C# example](https://github.com/TomGrobbe/vMenu.ExamplePlugin/tree/main/csharp) is a complete project to copy.

| Package                    | Used by            |
| -------------------------- | ------------------ |
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

:::caution[Call at least one native in Initialize]
An `Initialize` that calls no FiveM code fails to load. This looks like a FiveM Enhanced bug. A logging statement is enough to work around it.
:::

## Things specific to C#

| Topic | Detail |
| --- | --- |
| Entry points | `VMenuPlugin.Create` on the client, `VMenuServer.RegisterAsync` on the server, both from an `IScript`'s `Initialize`. |
| Text | `Text.Literal` and `Text.Key`. A plain string converts to a literal by itself. |
| Gates | `PluginGate.Permission` and `PluginGate.Setting`, combined with `&` and `\|`. A plain string converts to a permission gate. |
| Batches | `using (plugin.BeginBatch()) { ... }` |
| Inserting rows | `using (menu.InsertAt(2)) { ... }` |
| Positions | Counted from 0. |

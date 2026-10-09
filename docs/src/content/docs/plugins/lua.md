---
title: "Lua setup"
description: "Setting up a vMenu Enhanced plugin in Lua: the vmenu.lua file, the manifest and what differs from C#."
---

The Lua plugin API is one file, `vmenu.lua`. The rest of the API is on [Making a plugin](/vmenu/enhanced/plugins/developing/). The [Lua example](https://github.com/TomGrobbe/vMenu.ExamplePlugin/tree/main/lua) is a complete resource to copy.

## Getting the file

Every vMenu Enhanced zip on the [releases page](https://github.com/TomGrobbe/vMenu/releases/latest) has a `plugin-api` folder with `vmenu.lua` in it. Take the one from the vMenu version your server runs and copy it into your resource. Its first line says which vMenu version it was built for.

Your resource ships its own copy, so it never loads anything from vMenu's folder, and start order does not matter. Replace the file when you update vMenu.

## Manifest

Load it as a shared script, before your own scripts. It gives both halves a global `vMenu` table.

```lua
fx_version 'cerulean'
games { 'gta5' }
lua54 'yes'

shared_script 'vmenu.lua'
client_script 'client.lua'
server_script 'server.lua'
```

## Things specific to Lua

| Topic | Detail |
| --- | --- |
| Entry points | `vMenu.CreatePlugin(name)` on the client. `vMenu.ServerPluginDeclaration(name)` and `vMenu.Server.Register(declaration)` on the server. |
| Properties | Things that never change are fields, such as `row.Id`, `submenu.Menu` and `plugin.RootMenu`. Everything else is a pair of `Get` and `Set` methods, such as `row:GetText()` and `row:SetText(x)`. |
| Options | Optional arguments go in a table with the C# names: `menu:AddCheckbox('Music', { Id = 'music', Checked = true, Persist = true, Description = '...' })`. |
| Events | `row:OnSelected(fn)`, `menu:OnOpened(fn)` and so on. Each returns a function that unsubscribes. A handler that errors is logged and the others still run. |
| Positions | Counted from 1, everywhere: list indexes, `InsertAt`, `Move` and `OnIndexChanged`. |
| Text | A plain string is a literal. `vMenu.Text.Key('greet', { name = 'world' })` for a translation key. |
| Gates | `vMenu.Gate.Permission` and `vMenu.Gate.Setting`, combined with `&` and `\|`, or `vMenu.Gate.All` and `vMenu.Gate.Any`. A plain string is a permission gate. |
| Settings | `setting:GetValue()` reads the convar. |
| Batches | `plugin:Batch(function() ... end)` |
| Inserting rows | `menu:InsertAt(3, function() ... end)` |
| Notifications | `plugin:Notify('success', text)`, with `info`, `success`, `warning` or `error`. |
| Players | A player action hands your handler `{ ServerId, Name }`. |

## Waiting for vMenu

`plugin:Connect()`, `plugin:GetText()`, `plugin:GetTexts()` and `vMenu.Server.Register()` wait for vMenu to answer, so they have to run inside a thread:

```lua
CreateThread(function()
    local result = plugin:Connect()
    print(result.Accepted)
end)
```

Menu event handlers such as `OnSelected` already run in a thread of their own, so you can call `GetText` in one directly. If you would rather not wait at all, pass a callback as the last argument and the call returns straight away:

```lua
plugin:Connect(function(result)
    print(result.Accepted)
end)
```

Calling one of these outside a thread without a callback is an error that tells you exactly that.

## Requirements

Lua 5.4, which is what `lua54 'yes'` asks for. The `&` and `|` on gates are Lua 5.4 operators.

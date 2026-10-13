# vMenu Enhanced plugin API for Lua

One file, `vmenu.lua`, that lets a Lua resource add menus to vMenu Enhanced, under a Plugins entry on its main menu. It has everything the C# plugin API has. That covers buttons, checkboxes, lists, sliders, dynamic lists, submenus, confirm rows, separators, keys, player actions, translations, permission and setting gates, notifications, text prompts and themes.

Take `vmenu.lua` out of the `plugin-api` folder in the vMenu Enhanced zip your server runs, which you get from the [releases page](https://github.com/TomGrobbe/vMenu/releases/latest). Copy it into your resource and load it before your own scripts. The first line of the file says which vMenu version it was built for.

```lua
shared_script 'vmenu.lua'
client_script 'client.lua'
server_script 'server.lua'
```

```lua
local plugin = vMenu.CreatePlugin('My Plugin')

plugin.Translations:Add('en', { greet = 'Say hello' })

local enabled = plugin.Settings:Bool('Enabled', true, 'Turns my plugin on or off.')

local greet = plugin.RootMenu:AddButton(vMenu.Text.Key('greet'), {
    Gate = vMenu.Gate.Permission('Greet') & vMenu.Gate.Setting(enabled),
})

greet:OnSelected(function() plugin:Notify('success', 'Hello!') end)

CreateThread(function()
    plugin:Connect()
end)
```

The documentation is at [docs.vespura.com](https://docs.vespura.com/vmenu/enhanced/plugins/lua/), and a full example resource lives in the [vMenu.ExamplePlugin](https://github.com/TomGrobbe/vMenu.ExamplePlugin) repository.

## License

`GPL-3.0-or-later`, the same license vMenu Enhanced uses. A plugin built on this file is a work based on vMenu, so if you hand your plugin to somebody else, you owe them the complete source under this same license.

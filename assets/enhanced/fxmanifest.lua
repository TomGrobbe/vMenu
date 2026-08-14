-- Manifest data
fx_version 'cerulean'
games { 'gta5' }

-- Resource stuff
name 'vMenu Enhanced'
description 'vMenu for FiveM Enhanced (GTA V Enhanced). Server sided trainer/menu with custom permissions.'
version 'versiongoeshere'
author 'Tom Grobbe'
url 'https://github.com/TomGrobbe/vMenu/'

ui_page 'ui/index.html'

files {
    'ui/**/*',

    -- A wildcard, so a language an owner adds is picked up without touching this file.
    'language/*.json',

    'client/CitizenFX.Base.dll',
    'client/CitizenFX.FiveM.Shared.dll',
    'client/CitizenFX.FiveM.Client.dll',

    'client/MessagePack.dll',
    'client/MessagePack.Annotations.dll',

    'client/Microsoft.NET.StringTools.dll',

    'client/MenuAPI.dll',

    'client/Newtonsoft.Json.dll',

    'client/vMenu.Enhanced.Actions.dll',
    'client/vMenu.Enhanced.BrokenNatives.dll',
    'client/vMenu.Enhanced.Configuration.dll',
    'client/vMenu.Enhanced.Data.dll',
    'client/vMenu.Enhanced.Events.dll',
    'client/vMenu.Enhanced.Logging.dll',
    'client/vMenu.Enhanced.Permissions.dll',
    'client/vMenu.Enhanced.Serialization.dll',
    'client/vMenu.Enhanced.Storage.dll',
    'client/vMenu.Enhanced.MenuFramework.dll',
    'client/vMenu.Enhanced.Menus.dll',
    'client/vMenu.Enhanced.NoClip.dll',
    'client/vMenu.Enhanced.Ticks.dll',
    'client/vMenu.Enhanced.NativeHooks.dll',
}

-- Client assembly
client_script 'client/vMenu.Enhanced.Core.dll'

-- Server assembly. The Lua file is listed first because the assembly calls its export.
server_scripts {
    'server/host_clock.lua',
    'server/vMenu.Enhanced.Core.Server.dll',
}

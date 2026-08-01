-- Manifest data
fx_version 'cerulean'
games { 'gta5' }

-- Resource stuff
name 'vMenu Enhanced'
description 'vMenu for FiveM Enhanced (GTA V Enhanced). Server sided trainer/menu with custom permissions.'
version 'versiongoeshere'
author 'Tom Grobbe'
url 'https://github.com/TomGrobbe/vMenu/'

-- Adds additional logging, useful when debugging issues.
client_debug_mode 'false'
server_debug_mode 'false'

-- Adds extra commands for testing and development
experimental_features_enabled '0'

files {
    'client/CitizenFX.Base.dll',
    'client/CitizenFX.FiveM.Shared.dll',
    'client/CitizenFX.FiveM.Client.dll',

    'client/MessagePack.dll',
    'client/MessagePack.Annotations.dll',

    'client/Microsoft.NET.StringTools.dll',

    'client/MenuAPI.dll',

    'client/vMenu.Enhanced.BrokenNatives.dll',
    'client/vMenu.Enhanced.Configuration.dll',
    'client/vMenu.Enhanced.Data.dll',
    'client/vMenu.Enhanced.Permissions.dll',
    'client/vMenu.Enhanced.MenuFramework.dll',
    'client/vMenu.Enhanced.Menus.dll',
    'client/vMenu.Enhanced.NoClip.dll',
}

-- Client assembly
client_script 'client/vMenu.Enhanced.Core.dll'

-- Server assembly
server_script 'server/vMenu.Enhanced.Core.Server.dll'

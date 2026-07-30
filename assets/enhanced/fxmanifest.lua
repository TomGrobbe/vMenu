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
    'CitizenFX.Base.dll',
    'CitizenFX.FiveM.Shared.dll',
    'CitizenFX.FiveM.Client.dll',

    'MessagePack.dll',
    'MessagePack.Annotations.dll',

    'Microsoft.NET.StringTools.dll',

    'MenuAPI.dll',

    'vMenu.Enhanced.Configuration.dll',
    'vMenu.Enhanced.Permissions.dll',
    'vMenu.Enhanced.Menus.dll',
    'vMenu.Enhanced.NoClip.dll',

    'vMenu.Enhanced.Data.dll',
}

-- Client assemblies.
client_scripts {
    'vMenu.Enhanced.Core.dll',
}

-- Server assembly
server_script 'server/vMenu.Enhanced.Core.Server.dll'

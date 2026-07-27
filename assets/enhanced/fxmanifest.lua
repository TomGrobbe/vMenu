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

-- Shared data models compiled into a single shared assembly.
shared_script 'vMenu.Enhanced.Data.dll'

-- Client assemblies.
client_scripts {
    'vMenu.Enhanced.Core.dll',
    'vMenu.Enhanced.Configuration.dll',
    'vMenu.Enhanced.Permissions.dll',
    'vMenu.Enhanced.Menus.dll',
}

-- Server assemblies.
server_scripts {
    'vMenu.Enhanced.Core.Server.dll',
    'vMenu.Enhanced.Configuration.Server.dll',
    'vMenu.Enhanced.Permissions.Server.dll',
}

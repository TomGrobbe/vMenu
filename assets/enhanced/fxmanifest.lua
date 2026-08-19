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

    
    'client/*.dll',
}

-- Client assembly
client_script 'client/vMenu.Enhanced.Core.dll'

-- Server assembly. The Lua file is listed first because the assembly calls its export.
server_scripts {
    'server/host_clock.lua',
    'server/vMenu.Enhanced.Core.Server.dll',
}

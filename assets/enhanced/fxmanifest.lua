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

    'config/tattoos.json',

    'client/*.dll',
}

-- Client assembly
client_script 'client/vMenu.Enhanced.Core.dll'

-- Linux TLS bridge: net_bridge.js carries HTTPS and WebSocket traffic on Linux and needs Node 22.
node_version '22'

-- Server assembly.
server_scripts {
    'server/net_bridge.js',
    'server/vMenu.Enhanced.Core.Server.dll',
}

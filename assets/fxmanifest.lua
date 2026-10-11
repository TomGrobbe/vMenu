-- Manifest data
fx_version 'cerulean'
games {'gta5'}

-- Resource stuff
name 'vMenu'
description 'Server sided trainer for FiveM with custom permissions, using a custom MenuAPI. More info can be found at www.vespura.com/fivem'
version 'versiongoeshere'
author 'Tom Grobbe'
url 'https://github.com/TomGrobbe/vMenu/'

-- Adds additional logging, useful when debugging issues.
client_debug_mode 'false'
server_debug_mode 'false'

-- Adds extra commands for testing and development
experimental_features_enabled '0'

-- MenuAPI draws the menus on this NUI page.
ui_page 'index.html'

-- Files & scripts
files {
    'Newtonsoft.Json.dll',
    'MenuAPI.dll',
    'index.html',
    'menuapi/**/*',
    'config/*.json'
}

client_script 'vMenuClient.net.dll'
server_script 'vMenuServer.net.dll'
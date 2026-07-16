-- Manifest data
fx_version 'cerulean'
games {'gta5'}

-- Resource stuff
name 'vMenu'
description '适用于 FiveM 的服务端管理菜单，支持自定义权限并使用自定义 MenuAPI。更多信息请访问 www.vespura.com/fivem'
version 'versiongoeshere'
author 'Tom Grobbe'
url 'https://github.com/TomGrobbe/vMenu/'

-- Adds additional logging, useful when debugging issues.
client_debug_mode 'false'
server_debug_mode 'false'

-- Adds extra commands for testing and development
experimental_features_enabled '0'

-- Files & scripts
files {
    'Newtonsoft.Json.dll',
    'MenuAPI.dll',
    'config/*.json'
}

client_script 'vMenuClient.net.dll'
server_script 'vMenuServer.net.dll'

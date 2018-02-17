resource_manifest_version '44febabe-d386-4d18-afbe-5e627f4af937'

name 'vMenu'
description 'Server sided trainer for FiveM with custom permissions, using NativeUI.'
author 'Tom Grobbe (aka Vespura)'
version '00.01.14'
url 'https://github.com/TomGrobbe/vMenu'
license 'TBA'

files {
    'Newtonsoft.Json.xml'
}

client_scripts {
    'NativeUI.dll',
    'Newtonsoft.Json.dll',
    'vMenuClient.net.dll',
}

server_scripts {
    'Newtonsoft.Json.dll',
    'vMenuServer.net.dll',
}


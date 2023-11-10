fx_version 'cerulean'
game 'gta5'

author 'Vespura, XdGoldenTiger, and the community'
description 'vMenu Revamped - WIP'
version '1.0.0'


client_script 'Client/vMenu.Client.net.dll'
server_script 'Server/vMenu.Server.net.dll'
shared_script 'Server/vMenu.Shared.net.dll'

files {
	'Client/ScaleformUI.dll',
	'Client/Newtonsoft.Json.dll',
	'Client/FxEvents.Client.dll',
	'stream/**/*',
	'Theme.json',
	'KeyMapping.json',
	'RichPresence.json'
}

--fxevents_debug_mode '1'
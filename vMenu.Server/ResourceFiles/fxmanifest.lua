fx_version 'cerulean'
game 'gta5'

author 'Vespura, XdGoldenTiger, and the community'
description 'vMenu Revamped - WIP'
version '1.0.0'


client_script 'vMenu.Client.net.dll'
server_script 'vMenu.Server.net.dll'
shared_script 'vMenu.Shared.net.dll'

files {
	'ScaleformUI.dll',
	'Newtonsoft.Json.dll',
	'FxEvents.Server.dll',
	'FxEvents.Client.dll',
	'netstandard.dll',
	'stream/**/*'
}

fxevents_debug_mode '1'
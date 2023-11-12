fx_version 'cerulean'
game 'gta5'
author 'Vespura'
description 'vMenu Revamped was created by XdGoldenTiger, Ricky, Katt, and the FiveM community'
version '1.0.0'

--
--	Set the language for vMenu.
--	See /languages for all known vMenu languages.
--	Only enter the name of the file, not including the file extension!
--	Yes: language 'en'
--	NO: language 'en.json'
--
language 'en'

client_script 'Client/vMenu.Client.net.dll'
server_script 'Server/vMenu.Server.net.dll'
shared_script 'Server/vMenu.Shared.net.dll'

files {	
    'Client/ScaleformUI.dll',	
    'Client/Newtonsoft.Json.dll',	
    'Client/FxEvents.Client.dll',	
    'stream/**/*',	
    'languages/*.json',	
    '*.jsonc'
}
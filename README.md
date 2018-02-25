#### Latest Builds
|Master (Stable)|Development (Beta)|
|:-|:-|
|[![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=master)](https://travis-ci.com/TomGrobbe/vMenu) | [![Build Status](https://travis-ci.com/TomGrobbe/vMenu.svg?token=ssVStPpK5ekxFpbVzc3k&branch=development)](https://travis-ci.com/TomGrobbe/vMenu)|

--------

# vMenu
vMenu is a custom built server sided trainer, with basic permissions support, whenever possible using labels to automatically translate many menu options to the player's game language, and much more.

## Features
**Almost all features have custom permissions options.**


## Menu Features
_(Grouped by submenu/category)_


**Wildcard permissions include: **
- `vMenu.everything`: Grants access to everything (not recommended, only use this if you're lazy.)
- `vMenu.menus.*`: Grans access to all menus, note this does not grant access to any option inside the menus.
- `vMenu.<submenuName>.*`: Grants access to all the options inside that specific submenu.


### Online Player Options
Permission to access this submenu: `vMenu.menus.onlinePlayers` (note this does not auto-grant any of the options inside the submenu).

|Option|Type|Description|Permission|Allowed by default|
|:-|:-|:-|:-|-:|
|Teleport To Player|Button|Teleport to the selected player.|`vMenu.onlinePlayers.teleport`|yes|
|Teleport Into Vehicle|Button|Teleport to the selected player's vehicle.|`vMenu.onlinePlayers.teleport`|yes|
|Set Waypoint|Button|Set a waypoint to the selected player.|`vMenu.onlinePlayers.waypoint`|yes|
|Spectate|Button|Spectate the selected player.|`vMenu.onlinePlayers.spectate`|yes|
|Summon|Button|Teleports the selected player to you.|`vMenu.onlinePlayers.summon`|no _(yes for moderators)_|
|Kill|Button|Kills the selected player. Damn, you're cruel if you use this.|`vMenu.onlinePlayers.kill`|no _(yes for admins)_|
|Kick|Button|Kicks the selected player from the server.|`vMenu.onlinePlayers.kick`|no _(yes for admins)_|


### Player Options:
Permission to access this submenu: `vMenu.menus.playerOptions` (note this does not auto-grant any of the options inside the submenu).

|Option|Type|Description|Permission|Allowed by default|
|:-|:-|:-|:-|-:|
|God Mode|Checkbox|Completely invincible.|`vMenu.playerOptions.god`|yes|
|Invisibility|Checkbox|Become invisible for yourself and others.|`vMenu.playerOptions.invisible`|yes|
|Unlimited Stamina|Checkbox|You can keep running without slowing down or taking damage.|`vMenu.playerOptions.stamina`|yes|
|Fast Run|Checkbox|Enable super fast running.|`vMenu.playerOptions.fastrun`|yes|
|Fast Swim|Checkbox|Enable super fast swimming.|`vMenu.playerOptions.fastswim`|yes|
|Super Jump|Checkbox|Enable super high jumping.|`vMenu.playerOptions.superjump`|yes|
|No Ragdoll|Checkbox|Keeps you from falling over when standing on top of a moving vehicle.|`vMenu.playerOptions.noragdoll`|yes|
|Never Wanted|Checkbox|Disable all wanted levels.|`vMenu.playerOptions.neverwanted`|yes|
|Set Wanted Level|List|Set your current wanted level.|`vMenu.playerOptions.setwanted`|yes|
|Everyone Ignores You|Checkbox|Everyone ignores you with this enabled.|`vMenu.playerOptions.ignored`|yes|
|Player Options|List|Allows you to perform these actions on your character: heal, clean, add armor, dry clothes, wet clothes, commit suicide, drive to wp, drive around randomly.|`vMenu.playerOptions.options`|yes|
|Freeze Player|Checkbox|Freeze yourself, why would you do this though?|`vMenu.playerOptions.freeze`|yes|
|Scenarios|List|Select a scenario and press **enter** to enable it. Press **enter** again to disable, or select another and it will switch instead.|`vMenu.playerOptions.scenarios`|yes|
|Force Stop Scenario|Button|Allows you to forcefully stop a currently playing scenario.|`vMenu.playerOptions.scenarios`|yes|


### Vehicle Options
Permission to access this submenu: `vMenu.menus.vehicleOptions` (note this does not auto-grant any of the options inside the submenu).

|Option|Type|Description|Permission|Allowed by default|
|:-|:-|:-|:-|-:|
|God Mode|Checkbox|Prevents any visual and physical damage on your vehicle.|`vMenu.vehicleOptions.god`|yes|
|Repair|Button|Repairs all damage on your vehicle.|`vMenu.vehicleOptions.repair`|yes|
|Wash|Button|Cleans your vehicle.|`vMenu.vehicleOptions.wash`|yes|
|Set Dirt Level|List|Set a specific dirt level on your vehicle.|`vMenu.vehicleOptions.setdirt`|yes|
|Toggle Engine|Button|Turns your vehicle's engine on/off.|`vMenu.vehicleOptions.engine`|yes|
|Set License Plate Text|Button (input)|Allows you to set your custom plate text.|`vMenu.vehicleOptions.platetext`|yes|
|Set LIcense Plate Style|List|Sets the style of your license plate.|`vMenu.vehicleOptions.platestyle`|yes|
|Mod Menu|Submenu|Allows you to customize your vehicle.|`vMenu.vehicleOptions.mod`|yes|
|Vehicle Colors|Submenu|Allows you to change your vehicle's colors.|`vMenu.vehicleOptions.colors`|yes|
|Vehicle Liveries|Submenu|Change your vehicle's livery (if available).|`vMenu.vehicleOptions.livery`|yes|
|Vehicle Extras|Submenu|Enable/disable any vehicle extras/components (if available).|`vMenu.vehicleOptions.components`|yes|
|Vehicle Doors|Submenu|Open/close specific or all vehicle doors.|`vMenu.vehicleOptions.doors`|yes|
|Vehicle Windows|Submenu|Roll down/up the front/back windows.|`vMenu.vehicleOptions.windows`|yes|
|Freeze Vehicle|Checkbox|Freezes your vehicle position, why would you do this though?|`vMenu.vehicleOptions.freeze`|yes|
|Enable Engine Torque multiplier|Checkbox|Enables the selected engine torque multiplier.|`vMenu.vehicleOptions.torque`|yes|
|Set Engine Torque multiplier|List|Set the engine torque multiplier amount.|`vMenu.vehicleOptions.torque`|yes|
|Enable Engine Power multiplier|Checkbox|Enables the selected engine power multiplier.|`vMenu.vehicleOptions.power`|yes|
|Set Engine Power multiplier|List|Set the engine power multiplier amount.|`vMenu.vehicleOptions.power`|yes|
|Flip Vehicle|Button|Flips your vehicle if it's upside down.|`vMenu.vehicleOptions.flip`|yes|
|Toggle Alarm|Button|Enables or disables the vehicle alarm, when you enable it the amount of time before the alarm shuts off is randomized between 8-30 seconds.|`vMenu.vehicleOptions.alarm`|yes|
|Cycle Through Seats|Button|Places you in the next available seat.|`vMenu.vehicleOptions.seats`|yes|
|Engine Always On|Checkbox|Leave the engine running when you exit a vehicle.|`vMenu.vehicleOptions.alwayson`|yes|
|Disable Siren|Checkbox|Disables the siren on this vehicle (may not be synced for other players).|`vMenu.vehicleOptions.nosiren`|yes|
|No Bike Helmet|Checkbox|Disables the helmet you would normally auto-equip when getting on a motorcycle or quad.|`vMenu.vehicleOptions.nohelmet`|yes|
|Delete Vehicle|Button|Deletes your current vehicle.|`vMenu.vehicleOptions.delete`|yes|


### Vehicle Spawner
Permission to access this submenu: `vMenu.menus.vehicleSpawner` (note this does not auto-grant any of the options inside the submenu).

|Option|Type|Description|Permission|Allowed by default|
|:-|:-|:-|:-|-:|
|Spawn Inside Vehicle|Checkbox|If this is enabled, then you will spawn inside the vehicle when you spawn it. If this is disabled, then the vehicle will spawn in front of you, rotated 90 degrees.|`vMenu.vehicleSpawner.spawninside`|yes|
|Replace Previous|Checkbox|When you spawn a new vehicle, this will make sure to delete your old vehicle. If this permission is **NOT** granted, this option will be invisible, but always turned on iow: player's can't disable this, thus their previous vehicles will always be cleaned up.|`vMenu.vehicleSpawner.name`|yes|
|Spawn Vehicle By Name|Button (input)|Allows you to enter a vehicle name to quickly spawn it in front of you. If you don't have permission to spawn the requested vehicle by it's class/category list (see the option below) then you won't be able to spawn it via this button either.|`vMenu.vehicleSpawner.name`|yes|
|Spawn Vehicle By Category|Submenus [multiple]|Allows you to browse each vehicle class/category and spawn a specific vehicle. (Check the provided permissions.cfg file in the downloaded zip for more information regarding the different vehicle classes)|`vMenu.vehicleSpawner.<className>`|yes (only `.trains` is no by default)|

***More documentation will be added soon™.***

-----


## NativeUI
This menu is created using [a modified version of NativeUI](https://github.com/TomGrobbe/NativeUI), originally by [Guad](https://github.com/Guad/NativeUI).

## License
Tom Grobbe - https://www.vespura.com/
Copyright © 2017-2018

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. PROPER CREDIT IS **ALWAYS** REQUIRED WHEN RELEASING MODIFIED VERSIONS OF MY WORK.

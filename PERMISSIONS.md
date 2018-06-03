## Features & Permissions List
**Almost all features have custom permissions options. Some options without permissions are not listed below, however they are present in the menu itself.**
>### ⚠ Please note that ALL permissions are `CaSeSeNSItIve`!

## Global Permissions
**Notes:**
1. __\*__ The `Default (allowed/denied)` values are based on the assumption that you use the default permissions file included with the menu, and you've granted yourself no special permissions or added yourself to any of the admin/moderator groups. If you **DON'T** use the default permissions file, then every option will be **DENIED** by default.
2. __\*\*__ These options are only allowed by default for the "Moderators" / "Admins" groups in the provided permissions file with this resource.
3. __\*\*\*__ When spawning a car using the `Spawn By Name` button, it will always check to see if you have permission for that specific vehicle's class. eg: If you don't have permission to spawn cars from the `Super` class, trying to spawn an `adder` using the `Spawn By Name` button won't work.
4. __\*\*\*\*__ Only admins are allowed to use this by default.

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.Everything`|Grants access to everything, not recommended to give this out.|Denied|
|`vMenu.DontKickMe`|Prevents this player from being kicked.|Denied|
|`vMenu.DontBanMe`|Prevents this player from being banned.|Denied|
|`vMenu.NoClip`|Allows the user to use the NoClip feature.|Allowed|

## Online Players

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.OnlinePlayers.Menu`|Grants access to the Online Players Menu|Allowed|
|`vMenu.OnlinePlayers.All`|Grants access to **ALL** `Online Players Menu` options.|Denied|
|`vMenu.OnlinePlayers.Teleport`|Allows you to teleport to another player.|Allowed|
|`vMenu.OnlinePlayers.Waypoint`|Allows you to set a waypoint to another player.|Allowed|
|`vMenu.OnlinePlayers.Spectate`|Allows you to spectate another player.|Allowed|
|`vMenu.OnlinePlayers.Summon`|Allows you to summon/teleport another player to you. (Default: moderators only)|Denied|
|`vMenu.OnlinePlayers.Kill`|Allows you to kill another player by pressing a button. Dam, you're very cruel. (Default: moderators only)|Denied|
|`vMenu.OnlinePlayers.Kick`|Allows you to kick another player from the server. (Default: moderators only)|Denied|
|`vMenu.OnlinePlayers.TempBan`|Allows you to ban the player from the server for a custom amount of time, max temp-ban duration: 30 days. (Default: admins only)|Denied\*|
|`vMenu.OnlinePlayers.PermBan`|Allows you to ban the player from the server forever. (Default: admin only)|Denied\*\*\*\*|
|`vMenu.OnlinePlayers.Unban`|Allows you to access the "Banned Players" menu to view and unban banned players. (The menu is not actually located in the "Online Players menu", it's in the main menu instead. It is still part of "user management" and thus the permission node is related to "Online Players"). (Allowed by default for admins)|Denied|

## Player Options

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.PlayerOptions.Menu`|Grants access to the Player Options Menu.|Allowed|
|`vMenu.PlayerOptions.All`|Grants access to **ALL** `Player Options Menu` options.|Denied|
|`vMenu.PlayerOptions.God`|Allows you to use god mode.|Allowed|
|`vMenu.PlayerOptions.Invisible`|Allows you to go invisble.|Allowed|
|`vMenu.PlayerOptions.FastRun`|Allows you to enable Fast Run.|Allowed|
|`vMenu.PlayerOptions.FastSwim`|Allows you to enable Fast Swim.|Allowed|
|`vMenu.PlayerOptions.Superjump`|Allows you to enable Superjump.|Allowed|
|`vMenu.PlayerOptions.NoRagdoll`|Allows you to enable No Ragdoll.|Allowed|
|`vMenu.PlayerOptions.NeverWanted`|Allows you to enable Never Wanted.|Allowed|
|`vMenu.PlayerOptions.SetWanted`|Allows you to set a custom wanted level.|Allowed|
|`vMenu.PlayerOptions.Ignored`|Allows you to enable the Everyone Ignores Player option.|Allowed|
|`vMenu.PlayerOptions.Functions`|Allows you to access some basic functions like healing, cleaning clothes, dry/wet clothes, commit suicide, etc.|Allowed|
|`vMenu.PlayerOptions.Freeze`|Allows you to freeze your own player. Why would you need to do this though...|Allowed|
|`vMenu.PlayerOptions.Scenarios`|Allows you to play and stop scenarios.|Allowed|

## Vehicle Options

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.VehicleOptions.Menu`|Grants access to the Vehicle Options Menu.|Allowed|
|`vMenu.VehicleOptions.All`|Grants access to **ALL** `Vehicle Options Menu` options.|Denied|
|`vMenu.VehicleOptions.God`|Allows you to enable vehicle godmode.|Allowed|
|`vMenu.VehicleOptions.SpecialGod`|Allows you to enable a special vehicle godmode which repairs your vehicle instantly when it gets damaged, this is required for vehicles like the Phantom Wedge to keep them from slowly losing health even with regular god mode turned on.|Allowed|
|`vMenu.VehicleOptions.Repair`|Allows you to repair your vehicle.|Allowed|
|`vMenu.VehicleOptions.Wash`|Allows you to wash/clean your vehicle & set a custom dirt level.|Allowed|
|`vMenu.VehicleOptions.Engine`|Allows you to toggle your engine on/off.|Allowed|
|`vMenu.VehicleOptions.ChangePlate`|Allows you to change your vehicle's license plate style & text.|Allowed|
|`vMenu.VehicleOptions.Mod`|Allows you to modify any visual and performance specs of your vehicle.|Allowed|
|`vMenu.VehicleOptions.Colors`|Allows you to change the color of your vehicle.|Allowed|
|`vMenu.VehicleOptions.Liveries`|Allows you to change the livery of your vehicle.|Allowed|
|`vMenu.VehicleOptions.Components`|Allows you to modify the components/extras of your vehicle.|Allowed|
|`vMenu.VehicleOptions.Doors`|Allows you to open/close vehicle doors using the menu.|Allowed|
|`vMenu.VehicleOptions.Windows`|Allows you to roll up/down your windows using the menu.|Allowed|
|`vMenu.VehicleOptions.Freeze`|Allows you to freeze the position of your vehicle (why would you do this though...)|Allowed|
|`vMenu.VehicleOptions.TorqueMultiplier`|Allows you to set and enable an engine torque multiplier.|Allowed|
|`vMenu.VehicleOptions.PowerMultiplier`|Allows you to set and enable an engine power multiplier.|Allowed|
|`vMenu.VehicleOptions.Flip`|Allows you to flip your vehicle if it's upside down.|Allowed|
|`vMenu.VehicleOptions.Alarm`|Allows you to toggle the vehicle's alarm on/off. Turning it on will randomize the alarm duration between 8-30 seconds.|Allowed|
|`vMenu.VehicleOptions.CycleSeats`|Allows you to cycle through all available vehicle seats.|Allowed|
|`vMenu.VehicleOptions.EngineAlwaysOn`|Allows you to enable the Engine Always On feature, this keeps the engine running when you exit your vehicle.|Allowed|
|`vMenu.VehicleOptions.NoSiren`|Allows you to disable the siren on the vehicle.|Allowed|
|`vMenu.VehicleOptions.NoHelmet`|Allows you to disable the "automatically equipped" helmets when getting on a bike.|Allowed|
|`vMenu.VehicleOptions.Lights`|Allows you to enable/disable specific vehicle lights like hazard lights, turn signals, interior lighting, taxi lights or helicopter spotlights.|Allowed|
|`vMenu.VehicleOptions.Delete`|Allows you to delete your current vehicle.|Allowed|
|`vMenu.VehicleOptions.Underglow`|Allows you to access the vehicle underglow options submenu.|Allowed|

## Vehicle Spawner

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.VehicleSpawner.Menu`|Grants access to the Vehicle Spawner Menu.|Allowed|
|`vMenu.VehicleSpawner.All`|Allows you to spawn **ANY** vehicle.|Denied|
|`vMenu.VehicleSpawner.DisableReplacePrevious`|Allows you to disable the "Replace Previous" vehicle option, if this is not allowed, then the option will be forced on.|Allowed|
|`vMenu.VehicleSpawner.SpawnByName`|Allows you to enter a **custom vehicle name** to spawn[\*\*\*](#global-permissions).|Allowed|
|`vMenu.VehicleSpawner.Addon`|Allows you to spawn a vehicle from the Addon Vehicles list (requires vMenu v1.0.7+).|Allowed|
|`vMenu.VehicleSpawner.Compacts`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Sedans`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.SUVs`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Coupes`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Muscle`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.SportsClassic`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Sports`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Super`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Motorcycles`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.OffRoad`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Industrial`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Utility`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Vans`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Cycles`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Boats`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Helicopters`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Planes`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Service`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Emergency`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Military`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Commercial`|Allows you to spawn a vehicle from this category.|Allowed|
|`vMenu.VehicleSpawner.Trains`|Allows you to spawn a vehicle from this category.|Allowed|

## Saved Vehicles

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.SavedVehicles.Menu`|Grants access to the Saved Vehicles Menu.|Allowed|
|`vMenu.SavedVehicles.All`|Grants access to **ALL** `Saved Vehicles Menu` options.|Denied|
|`vMenu.SavedVehicles.Spawn`|Allows you to spawn one of your saved cars. Saving new cars or deleting existing saved cars is always allowed no matter what.|Allowed|

## Player Appearance

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.PlayerAppearance.Menu`|Grants access to the Player Appearance Menu.|Allowed|
|`vMenu.PlayerAppearance.All`|Grants access to **ALL** `Player Appearance Menu` options.|Denied|
|`vMenu.PlayerAppearance.Customize`|Allows you to customize your current ped.|Allowed|
|`vMenu.PlayerAppearance.SpawnSaved`|Allows you to spawn a saved ped. Saving new peds or deleting existing saved peds is always allowed no matter what.|Allowed|
|`vMenu.PlayerAppearance.SpawnNew`|Allows you to spawn any ped model from a list.|Allowed|

## Time Options

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.TimeOptions.Menu`|Grants access to the Time Options Menu.|Denied[\*\*](#global-permissions)|
|`vMenu.TimeOptions.All`|Grants access to **ALL** `Time Options Menu` options.|Denied|
|`vMenu.TimeOptions.FreezeTime`|Allows you to freeze the current time. (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|
|`vMenu.TimeOptions.SetTime`|Allows you to set the current time. (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|

## Weather Options

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.WeatherOptions.Menu`|Grants access to the Weather Options Menu.|Denied[\*\*](#global-permissions)|
|`vMenu.WeatherOptions.All`|Grants access to **ALL** `Weather Options Menu` options.|Denied|
|`vMenu.WeatherOptions.Dynamic`|Allows you to enable/disable dynamic weather changes (which, when enabled, occur every 5 minutes). (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|
|`vMenu.WeatherOptions.Blackout`|Allows you to enable/disable blackout mode (all light sources in the map go dark). (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|
|`vMenu.WeatherOptions.SetWeather`|Allows you to set a custom weather type. (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|
|`vMenu.WeatherOptions.RemoveClouds`|Allows you to remove all cloud effects (only use this with Clear or Extra Sunny weather, obviously). (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|
|`vMenu.WeatherOptions.RandomizeClouds`|Allows you to randomize the cloud patterns/effects. (Synced for everyone in the server)|Denied[\*\*](#global-permissions)|

## Weapon Options
**Adding/Removing/Customizing any weapon is automatically _ALLOWED_ when you give the player permissions to access this menu.**

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.WeaponOptions.Menu`|Grants access to the Weapon Options Menu.|Allowed|
|`vMenu.WeaponOptions.All`|Grants access to all `Weapon Options Menu` options.|Denied|
|`vMenu.WeaponOptions.GetAll`|Allows you to use the `Get All Weapons` button.|Allowed|
|`vMenu.WeaponOptions.RemoveAll`|Allows you to use the `Remove All Weapons` button.|Allowed|
|`vMenu.WeaponOptions.UnlimitedAmmo`|Allows you to enable/disable unlimited ammo.|Allowed|
|`vMenu.WeaponOptions.NoReload`|Allows you to enable/disable no-reload.|Allowed|
|`vMenu.WeaponOptions.Spawn`|Allows you to spawn/remove ANY weapon, denying this will still grant access to the customization options for each weapon. This also allows players to spawn addon weapons.|Allowed|
|`vMenu.WeaponOptions.SetAllAmmo`|Allows you to bulk set the ammo count in all currently equipped weapons.|Allowed|

**For a list of individual weapon permissions check [this link](https://hastebin.com/anowahehub.css).**

## Misc Settings
**The `Save Personal Settings` option in the Misc Settings Menu is always allowed, so there's no permission line for that.**

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.MiscSettings.All`|Grants access to **ALL** `Misc Settings Menu` options.|Denied|
|`vMenu.MiscSettings.ClearArea`|Allows you to reset/clear everything 100m around you in the world.|Allowed|
|`vMenu.MiscSettings.TeleportToWp`|Allows you to teleport to the waypoint on your map.|Allowed|
|`vMenu.MiscSettings.ShowCoordinates`|Allows you to show your current coordinates on screen.|Allowed|
|`vMenu.MiscSettings.ShowLocation`|Allows you to show your current location on screen (pretty much just like PLD).|Allowed|
|`vMenu.MiscSettings.JoinQuitNotifs`|Allows you to receive join/quit notifications when someone joins/quits the server.|Allowed|
|`vMenu.MiscSettings.DeathNotifs`|Allows you to receive death notifications when someone dies or gets killed.|Allowed|
|`vMenu.MiscSettings.NightVision`|Allows you to toggle night vision on/off.|Allowed|
|`vMenu.MiscSettings.ThermalVision`|Allows you to toggle thermal vision on/off.|Allowed|

## Voice Chat

|Permission|Description|Default[\*](#global-permissions)|
|:-|:-|:-|
|`vMenu.VoiceChat.Menu`|Grants access to the Voice Chat Options Menu|Allowed|
|`vMenu.VoiceChat.All`|Grants access to **ALL** `Voice Chat Options Menu` options.|Denied|
|`vMenu.VoiceChat.Enable`|Allows you to enable/disable voice chat.|Allowed|
|`vMenu.VoiceChat.ShowSpeaker`|Allows you to enable/disable the "Currently Talking" display at the top of your screen when someone is using voice chat.|Allowed|
|`vMenu.VoiceChat.StaffChannel`|Allows you to enter the staff-only voice channel.|Denied[\*\*](#global-permissions)|

## About Submenu
The **About vMenu** submenu is always available for everyone, and can not be disabled with the use of permissions. If you don't feel like showing credits to everyone --which seems very selfish to me-- then you'll have to edit the code and disable it yourself, which also means I won't be giving you any support whatsoever.

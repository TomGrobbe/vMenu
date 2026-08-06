---
title: "Configuration Options"
---

**vMenu is configured using convars. A list of all available options can be found below. All options can also be found in the permissions.cfg, which is also the place where you can configure them. If you want, you could also move this to the server.cfg, as long as it's above the `start vMenu` line, but for most people it'll be easier to just keep the config options in the permissions.cfg.**

To set an option, use the following command in the server.cfg or permissions.cfg:

```yaml
setr vmenu_option_name value
```

### General settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_use_permissions`|`true` / `false`|This option allows you to toggle on/off the permissions system in vMenu. If you just want a plug-and-play resource and don't want to mess with configuring permissions, then use this. It will disable all admin features like banning, kicking, unbanning and similar.|
|`vmenu_menu_staff_only`|`true` / `false`|This option allows you to make this menu staff-only. Anyone with the `vMenu.Staff` permission will be able to use the menu. It will still keep in mind all other permissions, it's just a way of limiting who can open the menu.|
|`vmenu_disable_player_stats_setup`|`true` / `false`|Prevent vMenu from setting any player stats like stamina, shooting, driving ability, etc.|
|`vmenu_menu_toggle_key`|A valid [control ID][control].|Sets the **default** key that opens vMenu (keyboard/mouse only, controller key will always be (hold) 'change view/select'). This is only the default — each player can rebind it themselves via FiveM's **Settings » Key Bindings** menu ("vMenu Toggle Button"), which overrides this value for them.|
|`vmenu_noclip_toggle_key`|A valid [control ID][control].|Sets the **default** noclip toggle key (keyboard/mouse only). This is only the default — each player can rebind it themselves via FiveM's **Settings » Key Bindings** menu ("vMenu NoClip Toggle Button"), which overrides this value for them.|
|`vmenu_keymapping_id`|A single word (no spaces)|Sets the identifier used for vMenu's key mappings (the menu and noclip toggle keys registered through FiveM's key mapping system). Must be one word with no spaces. (Default: `Default`)|
|`vmenu_keep_spawned_vehicles_persistent`|`true` / `false`|Keeps spawned vehicles persistent if 'replace previous vehicle' is turned off.|
|`vmenu_use_els_compatibility_mode`|`true` / `false`|Enables compatibility mode for ELS resources (and other siren control scripts) by disabling vMenu's siren control option completely.|
|`vmenu_handle_invisibility`|`true` / `false`|Prevent vMenu from setting the player's invisibility every frame. Useful if you have other resources that want to override vMenu's player invisibility option.|
|`vmenu_quit_session_in_rockstar_editor`|`true` / `false`|When you set this to true, it will leave the current game session if a player uses the rockstar editor button in the recording options menu.|
|`vmenu_server_info_message`|`string`|Allows you to write a very short description about your server. Used in the About menu.|
|`vmenu_server_info_website_url`|`string`|Enter a (short) website/domain name of your community here. Used in the About menu.|
|`vmenu_teleport_to_wp_keybind_key`|A valid [control ID][control]|The control ID for the Teleport To Waypoint keybind. 168 / F7 by default.|
|`vmenu_disable_spawning_as_default_character`|`true` / `false`|Setting this to `true` will globally disable the 'Spawn As Default Character' feature for everyone on the server.|
|`vmenu_enable_animals_spawn_menu`|`true` / `false`|Enables the animals menu in the player appearance » ped spawner menu.|
|`vmenu_pvp_mode`|`0`, `1` or `2`|0 = Do nothing. 1 = Enable PVP for everyone. 2 = Disable PVP for everyone.|
|`keep_player_head_props`|`true` / `false`|Sets whether or not players can lose their head props when they are hit/pushed. (Default: `true`)|
|`vmenu_player_names_distance`|Any float value|Sets the range on the player overhead names. (Default: `500.0`)|
|`vmenu_disable_entity_outlines_tool`|`true` / `false`|Disables the entity related dev-tools in the Misc Settings » Dev Tools menu.|

### Ban & Kick settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_auto_ban_cheaters`|`true` / `false`|Automatically bans people who try to trigger events when they don't have permissions for it. In rare cases this can be unreliable so it's disabled by default.|
|`vmenu_auto_ban_cheaters_ban_message`|Any string|This allows you to set a custom ban reason for whenever a player is automatically banned for cheating. (Added in vMenu v2.2.1).|
|`vmenu_log_ban_actions`|`true` / `false`|Log whenever someone gets banned to the `vmenu.log` file.|
|`vmenu_log_kick_actions`|`true` / `false`|Log whenever someone gets kicked to the `vmenu.log` file.|
|`vmenu_default_ban_message_information`|`string`|This message gets added at the end of all ban messages, use this to show users where they can contact the server staff team in case they want to appeal it or if they have any questions. (Default: `"Please contact the staff team by going to (support url) if you want to appeal this ban"`)|

### Weather settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_enable_weather_sync`|`true` / `false`|Enables/disables weather sync completely.|
|`vmenu_enable_dynamic_weather`|`true` / `false`|Enables/disables dynamic weather changes by default. It can still be toggled on/off in-game using the menu.|
|`vmenu_dynamic_weather_timer`|Any [integer][integer] (whole number)|Sets the delay between dynamic weather changes in whole minutes.|
|`vmenu_current_weather`|A valid weather type|Sets the default weather type that will be selected when the resource is loaded. (Default: `clear`)|
|`vmenu_blackout_enabled`|`true` / `false`|This allows you to set the (default) state for blackout mode.|
|`vmenu_vehicle_blackout_enabled`|`true` / `false`|Sets the (default) state for vehicle blackout mode, which disables the lights on all vehicles. This option is not present in the default `permissions.cfg`, so you'll need to add it manually if you want to change it. (Default: `false`)|
|`vmenu_weather_change_duration`|`integer`|Allows you to configure how long in seconds weather changes take, value should be between 0 and 45 for best results.|
|`vmenu_enable_snow`|`true` / `false`|Manually enable snow on the ground without having the 'xmas' weather type selected.|

### Time settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_enable_time_sync`|`true` / `false`|Enables/disables time sync completely.|
|`vmenu_freeze_time`|`true` / `false`|Enables/disables the default freeze time state.|
|`vmenu_smooth_time_transitions`|`true` / `false`|Enables smooth time transitions when the in-game time changes, instead of the time instantly jumping. (Default: `true`)|
|`vmenu_ingame_minute_duration`|Any [integer][integer] (whole number)|Sets the duration of 1 in-game minute to this value in real time (ms). Example: if this is set to 2000 (ms) then 1 in-game minute takes 2 seconds in real life. Must be at least 100 (values below 100 are reset to 2000). (Default: `2000`)|
|`vmenu_current_hour`|Any [integer][integer] (whole number)|The default in-game hour to set the time to whenever the resource is started (24 hours clock). Value must be between 0 and 23 (inclusive). (Default: `7`)|
|`vmenu_current_minute`|Any [integer][integer] (whole number)|The default in-game minute to set the time to whenever the resource is started. Value must be between 0 and 59 (inclusive). (Default: `0`)|
|`vmenu_sync_to_machine_time`|`true` / `false`|Allows you to sync the time to the server's real system time (the custom in-game hour duration setting will be ignored if this is enabled and you won't be able to change the time manually).|

### Vehicle settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_using_chameleon_colours`|`true` / `false`|Allows the use of chameleon colors for supported vehicles. You must be streaming the chameleon colours in order for this to function properly. (Default: `false`)|
|`vmenu_prevent_extras_when_damaged`|`true` / `false`|Setting this to `true` will prevent players from modifying vehicle extras to repair their vehicle when it is damaged. (Default: `false`)|
|`vmenu_allowed_engine_damage_for_extra_change`|`integer` (0-1000)|The amount of engine damage before vehicle extras are blocked from being changed (only used when `vmenu_prevent_extras_when_damaged` is `true`). Value must be between 0 and 1000 (inclusive). (Default: `800`)|
|`vmenu_allowed_body_damage_for_extra_change`|`integer` (0-1000)|The amount of body damage before vehicle extras are blocked from being changed (only used when `vmenu_prevent_extras_when_damaged` is `true`). Value must be between 0 and 1000 (inclusive). (Default: `800`)|
|`vmenu_delete_vehicle_distance`|Any float value|Sets the maximum distance (in game units) at which the `vMenu:DV` delete vehicle command will delete a vehicle. (Default: `5.0`)|
|`vmenu_vehicle_spawn_delay`|Any [integer][integer] (whole number)|Sets the delay in seconds between vehicle spawns. Value must be 0 or greater. (Default: `5`)|

### MP Ped settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_mp_ped_preview`|`true` / `false`|Setting this to `true` will enable a 3D ped preview when viewing saved MP Peds. (Default: `true`)|

### Voice Chat settings

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_override_voicechat_default_range`|Any float value|Overrides the default voice chat proximity range (in meters). Set to `0.0` to use the user/server default range. (Default: `0.0`)|

## fxmanifest.lua options

A few debugging and development options aren't convars — they're set as resource metadata in the `fxmanifest.lua` file (inside the vMenu resource folder), **not** in the `permissions.cfg`. To change one, edit the value between the quotes on the relevant line in `fxmanifest.lua`. Most servers should leave these at their defaults.

|Option|Possible values|Description|
|:--|:--|:--|
|`client_debug_mode`|`'true'` / `'false'`|Adds additional client-side logging, useful when debugging issues. (Default: `'false'`)|
|`server_debug_mode`|`'true'` / `'false'`|Adds additional server-side logging, useful when debugging issues. (Default: `'false'`)|
|`experimental_features_enabled`|`'0'` / `'1'`|Adds extra commands for testing and development. Not recommended on production servers. (Default: `'0'`)|

> The `version` field in `fxmanifest.lua` is set automatically during the build and should not be edited manually.

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!

[control]: https://docs.fivem.net/docs/game-references/controls/#controls
[integer]: https://en.wikipedia.org/wiki/Integer

## 1-click installation with Zap Hosting
[![](https://zap-hosting.com/interface/_images/banner/gameserver/fivem-affiliate-banner-1006x180.png)](https://zap-hosting.com/vespura)
Zap Hosting provides a simple 1 click installation method for vMenu! Click [this link](https://zap-hosting.com/vespura) to get a Zap server and use code `Vespura-a-3715` at checkout for 20% off your purchase!

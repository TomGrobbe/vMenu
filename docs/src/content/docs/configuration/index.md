---
title: "Configuration Options"
---

**Starting from version v1.4.0, vMenu now uses different convars to configure it's options. A list of all available options can be found below. Please note that you need to have a recent server artifacts version (around 804) for this to work. All options can also be found in the permissions.cfg, this is also the place where you can configure them. If you want, you could also move this to the server.cfg, as long as it's above the `start vMenu` line, but for most people it'll be eaiser to just keep the config options in the permissions.cfg.**

To set an option, use the following command in the server.cfg or permissions.cfg:

```yaml
setr vmenu_option_name value
```

|Option|Possible values|Description|
|:--|:--|:--|
|`vmenu_use_permissions`|`true` / `false`|This option allows you to toggle on/off the permissions system in vMenu. If you just want a plug-and-play resource and don't want to mess with configuring permissions, then use this. It will disable all admin features like banning, kicking, unbanning and similar.|
|`vmenu_menu_staff_only`|`true` / `false`|This option allows you to make this menu staff-only. Anyone with the `vMenu.Staff` permission will be able to use the menu. It will still keep in mind all other permissions, it's just a way of limiting who can open the menu.|
|`vmenu_menu_toggle_key`|A valid [control ID][control].|Sets the control ID for the key that opens vMenu (keyboard/mouse only, controller key will always be (hold) 'change view/select').|
|`vmenu_noclip_toggle_key`|A valid [control ID][control].|Sets the control ID for the noclip toggle key (keyboard/mouse only).|
|`vmenu_keep_spawned_vehicles_persistent`|`true` / `false`|Keeps spawned vehicles persistend if 'replace previous vehicle' is turned off.|
|`vmenu_enable_weather_sync`|`true` / `false`|Enables/disables weather sync completely.|
|`vmenu_enable_dynamic_weather`|`true` / `false`|Enbales/disables dynamic weather changes by default. It can still be toggled on/off in-game using the menu.|
|`vmenu_dynamic_weather_timer`|Any [integer][integer] (whole number)|Sets the delay between dynamic weather changes in whole minutes.|
|`vmenu_default_weather`|A valid weather type|Set the default weather type that will be selected when the resource is loaded.|
|`vmenu_enable_time_sync`|`true` / `false`|Enables/disables time sync completely.|
|`vmenu_freeze_time`|`true` / `false`|Enables/disables the default freeze time state.|
|`vmenu_default_time_hour`|Any [integer][integer] (whole number)|The default in-game hour to set the time to whenever the resource is started (24 hours clock).|
|`vmenu_default_time_min`|Any [integer][integer] (whole number)|The default in-game minute to set the time to whenever the resource is started.|
|`vmenu_ingame_minute_duration`|Any [integer][integer] (whole number)|(Default: 2000) Sets the duration of 1 in-game minute to this value in real time (ms). Example: if this is set to 2000 (ms) then 1 in-game minute takes 2 seconds in real life.|
|`vmenu_auto_ban_cheaters`|`true` / `false`|Automatically bans people who try to trigger events when they don't have permissions for it. In rare cases this can be unreliable so it's disabled by default.|
|`vmenu_log_ban_actions`|`true` / `false`|Log whenever someone gets banned to the `vmenu.log` file.|
|`vmenu_log_kick_actions`|`true` / `false`|Log whenever someone gets kicked to the `vmenu.log` file.|
|`vmenu_outdated_version_notify_players`|`true` / `false`|Warn players whenever they join through the use of a notification above the minimap whenever vMenu is outdated on that server.|
|`vmenu_use_els_compatibility_mode`|`true` / `false`|Enables compatibility mode for ELS resources (and other siren control scripts) by disabling vMenu's siren control option completely.|
|`vmenu_quit_session_in_rockstar_editor`|`true` / `false`|When you set this to true, it will leave the current game session if a player uses the rockstar editor button in the recording options menu.|
|`vmenu_server_info_message`|`string`|Allows you to write a very short description about your server. Used in the About menu.|
|`vmenu_server_info_website_url`|`string`|Enter a (short) website/domain name of your community here. Used in the About menu.|
|`vmenu_teleport_to_wp_keybind_key`|A valid [control ID][control]|The control ID for the Teleport To Waypoint keybind. 168 / F7 by default.|
|`vmenu_disable_spawning_as_default_character`|`true` / `false`|Setting this to `true` will globally disable the 'Spawn As Default Character' feature for everyone on the server.|
|`vmenu_auto_ban_cheaters_ban_message`|Any string|This allows you to set a custom ban reason for whenever a player is automatically banned for cheating. (Added in vMenu v2.2.1).|
|`vmenu_disable_daily_update_checks`|`true` / `false`|Allows you to disable the daily update checks.|
|`vmenu_enable_animals_spawn_menu`|`true` / `false`|Enables the animals menu in the player appearance » ped spawner menu.|
|`vmenu_pvp_mode`|`0`, `1` or `2`|0 = Do nothing. 1 = Enable PVP for everyone. 2 = Disable PVP for everyone.|
|`vmenu_disable_server_info_convars`|`true` / `false`|Disables the server info convars.|
|`vmenu_player_names_distance`|Any float value|Default `500.0`, sets the range on the player overhead names.|
|`vmenu_disable_entity_outlines_tool`|`true` / `false`|Disables the entity related dev-tools in the Misc Settings » Dev Tools menu.|
|`vmenu_default_ban_message_information`|`string`|Default: `"Please contact the staff team by going to (support url) if you want to appeal this ban"`, This message gets added at the end of all ban messages, use this to show users where they can contact the server staff team in case they want to appeal it or if they have any questions.|
|`vmenu_blackout_enabled`|`true` / `false`|This allows you to set the (default) state for blackout mode.|
|`vmenu_sync_to_machine_time`|`true` / `false`|Allows you to sync the time to the server's real system time (the custom in-game hour duration setting will be ignored if this is enabled and you won't be able to change the time manually).|
|`vmenu_weather_change_duration`|`integer`|Allows you to configure how long in seconds weather changes take, value should be between 0 and 45 for best results.|
|`vmenu_handle_invisibility`|`true` / `false`|Prevent vMenu from setting the player's invisibility every frame. Useful if you have other resources that want to override vMenu's player invisibility option.|
|`vmenu_disable_player_stats_setup`|`true` / `false`|Prevent vMenu from setting any player stats like stamina, shooting, driving ability, etc.|
|`vmenu_enable_snow`|`true` / `false`|Manually enable snow on the ground without having the 'xmas' weather type selected.|
|`vmenu_using_chameleon_colours`|`true` / `false`|Allows the use of chameleon colors for supported vehicles. (Default: `false`)|
|`keep_player_head_props`|`true` / `false`|Sets whether or not players can lose their head props when they are hit/pushed. (Default `true`)|

## Appreciate my work?
Consider supporting me on [Patreon](https://www.patreon.com/vespura)!

[control]: https://docs.fivem.net/game-references/controls/#controls
[integer]: https://en.wikipedia.org/wiki/Integer

## 1-click installation with Zap Hosting
[![](https://zap-hosting.com/interface/_images/banner/gameserver/fivem-affiliate-banner-1006x180.png)](https://zap-hosting.com/vespura)
Zap Hosting provides a simple 1 click installation method for vMenu! Click [this link](https://zap-hosting.com/vespura) to get a Zap server and use code `Vespura-a-3715` at checkout for 20% off your purchase!

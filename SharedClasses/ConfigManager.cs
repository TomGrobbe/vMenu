using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace redMenuShared
{
    public static class ConfigManager
    {
        public enum Setting
        {
            // General settings
            redmenu_use_permissions,
            redmenu_menu_staff_only,
            redmenu_menu_toggle_key,
            redmenu_noclip_toggle_key,
            redmenu_server_info_message,
            redmenu_server_info_website_url,
            redmenu_teleport_to_wp_keybind_key,
            redmenu_pvp_mode,
            redmenu_disable_server_info_convars,
            redmenu_player_names_distance,
            redmenu_disable_entity_outlines_tool,
            redmenu_disable_player_stats_setup,

            // Kick & ban settings
            redmenu_default_ban_message_information,
            redmenu_auto_ban_cheaters,
            redmenu_auto_ban_cheaters_ban_message,
            redmenu_log_ban_actions,
            redmenu_log_kick_actions,

            // Weather settings
            redmenu_enable_weather_sync,
            redmenu_current_weather,
            redmenu_blackout_enabled,
            redmenu_weather_change_duration,
            redmenu_enable_snow,

            // Time settings
            redmenu_enable_time_sync,
            redmenu_freeze_time,
            redmenu_ingame_minute_duration,
            redmenu_current_hour,
            redmenu_current_minute,
            redmenu_sync_to_machine_time,

            // Voice Chat Settings
            redmenu_override_voicechat_default_range,

            // Key Mapping
            redmenu_keymapping_id,
        }

        /// <summary>
        /// Get a boolean setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static bool GetSettingsBool(Setting setting)
        {
            return GetConvar(setting.ToString(), "false") == "true";
        }

        /// <summary>
        /// Get an integer setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static int GetSettingsInt(Setting setting)
        {
            var convarInt = GetConvarInt(setting.ToString(), -1);
            if (convarInt == -1)
            {
                if (int.TryParse(GetConvar(setting.ToString(), "-1"), out var convarIntAlt))
                {
                    return convarIntAlt;
                }
            }
            return convarInt;
        }

        /// <summary>
        /// Get a float setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static float GetSettingsFloat(Setting setting)
        {
            if (float.TryParse(GetConvar(setting.ToString(), "-1.0"), out var result))
            {
                return result;
            }
            return -1f;
        }

        /// <summary>
        /// Get a string setting.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetSettingsString(Setting setting, string defaultValue = null)
        {
            var value = GetConvar(setting.ToString(), defaultValue ?? "");
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return value;
        }

        /// <summary>
        /// Debugging mode
        /// </summary>
        public static bool DebugMode => IsDuplicityVersion() ? IsServerDebugModeEnabled() : IsClientDebugModeEnabled();

        /// <summary>
        /// Default value for server debugging mode.
        /// </summary>
        /// <returns></returns>
        public static bool IsServerDebugModeEnabled()
        {
            return GetResourceMetadata("redMenu", "server_debug_mode", 0).ToLower() == "true";
        }

        /// <summary>
        /// Default value for client debugging mode.
        /// </summary>
        /// <returns></returns>
        public static bool IsClientDebugModeEnabled()
        {
            return GetResourceMetadata("redMenu", "client_debug_mode", 0).ToLower() == "true";
        }

        #region Get saved locations from the locations.json
        /// <summary>
        /// Gets the locations.json data.
        /// </summary>
        /// <returns></returns>
        public static Locations GetLocations()
        {
            var data = new Locations();

            var jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
#if CLIENT
                    redMenuClient.Notify.Error("The locations.json file is empty or does not exist, please tell the server owner to fix this.");
#endif
#if SERVER
                    redMenuServer.DebugLog.Log("The locations.json file is empty or does not exist, please fix this.", redMenuServer.DebugLog.LogLevel.error);
#endif
                }
                else
                {
                    data = JsonConvert.DeserializeObject<Locations>(jsonFile);
                }
            }
            catch (Exception e)
            {
#if CLIENT
                    redMenuClient.Notify.Error("An error occurred while processing the locations.json file. Teleport Locations and Location Blips will be unavailable. Please correct any errors in the locations.json file.");
#endif
                Debug.WriteLine($"[redMenu] json exception details: {e.Message}\nStackTrace:\n{e.StackTrace}");
            }

            return data;
        }

        /// <summary>
        /// Gets just the teleport locations data from the locations.json.
        /// </summary>
        /// <returns></returns>
        public static List<TeleportLocation> GetTeleportLocationsData()
        {
            return GetLocations().teleports;
        }

        /// <summary>
        /// Gets just the blips data from the locations.json.
        /// </summary>
        /// <returns></returns>
        public static List<LocationBlip> GetLocationBlipsData()
        {
            return GetLocations().blips;
        }

        /// <summary>
        /// Struct used for deserializing json only.
        /// </summary>
        public struct Locations
        {
            public List<TeleportLocation> teleports;
            public List<LocationBlip> blips;
        }

        /// <summary>
        /// Teleport location struct.
        /// </summary>
        public struct TeleportLocation
        {
            public string name;
            public Vector3 coordinates;
            public float heading;

            public TeleportLocation(string name, Vector3 coordinates, float heading)
            {
                this.name = name;
                this.coordinates = coordinates;
                this.heading = heading;
            }
        }

        /// <summary>
        /// Location blip struct.
        /// </summary>
        public struct LocationBlip
        {
            public string name;
            public Vector3 coordinates;
            public int spriteID;
            public int color;

            public LocationBlip(string name, Vector3 coordinates, int spriteID, int color)
            {
                this.name = name;
                this.coordinates = coordinates;
                this.spriteID = spriteID;
                this.color = color;
            }
        }
        #endregion
    }
}

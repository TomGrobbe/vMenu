﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuShared
{
    public static class ConfigManager
    {
        public enum Setting
        {
            vmenu_use_permissions,
            vmenu_menu_staff_only,

            vmenu_menu_toggle_key,
            vmenu_noclip_toggle_key,

            vmenu_keep_spawned_vehicles_persistent,

            vmenu_enable_weather_sync,
            vmenu_enable_dynamic_weather,
            vmenu_dynamic_weather_timer,
            vmenu_default_weather,
            vmenu_allow_random_blackout,

            vmenu_enable_time_sync,
            vmenu_freeze_time,
            vmenu_default_time_hour,
            vmenu_default_time_min,
            vmenu_ingame_minute_duration,

            vmenu_auto_ban_cheaters,
            vmenu_auto_ban_cheaters_ban_message,
            vmenu_log_ban_actions,
            vmenu_log_kick_actions,
            vmenu_outdated_version_notify_players,

            vmenu_use_els_compatibility_mode,
            vmenu_quit_session_in_rockstar_editor,

            vmenu_server_info_message,
            vmenu_server_info_website_url,

            vmenu_teleport_to_wp_keybind_key,

            vmenu_disable_spawning_as_default_character,
        }

        public static bool GetSettingsBool(Setting setting)
        {
            return GetConvar(setting.ToString(), "false") == "true";
        }

        public static int GetSettingsInt(Setting setting)
        {
            int convarInt = GetConvarInt(setting.ToString(), -1);
            if (convarInt == -1)
            {
                if (int.TryParse(GetConvar(setting.ToString(), "-1"), out int convarIntAlt))
                {
                    return convarIntAlt;
                }
            }
            return convarInt;
        }

        public static float GetSettingsFloat(Setting setting)
        {
            if (float.TryParse(GetConvar(setting.ToString(), "-1.0"), out float result))
            {
                return result;
            }
            return -1f;
        }

        public static string GetSettingsString(Setting setting)
        {
            return GetConvar(setting.ToString(), "");
        }

        public static bool DebugMode => IsDuplicityVersion() ? IsServerDebugModeEnabled() : IsClientDebugModeEnabled();

        public static bool IsServerDebugModeEnabled()
        {
            return GetResourceMetadata("vMenu", "server_debug_mode", 0).ToLower() == "true";
        }

        public static bool IsClientDebugModeEnabled()
        {
            return GetResourceMetadata("vMenu", "client_debug_mode", 0).ToLower() == "true";
        }
    }
}

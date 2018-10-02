using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    //public static class Configuration
    //{
    //    public enum Setting
    //    {
    //        use_permissions,
    //        menu_staff_only,

    //        menu_toggle_key,
    //        noclip_toggle_key,

    //        keep_spawned_vehicles_persistent,

    //        enable_weather_sync,
    //        enable_dynamic_weather,

    //        dynamic_weather_timer,
    //        default_weather,
    //        allow_random_blackout,

    //        enable_time_sync,
    //        default_time_hour,
    //        default_time_min,

    //        auto_ban_cheaters,
    //        log_ban_actions,
    //        log_kick_actions,
    //        oudated_version_notify_players
    //    }

    //    public enum SettingsCategory
    //    {
    //        permissions,
    //        general,
    //        vehicles,
    //        weather,
    //        time,
    //        system
    //    }

    //    private static Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

    //    private static bool initialized = false;

    //    public static bool InitializeConfig()
    //    {
    //        if (!initialized)
    //        {
    //            string file = LoadResourceFile("vMenu", "config/config.ini");
    //            if (!string.IsNullOrEmpty(file))
    //            {
    //                initialized = true;
    //                return ParseConfig(file);
    //            }
    //            SendErrorMessage("File could not be found or it's empty.");
    //        }
    //        initialized = true;
    //        return false;
    //    }

    //    private static bool ParseConfig(string config)
    //    {
    //        if (!string.IsNullOrEmpty(config))
    //        {
    //            config = config.Replace("\r", "");
    //            string lastKey = "";
    //            foreach (string line in config.Split('\n'))
    //            {
    //                if (!(string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) && !line.StartsWith(";") && !line.StartsWith("#"))
    //                {
    //                    if (line.StartsWith("[") && line.EndsWith("]"))
    //                    {
    //                        lastKey = line.Replace("[", "").Replace("]", "");
    //                        Config[lastKey] = new Dictionary<string, string>();
    //                    }
    //                    else
    //                    {
    //                        if (!string.IsNullOrEmpty(lastKey) && Config[lastKey] != null)
    //                        {
    //                            string key = line.Split('=')[0];
    //                            string value = line.Split('=')[1];

    //                            if (key != null && value != null)
    //                            {
    //                                Config[lastKey].Add(key, value);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            Debug.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Config ?? new Dictionary<string, Dictionary<string, string>>(), Newtonsoft.Json.Formatting.Indented) + "\n");
    //            return true;
    //        }

    //        SendErrorMessage("The config was empty or the file does not exist.");
    //        return false;
    //    }


    //    private static void SendErrorMessage(string details = null)
    //    {
    //        if (string.IsNullOrEmpty(details))
    //        {
    //            Debug.WriteLine("\n\n[vMenu] Error loading config file! Please notify the server owner if you see this.\n\n");
    //        }
    //        else
    //        {
    //            Debug.WriteLine($"\n\n[vMenu] Error loading config file! Please notify the server owner if you see this.\n\nError details: {details}\n\n");
    //        }
    //    }


    //    public static bool GetSettingsBool(SettingsCategory cat, Setting setting)
    //    {
    //        if (Config.ContainsKey(cat.ToString()) && Config[cat.ToString()].ContainsKey(setting.ToString()))
    //        {
    //            return Config[cat.ToString()][setting.ToString()].ToLower() == "true";
    //        }
    //        return false;
    //    }

    //    public static int GetSettingsInt(SettingsCategory cat, Setting setting)
    //    {
    //        if (Config.ContainsKey(cat.ToString()) && Config[cat.ToString()].ContainsKey(setting.ToString()))
    //        {
    //            if (int.TryParse(Config[cat.ToString()][setting.ToString()], out int result))
    //            {
    //                return result;
    //            }
    //        }
    //        return -1;
    //    }

    //    public static float GetSettingsFloat(SettingsCategory cat, Setting setting)
    //    {
    //        if (Config.ContainsKey(cat.ToString()) && Config[cat.ToString()].ContainsKey(setting.ToString()))
    //        {
    //            if (float.TryParse(Config[cat.ToString()][setting.ToString()], out float result))
    //            {
    //                return result;
    //            }
    //        }
    //        return -1f;
    //    }

    //    public static string GetSettingsString(SettingsCategory cat, Setting setting)
    //    {
    //        if (Config.ContainsKey(cat.ToString()) && Config[cat.ToString()].ContainsKey(setting.ToString()))
    //        {
    //            return Config[cat.ToString()][setting.ToString()];
    //        }
    //        return null;
    //    }



    //}
}

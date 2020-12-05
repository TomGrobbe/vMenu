using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using static vMenuServer.DebugLog;
using static vMenuShared.ConfigManager;
using vMenuShared;

namespace vMenuServer
{

    public static class DebugLog
    {
        public enum LogLevel
        {
            error = 1,
            success = 2,
            info = 4,
            warning = 3,
            none = 0
        }

        /// <summary>
        /// Global log data function, only logs when debugging is enabled.
        /// </summary>
        /// <param name="data"></param>
        public static void Log(dynamic data, LogLevel level = LogLevel.none)
        {
            if (MainServer.DebugMode || level == LogLevel.error || level == LogLevel.warning)
            {
                string prefix = "[vMenu] ";
                if (level == LogLevel.error)
                {
                    prefix = "^1[vMenu] [ERROR]^7 ";
                }
                else if (level == LogLevel.info)
                {
                    prefix = "^5[vMenu] [INFO]^7 ";
                }
                else if (level == LogLevel.success)
                {
                    prefix = "^2[vMenu] [SUCCESS]^7 ";
                }
                else if (level == LogLevel.warning)
                {
                    prefix = "^3[vMenu] [WARNING]^7 ";
                }
                Debug.WriteLine($"{prefix}[DEBUG LOG] {data.ToString()}");
            }
        }
    }

    public class MainServer : BaseScript
    {
        #region vars
        // Debug shows more information when doing certain things. Leave it off to improve performance!
        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "server_debug_mode", 0) == "true";

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        // Time
        private int CurrentHours
        {
            get { return MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_hour), 0, 23); }
            set { SetConvarReplicated(Setting.vmenu_current_hour.ToString(), MathUtil.Clamp(value, 0, 23).ToString()); }
        }
        private int CurrentMinutes
        {
            get { return MathUtil.Clamp(GetSettingsInt(Setting.vmenu_current_minute), 0, 59); }
            set { SetConvarReplicated(Setting.vmenu_current_minute.ToString(), MathUtil.Clamp(value, 0, 59).ToString()); }
        }
        private int MinuteClockSpeed
        {
            get
            {
                var value = GetSettingsInt(Setting.vmenu_ingame_minute_duration);
                if (value < 100) value = 2000;
                return value;
            }
        }
        private bool FreezeTime
        {
            get { return GetSettingsBool(Setting.vmenu_freeze_time); }
            set { SetConvarReplicated(Setting.vmenu_freeze_time.ToString(), value.ToString().ToLower()); }
        }
        private bool IsServerTimeSynced { get { return GetSettingsBool(Setting.vmenu_sync_to_machine_time); } }


        // Weather
        private string CurrentWeather
        {
            get
            {
                var value = GetSettingsString(Setting.vmenu_current_weather, "CLEAR");
                if (!WeatherTypes.Contains(value.ToUpper()))
                {
                    return "CLEAR";
                }
                return value;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || !WeatherTypes.Contains(value.ToUpper()))
                {
                    SetConvarReplicated(Setting.vmenu_current_weather.ToString(), "CLEAR");
                }
                SetConvarReplicated(Setting.vmenu_current_weather.ToString(), value.ToUpper());
            }
        }
        private bool DynamicWeatherEnabled
        {
            get { return GetSettingsBool(Setting.vmenu_enable_dynamic_weather); }
            set { SetConvarReplicated(Setting.vmenu_enable_dynamic_weather.ToString(), value.ToString().ToLower()); }
        }
        private bool ManualSnowEnabled
        {
            get { return GetSettingsBool(Setting.vmenu_enable_snow); }
            set { SetConvarReplicated(Setting.vmenu_enable_snow.ToString(), value.ToString().ToLower()); }
        }
        private bool BlackoutEnabled
        {
            get { return GetSettingsBool(Setting.vmenu_blackout_enabled); }
            set { SetConvarReplicated(Setting.vmenu_blackout_enabled.ToString(), value.ToString().ToLower()); }
        }
        private int DynamicWeatherMinutes
        {
            get { return Math.Max(GetSettingsInt(Setting.vmenu_dynamic_weather_timer), 1); }
        }
        private long lastWeatherChange = 0;

        private readonly List<string> CloudTypes = new List<string>()
        {
            "Cloudy 01",
            "RAIN",
            "horizonband1",
            "horizonband2",
            "Puffs",
            "Wispy",
            "Horizon",
            "Stormy 01",
            "Clear 01",
            "Snowy 01",
            "Contrails",
            "altostratus",
            "Nimbus",
            "Cirrus",
            "cirrocumulus",
            "stratoscumulus",
            "horizonband3",
            "Stripey",
            "horsey",
            "shower",
        };
        private readonly List<string> WeatherTypes = new List<string>()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainServer()
        {
            // name check
            if (GetCurrentResourceName() != "vMenu")
            {
                Exception InvalidNameException = new Exception("\r\n\r\n^1[vMenu] INSTALLATION ERROR!\r\nThe name of the resource is not valid. " +
                    "Please change the folder name from '^3" + GetCurrentResourceName() + "^1' to '^2vMenu^1' (case sensitive) instead!\r\n\r\n\r\n^7");
                try
                {
                    throw InvalidNameException;
                }
                catch (Exception e)
                {
                    Debug.Write(e.Message);
                }
            }
            else
            {
                // Add event handlers.
                EventHandlers.Add("vMenu:GetPlayerIdentifiers", new Action<int, NetworkCallbackDelegate>((TargetPlayer, CallbackFunction) =>
                {
                    List<string> data = new List<string>();
                    Players[TargetPlayer].Identifiers.ToList().ForEach(e =>
                    {
                        if (!e.Contains("ip:"))
                            data.Add(e);
                    });
                    CallbackFunction(JsonConvert.SerializeObject(data));
                }));
                EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(PermissionsManager.SetPermissionsForPlayer));
                EventHandlers.Add("vMenu:RequestServerState", new Action<Player>(RequestServerStateFromPlayer));

                // check addons file for errors
                string addons = LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                try
                {
                    JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                    // If the above crashes, then the json is invalid and it'll throw warnings in the console.
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^7Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
                }

                // check if permissions are setup (correctly)
                if (!GetSettingsBool(Setting.vmenu_use_permissions))
                {
                    Debug.WriteLine("^3[vMenu] [WARNING] vMenu is set up to ignore permissions!\nIf you did this on purpose then you can ignore this warning.\nIf you did not set this on purpose, then you must have made a mistake while setting up vMenu.\nPlease read the vMenu documentation (^5https://docs.vespura.com/vmenu^3).\nMost likely you are not executing the permissions.cfg (correctly).^7");
                }

                Tick += PlayersFirstTick;

                // Start the loops
                if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
                    Tick += WeatherLoop;
                if (GetSettingsBool(Setting.vmenu_enable_time_sync))
                    Tick += TimeLoop;
            }
        }
        #endregion

        #region command handler
        [Command("vmenuserver", Restricted = true)]
        private void ServerCommandHandler(int source, List<object> args, string rawCommand)
        {
            if (args != null)
            {
                if (args.Count > 0)
                {
                    if (args[0].ToString().ToLower() == "debug")
                    {
                        DebugMode = !DebugMode;
                        if (source < 1)
                        {
                            Debug.WriteLine($"Debug mode is now set to: {DebugMode}.");
                        }
                        else
                        {
                            Players[source].TriggerEvent("chatMessage", $"vMenu Debug mode is now set to: {DebugMode}.");
                        }
                        return;
                    }
                    else if (args[0].ToString().ToLower() == "unban" && (source < 1))
                    {
                        if (args.Count() > 1 && !string.IsNullOrEmpty(args[1].ToString()))
                        {
                            var uuid = args[1].ToString().Trim();
                            var bans = BanManager.GetBanList();
                            var banRecord = bans.Find(b => { return b.uuid.ToString() == uuid; });
                            if (banRecord != null)
                            {
                                BanManager.RemoveBan(banRecord);
                                Debug.WriteLine("Player has been successfully unbanned.");
                            }
                            else
                            {
                                Debug.WriteLine($"Could not find a banned player with the provided uuid '{uuid}'.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("You did not specify a player to unban, you must enter the FULL playername. Usage: vmenuserver unban \"playername\"");
                        }
                        return;
                    }
                    else if (args[0].ToString().ToLower() == "weather")
                    {
                        if (args.Count < 2 || string.IsNullOrEmpty(args[1].ToString()))
                        {
                            Debug.WriteLine("[vMenu] Invalid command syntax. Use 'vmenuserver weather <weatherType>' instead.");
                        }
                        else
                        {
                            string wtype = args[1].ToString().ToUpper();
                            if (WeatherTypes.Contains(wtype))
                            {
                                TriggerEvent("vMenu:UpdateServerWeather", wtype, BlackoutEnabled, DynamicWeatherEnabled, ManualSnowEnabled);
                                Debug.WriteLine($"[vMenu] Weather is now set to: {wtype}");
                            }
                            else if (wtype.ToLower() == "dynamic")
                            {
                                if (args.Count == 3 && !string.IsNullOrEmpty(args[2].ToString()))
                                {
                                    if ((args[2].ToString().ToLower() ?? $"{DynamicWeatherEnabled}") == "true")
                                    {
                                        TriggerEvent("vMenu:UpdateServerWeather", CurrentWeather, BlackoutEnabled, true, ManualSnowEnabled);
                                        Debug.WriteLine("[vMenu] Dynamic weather is now turned on.");
                                    }
                                    else if ((args[2].ToString().ToLower() ?? $"{DynamicWeatherEnabled}") == "false")
                                    {
                                        TriggerEvent("vMenu:UpdateServerWeather", CurrentWeather, BlackoutEnabled, false, ManualSnowEnabled);
                                        Debug.WriteLine("[vMenu] Dynamic weather is now turned off.");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("[vMenu] Invalid command usage. Correct syntax: vmenuserver weather dynamic <true|false>");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("[vMenu] Invalid command usage. Correct syntax: vmenuserver weather dynamic <true|false>");
                                }

                            }
                            else
                            {
                                Debug.WriteLine("[vMenu] This weather type is not valid!");
                            }
                        }
                    }
                    else if (args[0].ToString().ToLower() == "time")
                    {
                        if (args.Count == 2)
                        {
                            if (args[1].ToString().ToLower() == "freeze")
                            {
                                TriggerEvent("vMenu:UpdateServerTime", CurrentHours, CurrentMinutes, !FreezeTime);
                                Debug.WriteLine($"Time is now {(FreezeTime ? "frozen" : "not frozen")}.");
                            }
                            else
                            {
                                Debug.WriteLine("Invalid syntax. Use: ^5vmenuserver time <freeze|<hour> <minute>>^7 instead.");
                            }
                        }
                        else if (args.Count > 2)
                        {
                            if (int.TryParse(args[1].ToString(), out int hour))
                            {
                                if (int.TryParse(args[2].ToString(), out int minute))
                                {
                                    if (hour >= 0 && hour < 24)
                                    {
                                        if (minute >= 0 && minute < 60)
                                        {
                                            TriggerEvent("vMenu:UpdateServerTime", hour, minute, FreezeTime);
                                            Debug.WriteLine($"Time is now {(hour < 10 ? ("0" + hour.ToString()) : hour.ToString())}:{(minute < 10 ? ("0" + minute.ToString()) : minute.ToString())}.");
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Invalid minute provided. Value must be between 0 and 59.");
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Invalid hour provided. Value must be between 0 and 23.");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Invalid syntax. Use: ^5vmenuserver time <freeze|<hour> <minute>>^7 instead.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Invalid syntax. Use: ^5vmenuserver time <freeze|<hour> <minute>>^7 instead.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Invalid syntax. Use: ^5vmenuserver time <freeze|<hour> <minute>>^7 instead.");
                        }
                    }
                    else if (args[0].ToString().ToLower() == "ban" && source < 1)  // only do this via server console (server id < 1)
                    {
                        if (args.Count > 3)
                        {
                            Player p = null;

                            bool findByServerId = args[1].ToString().ToLower() == "id";
                            string identifier = args[2].ToString().ToLower();

                            if (findByServerId)
                            {
                                if (Players.Any(player => player.Handle == identifier))
                                {
                                    p = Players.Single(pl => pl.Handle == identifier);
                                }
                                else
                                {
                                    Debug.WriteLine("[vMenu] Could not find this player, make sure they are online.");
                                    return;
                                }
                            }
                            else
                            {
                                if (Players.Any(player => player.Name.ToLower() == identifier.ToLower()))
                                {
                                    p = Players.Single(pl => pl.Name.ToLower() == identifier.ToLower());
                                }
                                else
                                {
                                    Debug.WriteLine("[vMenu] Could not find this player, make sure they are online.");
                                    return;
                                }
                            }

                            string reason = "Banned by staff for:";
                            args.GetRange(3, args.Count - 3).ForEach(arg => reason += " " + arg);

                            if (p != null)
                            {
                                BanManager.BanRecord ban = new BanManager.BanRecord(
                                    BanManager.GetSafePlayerName(p.Name),
                                    p.Identifiers.ToList(),
                                    new DateTime(3000, 1, 1),
                                    reason,
                                    "Server Console",
                                    new Guid()
                                );

                                BanManager.AddBan(ban);
                                BanManager.BanLog($"[vMenu] Player {p.Name}^7 has been banned by Server Console for [{reason}].");
                                TriggerEvent("vMenu:BanSuccessful", JsonConvert.SerializeObject(ban).ToString());
                                string timeRemaining = BanManager.GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                                p.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                            }
                            else
                            {
                                Debug.WriteLine("[vMenu] Player not found, could not ban player.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("[vMenu] Not enough arguments, syntax: ^5vmenuserver ban <id|name> <server id|username> <reason>^7.");
                        }
                    }
                    else if (args[0].ToString().ToLower() == "help")
                    {
                        Debug.WriteLine("Available commands:");
                        Debug.WriteLine("(server console only): vmenuserver ban <id|name> <server id|username> <reason> (player must be online!)");
                        Debug.WriteLine("(server console only): vmenuserver unban <uuid>");
                        Debug.WriteLine("vmenuserver weather <new weather type | dynamic <true | false>>");
                        Debug.WriteLine("vmenuserver time <freeze|<hour> <minute>>");
                        Debug.WriteLine("vmenuserver migrate (This copies all banned players in the bans.json file to the new ban system in vMenu v3.3.0, you only need to do this once)");
                    }
                    else if (args[0].ToString().ToLower() == "migrate" && source < 1)
                    {
                        string file = LoadResourceFile(GetCurrentResourceName(), "bans.json");
                        if (string.IsNullOrEmpty(file) || file == "[]")
                        {
                            Debug.WriteLine("&1[vMenu] [ERROR]^7 No bans.json file found or it's empty.");
                            return;
                        }
                        Debug.WriteLine("^5[vMenu] [INFO]^7 Importing all ban records from the bans.json file into the new storage system. ^3This may take some time...^7");
                        var bans = JsonConvert.DeserializeObject<List<BanManager.BanRecord>>(file);
                        bans.ForEach((br) =>
                        {
                            var record = new BanManager.BanRecord(br.playerName, br.identifiers, br.bannedUntil, br.banReason, br.bannedBy, Guid.NewGuid());
                            BanManager.AddBan(record);
                        });
                        Debug.WriteLine("^2[vMenu] [SUCCESS]^7 All ban records have been imported. You now no longer need the bans.json file.");
                    }
                    else
                    {
                        Debug.WriteLine($"vMenu is currently running version: {Version}. Try ^5vmenuserver help^7 for info.");
                    }
                }
                else
                {
                    Debug.WriteLine($"vMenu is currently running version: {Version}. Try ^5vmenuserver help^7 for info.");
                }
            }
            else
            {
                Debug.WriteLine($"vMenu is currently running version: {Version}. Try ^5vmenuserver help^7 for info.");
            }
        }
        #endregion

        #region kick players from personal vehicle
        /// <summary>
        /// Makes the player leave the personal vehicle.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="vehicleNetId"></param>
        /// <param name="playerOwner"></param>
        [EventHandler("vMenu:GetOutOfCar")]
        private void GetOutOfCar([FromSource] Player source, int vehicleNetId, int playerOwner)
        {
            if (source != null)
            {
                if (vMenuShared.PermissionsManager.GetPermissionAndParentPermissions(vMenuShared.PermissionsManager.Permission.PVKickPassengers).Any(perm => vMenuShared.PermissionsManager.IsAllowed(perm, source)))
                {
                    TriggerClientEvent("vMenu:GetOutOfCar", vehicleNetId, playerOwner);
                    source.TriggerEvent("vMenu:Notify", "All passengers will be kicked out as soon as the vehicle stops moving, or after 10 seconds if they refuse to stop the vehicle.");
                }
            }
        }
        #endregion

        #region clear area near pos
        /// <summary>
        /// Clear the area near this point for all players.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        [EventHandler("vMenu:ClearArea")]
        private void ClearAreaNearPos(float x, float y, float z)
        {
            TriggerClientEvent("vMenu:ClearArea", x, y, z);
        }
        #endregion

        #region Manage weather and time changes.
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            if (IsServerTimeSynced)
            {
                var currentTime = DateTime.Now;
                CurrentMinutes = currentTime.Minute;
                CurrentHours = currentTime.Hour;

                // Update this once every 60 seconds.
                await Delay(60000);
            }
            else
            {
                if (!FreezeTime)
                {
                    if ((CurrentMinutes + 1) > 59)
                    {
                        CurrentMinutes = 0;
                        if ((CurrentHours + 1) > 23)
                        {
                            CurrentHours = 0;
                        }
                        else
                        {
                            CurrentHours++;
                        }
                    }
                    else
                    {
                        CurrentMinutes++;
                    }
                }
                await Delay(MinuteClockSpeed);
            }
        }

        /// <summary>
        /// Task used for syncing and changing weather dynamically.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherLoop()
        {
            if (DynamicWeatherEnabled)
            {
                await Delay(DynamicWeatherMinutes * 60000);

                if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
                {
                    // Manage dynamic weather changes.

                    {
                        // Disable dynamic weather because these weather types shouldn't randomly change.
                        if (CurrentWeather == "XMAS" || CurrentWeather == "HALLOWEEN" || CurrentWeather == "NEUTRAL")
                        {
                            DynamicWeatherEnabled = false;
                            return;
                        }

                        // Is it time to generate a new weather type?
                        if (GetGameTimer() - lastWeatherChange > (DynamicWeatherMinutes * 60000))
                        {
                            // Choose a new semi-random weather type.
                            RefreshWeather();

                            // Log if debug mode is on how long the change has taken and what the new weather type will be.
                            if (DebugMode)
                            {
                                Log($"Changing weather, new weather: {CurrentWeather}");
                            }
                        }
                    }
                }
            }
            else
            {
                await Delay(5000);
            }
        }

        /// <summary>
        /// Select a new random weather type, based on the current weather and some patterns.
        /// </summary>
        private void RefreshWeather()
        {
            var random = new Random().Next(20);
            if (CurrentWeather == "RAIN" || CurrentWeather == "THUNDER")
            {
                CurrentWeather = "CLEARING";
            }
            else if (CurrentWeather == "CLEARING")
            {
                CurrentWeather = "CLOUDS";
            }
            else
            {
                switch (random)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        CurrentWeather = (CurrentWeather == "EXTRASUNNY" ? "CLEAR" : "EXTRASUNNY");
                        break;
                    case 6:
                    case 7:
                    case 8:
                        CurrentWeather = (CurrentWeather == "SMOG" ? "FOGGY" : "SMOG");
                        break;
                    case 9:
                    case 10:
                    case 11:
                        CurrentWeather = (CurrentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 12:
                    case 13:
                    case 14:
                        CurrentWeather = (CurrentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 15:
                        CurrentWeather = (CurrentWeather == "OVERCAST" ? "THUNDER" : "OVERCAST");
                        break;
                    case 16:
                        CurrentWeather = (CurrentWeather == "CLOUDS" ? "EXTRASUNNY" : "RAIN");
                        break;
                    case 17:
                    case 18:
                    case 19:
                    default:
                        CurrentWeather = (CurrentWeather == "FOGGY" ? "SMOG" : "FOGGY");
                        break;
                }
            }

        }
        #endregion

        #region Sync weather & time with clients
        /// <summary>
        /// Update the weather for all clients.
        /// </summary>
        /// <param name="newWeather"></param>
        /// <param name="blackoutNew"></param>
        /// <param name="dynamicWeatherNew"></param>
        [EventHandler("vMenu:UpdateServerWeather")]
        private void UpdateWeather(string newWeather, bool blackoutNew, bool dynamicWeatherNew, bool enableSnow)
        {

            // Automatically enable snow effects whenever one of the snow weather types is selected.
            if (newWeather == "XMAS" || newWeather == "SNOWLIGHT" || newWeather == "SNOW" || newWeather == "BLIZZARD")
            {
                enableSnow = true;
            }

            // Update the new weather related variables.
            CurrentWeather = newWeather;
            BlackoutEnabled = blackoutNew;
            DynamicWeatherEnabled = dynamicWeatherNew;
            ManualSnowEnabled = enableSnow;

            // Reset the dynamic weather loop timer to another (default) 10 mintues.
            lastWeatherChange = GetGameTimer();
        }

        /// <summary>
        /// Set a new random clouds type and opacity for all clients.
        /// </summary>
        /// <param name="removeClouds"></param>
        [EventHandler("vMenu:UpdateServerWeatherCloudsType")]
        private void UpdateWeatherCloudsType(bool removeClouds)
        {
            if (removeClouds)
            {
                TriggerClientEvent("vMenu:SetClouds", 0f, "removed");
            }
            else
            {
                float opacity = float.Parse(new Random().NextDouble().ToString());
                string type = CloudTypes[new Random().Next(0, CloudTypes.Count)];
                TriggerClientEvent("vMenu:SetClouds", opacity, type);
            }
        }

        /// <summary>
        /// Set and sync the time to all clients.
        /// </summary>
        /// <param name="newHours"></param>
        /// <param name="newMinutes"></param>
        /// <param name="freezeTimeNew"></param>
        [EventHandler("vMenu:UpdateServerTime")]
        private void UpdateTime(int newHours, int newMinutes, bool freezeTimeNew)
        {
            CurrentHours = newHours;
            CurrentMinutes = newMinutes;
            FreezeTime = freezeTimeNew;
        }
        #endregion

        #region Online Players Menu Actions
        /// <summary>
        /// Kick a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="kickReason"></param>
        [EventHandler("vMenu:KickPlayer")]
        private void KickPlayer([FromSource] Player source, int target, string kickReason = "You have been kicked from the server.")
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kick") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // If the player is allowed to be kicked.
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    if (!IsPlayerAceAllowed(targetPlayer.Handle, "vMenu.DontKickMe"))
                    {
                        TriggerEvent("vMenu:KickSuccessful", source.Name, kickReason, targetPlayer.Name);

                        KickLog($"Player: {source.Name} has kicked: {targetPlayer.Name} for: {kickReason}.");
                        TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: $"The target player (<C>{targetPlayer.Name}</C>) has been kicked.");

                        // Kick the player from the server using the specified reason.
                        DropPlayer(targetPlayer.Handle, kickReason);
                        return;
                    }
                    // Trigger the client event on the source player to let them know that kicking this player is not allowed.
                    TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "Sorry, this player can ~r~not ~w~be kicked.");
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }

        /// <summary>
        /// Kill a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        [EventHandler("vMenu:KillPlayer")]
        private void KillPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kill") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    // Trigger the client event on the target player to make them kill themselves. R.I.P.
                    TriggerClientEvent(player: targetPlayer, eventName: "vMenu:KillMe", args: source.Name);
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }

        /// <summary>
        /// Teleport a specific player to another player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        [EventHandler("vMenu:SummonPlayer")]
        private void SummonPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Summon") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // Trigger the client event on the target player to make them teleport to the source player.
                Player targetPlayer = Players[target];
                if (targetPlayer != null)
                {
                    TriggerClientEvent(player: targetPlayer, eventName: "vMenu:GoToPlayer", args: source.Handle);
                    return;
                }
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
            }
            else
            {
                BanManager.BanCheater(source);
            }
        }

        [EventHandler("vMenu:SendMessageToPlayer")]
        private void SendPrivateMessage([FromSource] Player source, int targetServerId, string message)
        {
            Player targetPlayer = Players[targetServerId];
            if (targetPlayer != null)
            {
                targetPlayer.TriggerEvent("vMenu:PrivateMessage", source.Handle, message);

                foreach (Player p in Players)
                {
                    if (p != source && p != targetPlayer)
                    {
                        if (vMenuShared.PermissionsManager.IsAllowed(vMenuShared.PermissionsManager.Permission.OPSeePrivateMessages, p))
                        {
                            p.TriggerEvent("vMenu:Notify", $"[vMenu Staff Log] <C>{source.Name}</C>~s~ sent a PM to <C>{targetPlayer.Name}</C>~s~: {message}");
                        }
                    }
                }
            }
        }

        [EventHandler("vMenu:PmsDisabled")]
        private void NotifySenderThatDmsAreDisabled([FromSource] Player source, string senderServerId)
        {
            var p = Players[int.Parse(senderServerId)];
            if (p != null)
            {
                p.TriggerEvent("vMenu:Notify", $"Sorry, your private message to <C>{source.Name}</C>~s~ could not be delivered because they disabled private messages.");
            }
        }
        #endregion

        #region logging and update checks notifications
        /// <summary>
        /// If enabled using convars, will log all kick actions to the server console as well as an external file.
        /// </summary>
        /// <param name="kickLogMesage"></param>
        private static void KickLog(string kickLogMesage)
        {
            //if (GetConvar("vMenuLogKickActions", "true") == "true")
            if (GetSettingsBool(Setting.vmenu_log_kick_actions))
            {
                string file = LoadResourceFile(GetCurrentResourceName(), "vmenu.log") ?? "";
                DateTime date = DateTime.Now;
                string formattedDate = (date.Day < 10 ? "0" : "") + date.Day + "-" +
                    (date.Month < 10 ? "0" : "") + date.Month + "-" +
                    (date.Year < 10 ? "0" : "") + date.Year + " " +
                    (date.Hour < 10 ? "0" : "") + date.Hour + ":" +
                    (date.Minute < 10 ? "0" : "") + date.Minute + ":" +
                    (date.Second < 10 ? "0" : "") + date.Second;
                string outputFile = file + $"[\t{formattedDate}\t] [KICK ACTION] {kickLogMesage}\n";
                SaveResourceFile(GetCurrentResourceName(), "vmenu.log", outputFile, -1);
                Debug.WriteLine("^3[vMenu] [KICK]^7 " + kickLogMesage + "\n");
            }
        }

        #endregion

        #region Add teleport location
        [EventHandler("vMenu:SaveTeleportLocation")]
        private void AddTeleportLocation([FromSource] Player source, string locationJson)
        {
            TeleportLocation location = JsonConvert.DeserializeObject<TeleportLocation>(locationJson);
            if (GetTeleportLocationsData().Any(loc => loc.name == location.name))
            {
                Log("A teleport location with this name already exists, location was not saved.", LogLevel.error);
                return;
            }
            var locs = GetLocations();
            locs.teleports.Add(location);
            if (!SaveResourceFile(GetCurrentResourceName(), "config/locations.json", JsonConvert.SerializeObject(locs, Formatting.Indented), -1))
            {
                Log("Could not save locations.json file, reason unknown.", LogLevel.error);
            }
            TriggerClientEvent("vMenu:UpdateTeleportLocations", JsonConvert.SerializeObject(locs.teleports));
        }
        #endregion

        #region Infinity bits
        private void RequestServerStateFromPlayer([FromSource] Player player)
        {
            player.TriggerEvent("vMenu:SetServerState", new
            {
                IsInfinity = GetConvar("onesync_enableInfinity", "false") == "true"
            });
        }

        [EventHandler("vMenu:RequestPlayerList")]
        private void RequestPlayerListFromPlayer([FromSource] Player player)
        {
            player.TriggerEvent("vMenu:ReceivePlayerList", Players.Select(p => new
            {
                n = p.Name,
                s = int.Parse(p.Handle),
            }));
        }

        [EventHandler("vMenu:GetPlayerCoords")]
        private void GetPlayerCoords([FromSource] Player source, int playerId, NetworkCallbackDelegate callback)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Teleport") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                var coords = Players[playerId]?.Character?.Position ?? Vector3.Zero;

                _ = callback(coords);

                return;
            }

            _ = callback(Vector3.Zero);
        }
        #endregion

        #region Player join/quit
        private HashSet<string> joinedPlayers = new HashSet<string>();

        private Task PlayersFirstTick()
        {
            Tick -= PlayersFirstTick;

            foreach (var player in Players)
            {
                joinedPlayers.Add(player.Handle);
            }

            return Task.FromResult(0);
        }

        [EventHandler("playerJoining")]
        private void OnPlayerJoining([FromSource] Player sourcePlayer)
        {
            joinedPlayers.Add(sourcePlayer.Handle);

            foreach (var player in Players)
            {
                if (IsPlayerAceAllowed(player.Handle, "vMenu.MiscSettings.JoinQuitNotifs") ||
                    IsPlayerAceAllowed(player.Handle, "vMenu.MiscSettings.All"))
                {
                    player.TriggerEvent("vMenu:PlayerJoinQuit", sourcePlayer.Name, null);
                }
            }
        }

        [EventHandler("playerDropped")]
        private void OnPlayerDropped([FromSource] Player sourcePlayer, string reason)
        {
            if (!joinedPlayers.Contains(sourcePlayer.Handle))
            {
                return;
            }

            joinedPlayers.Remove(sourcePlayer.Handle);

            foreach (var player in Players)
            {
                if (IsPlayerAceAllowed(player.Handle, "vMenu.MiscSettings.JoinQuitNotifs") ||
                    IsPlayerAceAllowed(player.Handle, "vMenu.MiscSettings.All"))
                {
                    player.TriggerEvent("vMenu:PlayerJoinQuit", sourcePlayer.Name, reason);
                }
            }
        }
        #endregion
    }
}

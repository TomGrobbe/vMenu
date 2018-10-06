using GHMatti.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using System.Dynamic;
using static vMenuServer.DebugLog;
using static vMenuShared.ConfigManager;

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
            if (MainServer.DebugMode)
            {
                string prefix = "[vMenu] ";
                if (level == LogLevel.error)
                {
                    prefix = "^1[vMenu] [ERROR]^0 ";
                }
                else if (level == LogLevel.info)
                {
                    prefix = "^5[vMenu] [INFO]^0 ";
                }
                else if (level == LogLevel.success)
                {
                    prefix = "^2[vMenu] [SUCCESS]^0 ";
                }
                else if (level == LogLevel.warning)
                {
                    prefix = "^3[vMenu] [WARNING]^0 ";
                }
                Debug.WriteLine($"{prefix}[DEBUG LOG] {data.ToString()}");

            }
        }
    }

    public class MainServer : BaseScript
    {
        public static bool UpToDate = true;
        // Debug shows more information when doing certain things. Leave it off to improve performance!
        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "server_debug_mode", 0) == "true" ? true : false;

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        private int currentHours = 9;
        private int currentMinutes = 0;
        private int minuteClockSpeed = 2000;
        private long minuteTimer = GetGameTimer();
        private long timeSyncCooldown = GetGameTimer();
        private string currentWeather = "CLEAR";
        private bool dynamicWeather = true;
        private bool blackout = false;
        private bool resetBlackout = false;
        private bool freezeTime = false;
        private int dynamicWeatherMinutes = 10;
        //private int dynamicWeatherTimeLeft = 5 * 12 * 10; // 5 seconds * 12 (because the loop checks 12 times a minute) * 10 (10 minutes)
        private long gameTimer = GetGameTimer();
        private long weatherTimer = GetGameTimer();
        private List<string> CloudTypes = new List<string>()
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

        public List<string> aceNames = new List<string>()
        {
            // Global
            "Everything",
            "DontKickMe",
            "NoClip",
            "Staff",

            // Online Players
            "OPMenu",
            "OPAll",
            "OPTeleport",
            "OPWaypoint",
            "OPSpectate",
            "OPSummon",
            "OPKill",
            "OPKick",
            "OPPermBan",
            "OPTempBan",
            "OPUnban",

            // Player Options
            "POMenu",
            "POAll",
            "POGod",
            "POInvisible",
            "POFastRun",
            "POFastSwim",
            "POSuperjump",
            "PONoRagdoll",
            "PONeverWanted",
            "POSetWanted",
            "POIgnored",
            "POFunctions",
            "POFreeze",
            "POScenarios",
            "POUnlimitedStamina",

            // Vehicle Options
            "VOMenu",
            "VOAll",
            "VOGod",
            "VOSpecialGod",
            "VORepair",
            "VOWash",
            "VOEngine",
            "VOChangePlate",
            "VOMod",
            "VOColors",
            "VOLiveries",
            "VOComponents",
            "VODoors",
            "VOWindows",
            "VOFreeze",
            "VOTorqueMultiplier",
            "VOPowerMultiplier",
            "VOFlip",
            "VOAlarm",
            "VOCycleSeats",
            "VOEngineAlwaysOn",
            "VONoSiren",
            "VONoHelmet",
            "VOLights",
            "VODelete",
            "VOUnderglow",
            "VOFlashHighbeamsOnHonk",
            
            // Vehicle Spawner
            "VSMenu",
            "VSAll",
            "VSDisableReplacePrevious",
            "VSSpawnByName",
            "VSAddon",
            "VSCompacts",
            "VSSedans",
            "VSSUVs",
            "VSCoupes",
            "VSMuscle",
            "VSSportsClassic",
            "VSSports",
            "VSSuper",
            "VSMotorcycles",
            "VSOffRoad",
            "VSIndustrial",
            "VSUtility",
            "VSVans",
            "VSCycles",
            "VSBoats",
            "VSHelicopters",
            "VSPlanes",
            "VSService",
            "VSEmergency",
            "VSMilitary",
            "VSCommercial",
            "VSTrains",

            // Saved Vehicles
            "SVMenu",
            "SVAll",
            "SVSpawn",

            // Player Appearance
            "PAMenu",
            "PAAll",
            "PACustomize",
            "PASpawnSaved",
            "PASpawnNew",

            // Time Options
            "TOMenu",
            "TOAll",
            "TOFreezeTime",
            "TOSetTime",

            // Weather Options
            "WOMenu",
            "WOAll",
            "WODynamic",
            "WOBlackout",
            "WOSetWeather",
            "WORemoveClouds",
            "WORandomizeClouds",

            // Weapon Options
            "WPMenu",
            "WPAll",
            "WPGetAll",
            "WPRemoveAll",
            "WPUnlimitedAmmo",
            "WPNoReload",
            "WPSpawn",
            "WPSpawnByName",
            "WPSetAllAmmo",
            
            // Weapons Permissions
            "WPSniperRifle",
            "WPFireExtinguisher",
            "WPCompactGrenadeLauncher",
            "WPSnowball",
            "WPVintagePistol",
            "WPCombatPDW",
            "WPHeavySniperMk2",
            "WPHeavySniper",
            "WPSweeperShotgun",
            "WPMicroSMG",
            "WPWrench",
            "WPPistol",
            "WPPumpShotgun",
            "WPAPPistol",
            "WPBall",
            "WPMolotov",
            "WPSMG",
            "WPStickyBomb",
            "WPPetrolCan",
            "WPStunGun",
            "WPAssaultRifleMk2",
            "WPHeavyShotgun",
            "WPMinigun",
            "WPGolfClub",
            "WPFlareGun",
            "WPFlare",
            "WPGrenadeLauncherSmoke",
            "WPHammer",
            "WPCombatPistol",
            "WPGusenberg",
            "WPCompactRifle",
            "WPHomingLauncher",
            "WPNightstick",
            "WPRailgun",
            "WPSawnOffShotgun",
            "WPSMGMk2",
            "WPBullpupRifle",
            "WPFirework",
            "WPCombatMG",
            "WPCarbineRifle",
            "WPCrowbar",
            "WPFlashlight",
            "WPDagger",
            "WPGrenade",
            "WPPoolCue",
            "WPBat",
            "WPPistol50",
            "WPKnife",
            "WPMG",
            "WPBullpupShotgun",
            "WPBZGas",
            "WPUnarmed",
            "WPGrenadeLauncher",
            "WPNightVision",
            "WPMusket",
            "WPProximityMine",
            "WPAdvancedRifle",
            "WPRPG",
            "WPPipeBomb",
            "WPMiniSMG",
            "WPSNSPistol",
            "WPPistolMk2",
            "WPAssaultRifle",
            "WPSpecialCarbine",
            "WPRevolver",
            "WPMarksmanRifle",
            "WPBattleAxe",
            "WPHeavyPistol",
            "WPKnuckleDuster",
            "WPMachinePistol",
            "WPCombatMGMk2",
            "WPMarksmanPistol",
            "WPMachete",
            "WPSwitchBlade",
            "WPAssaultShotgun",
            "WPDoubleBarrelShotgun",
            "WPAssaultSMG",
            "WPHatchet",
            "WPBottle",
            "WPCarbineRifleMk2",
            "WPParachute",
            "WPSmokeGrenade",

            // Misc Settings
            "MSAll",
            "MSClearArea",
            "MSTeleportToWp",
            "MSShowCoordinates",
            "MSShowLocation",
            "MSJoinQuitNotifs",
            "MSDeathNotifs",
            "MSNightVision",
            "MSThermalVision",
            "MSLocationBlips",
            "MSPlayerBlips",
            "MSTeleportLocations",
            "MSConnectionMenu",

            // Voice Chat
            "VCMenu",
            "VCAll",
            "VCEnable",
            "VCShowSpeaker",
            "VCStaffChannel",
        };
        public List<string> addonVehicles = new List<string>();
        public List<string> addonPeds = new List<string>();
        public List<string> addonWeapons = new List<string>();
        private List<string> weatherTypes = new List<string>()
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

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainServer()
        {
            RegisterCommand("vmenuserver", new Action<int, List<object>, string>(async (int source, List<object> args, string rawCommand) =>
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
                                new PlayerList()[source].TriggerEvent("chatMessage", $"vMenu Debug mode is now set to: {DebugMode}.");
                            }
                            return;
                        }
                        else if (args[0].ToString().ToLower() == "unban" && (source < 1))
                        {
                            if (args.Count() > 1 && !string.IsNullOrEmpty(args[1].ToString()))
                            {
                                string name = args[1].ToString().Trim();
                                name = name.Replace("\"", "");
                                name = BanManager.GetSafePlayerName(name);
                                var bans = await BanManager.GetBanList();
                                var banRecord = bans.Find(b => { return b.playerName == name; });
                                if (banRecord.playerName != null)
                                {
                                    if (await BanManager.RemoveBan(banRecord))
                                    {
                                        Debug.WriteLine("Player has been successfully unbanned.");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Could not unban the player, are you sure this player is actually banned?");
                                    }

                                }
                                else
                                {
                                    Debug.WriteLine($"Could not find a banned player by the name of '{name}'.");
                                }
                                bans = null;

                            }
                            else
                            {
                                Debug.WriteLine("You did not specify a player to unban, you must enter the FULL playername. Usage: vmenuserver unban \"playername\"");
                            }
                            return;
                        }

                    }
                }
                Debug.WriteLine($"vMenu is currently running version: {Version}.");

            }), true);

            if (GetCurrentResourceName() != "vMenu")
            {
                Exception InvalidNameException = new Exception("\r\n\r\n^1[vMenu] INSTALLATION ERROR!\r\nThe name of the resource is not valid. " +
                    "Please change the folder name from '^3" + GetCurrentResourceName() + "^1' to '^2vMenu^1' (case sensitive) instead!\r\n\r\n\r\n^0");
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
                //InitializeConfig();
                // Add event handlers.
                EventHandlers.Add("vMenu:SummonPlayer", new Action<Player, int>(SummonPlayer));
                EventHandlers.Add("vMenu:KillPlayer", new Action<Player, int>(KillPlayer));
                EventHandlers.Add("vMenu:KickPlayer", new Action<Player, int, string>(KickPlayer));
                EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(SendPermissionsAsync));
                EventHandlers.Add("vMenu:UpdateServerWeather", new Action<string, bool, bool>(UpdateWeather));
                EventHandlers.Add("vMenu:UpdateServerWeatherCloudsType", new Action<bool>(UpdateWeatherCloudsType));
                EventHandlers.Add("vMenu:UpdateServerTime", new Action<int, int, bool>(UpdateTime));
                EventHandlers.Add("vMenu:DisconnectSelf", new Action<Player>(DisconnectSource));

                string addons = LoadResourceFile(GetCurrentResourceName(), "addons.json") ?? LoadResourceFile(GetCurrentResourceName(), "config/addons.json") ?? "{}";
                try
                {
                    Dictionary<string, List<string>> json = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(addons);
                    if (json.ContainsKey("vehicles"))
                    {
                        foreach (var modelName in json["vehicles"])
                        {
                            Log("Addon vehicle loaded: " + modelName);
                            addonVehicles.Add(modelName);
                        }
                    }

                    if (json.ContainsKey("peds"))
                    {
                        foreach (var modelName in json["peds"])
                        {
                            Log("Addon ped loaded:" + modelName);
                            addonPeds.Add(modelName);
                        }
                    }

                    if (json.ContainsKey("weapons"))
                    {
                        foreach (var modelName in json["weapons"])
                        {
                            Log("Addon weapon loaded:" + modelName);
                            addonWeapons.Add(modelName);
                        }
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"\n\n^1[vMenu] [ERROR] ^0Your addons.json file contains a problem! Error details: {ex.Message}\n\n");
                }


                dynamicWeather = GetSettingsBool(Setting.vmenu_enable_dynamic_weather);
                if (GetSettingsInt(Setting.vmenu_dynamic_weather_timer) != -1)
                {
                    dynamicWeatherMinutes = GetSettingsInt(Setting.vmenu_dynamic_weather_timer);
                    //dynamicWeatherTimeLeft = 5 * 12 * dynamicWeatherMinutes;
                }



                string defaultWeather = GetSettingsString(Setting.vmenu_default_weather);

                if (!string.IsNullOrEmpty(defaultWeather))
                {
                    if (weatherTypes.Contains(defaultWeather))
                    {
                        currentWeather = defaultWeather;
                    }
                }

                currentHours = GetSettingsInt(Setting.vmenu_default_time_hour);
                currentHours = (currentHours >= 0 && currentHours < 24) ? currentHours : 9;
                currentMinutes = GetSettingsInt(Setting.vmenu_default_time_min);
                currentMinutes = (currentMinutes >= 0 && currentMinutes < 60) ? currentMinutes : 0;


                minuteClockSpeed = GetSettingsInt(Setting.vmenu_ingame_minute_duration);
                minuteClockSpeed = (minuteClockSpeed > 0) ? minuteClockSpeed : 2000;

                Tick += WeatherLoop;
                Tick += TimeLoop;
            }
        }

        /// <summary>
        /// Disconnect the source player because they used the disconnect menu button.
        /// </summary>
        /// <param name="src"></param>
        private void DisconnectSource([FromSource] Player src)
        {
            src.Drop("You disconnected yourself.");
        }
        #endregion

        #region Manage weather and time changes.
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            if (GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                if (minuteClockSpeed > 2000)
                {
                    await Delay(2000);
                }
                else
                {
                    await Delay(minuteClockSpeed);
                }
                if (!freezeTime)
                {
                    // only add a minute if the timer has reached the configured duration (2000ms (2s) by default).
                    if (GetGameTimer() - minuteTimer > minuteClockSpeed)
                    {
                        currentMinutes++;
                        minuteTimer = GetGameTimer();
                    }

                    if (currentMinutes > 59)
                    {
                        currentMinutes = 0;
                        currentHours++;
                    }
                    if (currentHours > 23)
                    {
                        currentHours = 0;
                    }
                }

                if (GetGameTimer() - timeSyncCooldown > 6000)
                {
                    TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
                    timeSyncCooldown = GetGameTimer();
                }
            }

        }

        /// <summary>
        /// Task used for syncing and changing weather dynamically.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherLoop()
        {
            await Delay(5000);

            if (GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                if (dynamicWeather)
                {
                    if (resetBlackout && GetGameTimer() - weatherTimer > 60000) // if 1 minute has passed since last change, and resetblackout is true, disable blackout and reset it.
                    {
                        resetBlackout = false;
                        blackout = false;
                    }
                    if (GetGameTimer() - weatherTimer > (dynamicWeatherMinutes * 60000))
                    {
                        RefreshWeather();
                        if (DebugMode)
                        {
                            long gameTimer2 = GetGameTimer();
                            Log($"Changing weather, last weather duration: {(int)((GetGameTimer() - weatherTimer) / 60000)} minutes. New Weather Type: {currentWeather}");
                            gameTimer = gameTimer2;
                        }
                        weatherTimer = GetGameTimer();
                    }
                }
                else
                {
                    weatherTimer = GetGameTimer();
                }
                if (GetSettingsBool(Setting.vmenu_allow_random_blackout) && (currentWeather == "THUNDER" || currentWeather == "HALLOWHEEN") && new Random().Next(5) == 1 && !blackout && !resetBlackout)
                {
                    blackout = true;
                    resetBlackout = true;
                }
                if (blackout == false && resetBlackout)
                {
                    resetBlackout = false;
                }
                TriggerClientEvent("vMenu:SetWeather", currentWeather, blackout, dynamicWeather);
            }
        }

        /// <summary>
        /// Select a new random weather type, based on the current weather and some patterns.
        /// </summary>
        private void RefreshWeather()
        {
            var random = new Random().Next(20);
            if (currentWeather == "RAIN" || currentWeather == "THUNDER")
            {
                currentWeather = "CLEARING";
            }
            else if (currentWeather == "CLEARING")
            {
                currentWeather = "CLOUDS";
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
                        currentWeather = (currentWeather == "EXTRASUNNY" ? "CLEAR" : "EXTRASUNNY");
                        break;
                    case 6:
                    case 7:
                    case 8:
                        currentWeather = (currentWeather == "SMOG" ? "FOGGY" : "SMOG");
                        break;
                    case 9:
                    case 10:
                    case 11:
                        currentWeather = (currentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 12:
                    case 13:
                    case 14:
                        currentWeather = (currentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 15:
                        currentWeather = (currentWeather == "OVERCAST" ? "THUNDER" : "OVERCAST");
                        break;
                    case 16:
                        currentWeather = (currentWeather == "CLOUDS" ? "EXTRASUNNY" : "RAIN");
                        break;
                    case 17:
                    case 18:
                    case 19:
                    default:
                        currentWeather = (currentWeather == "FOGGY" ? "SMOG" : "FOGGY");
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
        private void UpdateWeather(string newWeather, bool blackoutNew, bool dynamicWeatherNew)
        {
            currentWeather = newWeather;
            blackout = blackoutNew;
            dynamicWeather = dynamicWeatherNew;
            TriggerClientEvent("vMenu:SetWeather", currentWeather, blackout, dynamicWeather);
        }

        /// <summary>
        /// Set a new random clouds type and opacity for all clients.
        /// </summary>
        /// <param name="removeClouds"></param>
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
        private void UpdateTime(int newHours, int newMinutes, bool freezeTimeNew)
        {
            currentHours = newHours;
            currentMinutes = newMinutes;
            freezeTime = freezeTimeNew;
            TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
        }
        #endregion

        #region Online Players Menu Actions
        /// <summary>
        /// Kick a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="kickReason"></param>
        private void KickPlayer([FromSource] Player source, int target, string kickReason = "You have been kicked from the server.")
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kick") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // If the player is allowed to be kicked.
                Player targetPlayer = new PlayerList()[target];
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
        private void KillPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Kill") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player targetPlayer = new PlayerList()[target];
                if (targetPlayer != null)
                {
                    // Trigger the client event on the target player to make them kill themselves. R.I.P.
                    TriggerClientEvent(player: targetPlayer, eventName: "vMenu:KillMe");
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
        private void SummonPlayer([FromSource] Player source, int target)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Summon") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                // Trigger the client event on the target player to make them teleport to the source player.
                Player targetPlayer = new PlayerList()[target];
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
        #endregion

        #region Send Permissions & Settings to the requesting client
        /// <summary>
        /// Send the permissions to the client that requested it.
        /// </summary>
        /// <param name="player"></param>
        private async void SendPermissionsAsync([FromSource] Player player)
        {

            // Get Permissions
            Dictionary<string, bool> perms = new Dictionary<string, bool>();
            foreach (string ace in aceNames)
            {
                var realAceName = GetRealAceName(ace);
                var allowed = IsPlayerAceAllowed(player.Handle, realAceName);
                perms.Add(ace, allowed);
            }


            player.TriggerEvent("vMenu:ConfigureClient", addonVehicles, addonPeds, addonWeapons, perms);


            while (!UpdateChecker.CheckedForUpdates)
            {
                await Delay(0);
            }
            if (!UpToDate)
            {
                TriggerClientEvent(player, "vMenu:OutdatedResource");
            }
        }

        private string GetRealAceName(string inputString)
        {
            string outputString = inputString;
            var prefix = inputString.Substring(0, 2);

            if (prefix == "OP")
            {
                outputString = "vMenu.OnlinePlayers." + inputString.Substring(2);
            }
            else if (prefix == "PO")
            {
                outputString = "vMenu.PlayerOptions." + inputString.Substring(2);
            }
            else if (prefix == "VO")
            {
                outputString = "vMenu.VehicleOptions." + inputString.Substring(2);
            }
            else if (prefix == "VS")
            {
                outputString = "vMenu.VehicleSpawner." + inputString.Substring(2);
            }
            else if (prefix == "SV")
            {
                outputString = "vMenu.SavedVehicles." + inputString.Substring(2);
            }
            else if (prefix == "PA")
            {
                outputString = "vMenu.PlayerAppearance." + inputString.Substring(2);
            }
            else if (prefix == "TO")
            {
                outputString = "vMenu.TimeOptions." + inputString.Substring(2);
            }
            else if (prefix == "WO")
            {
                outputString = "vMenu.WeatherOptions." + inputString.Substring(2);
            }
            else if (prefix == "WP")
            {
                outputString = "vMenu.WeaponOptions." + inputString.Substring(2);
            }
            else if (prefix == "MS")
            {
                outputString = "vMenu.MiscSettings." + inputString.Substring(2);
            }
            else if (prefix == "VC")
            {
                outputString = "vMenu.VoiceChat." + inputString.Substring(2);
            }
            else
            {
                outputString = "vMenu." + inputString;
            }

            return outputString;
        }
        #endregion


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
                Debug.WriteLine("^3[vMenu] [KICK]^0 " + kickLogMesage + "\n");
            }
        }
    }
}

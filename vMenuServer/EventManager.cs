using GHMatti.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuServer
{
    public class EventManager : BaseScript
    {
        // Debug shows more information when doing certain things. Leave it off to improve performance!
        private bool debug = GetResourceMetadata(GetCurrentResourceName(), "server_debug_mode", 0) == "true" ? true : false;

        private int currentHours = 9;
        private int currentMinutes = 0;
        private string currentWeather = "CLEAR";
        private bool dynamicWeather = true;
        private bool blackout = false;
        private bool freezeTime = false;
        private int dynamicWeatherTimeLeft = 5 * 12 * 10; // 5 seconds * 12 (because the loop checks 12 times a minute) * 10 (10 minutes)
        private long gameTimer = GetGameTimer();
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

            // Onlie Players
            "OPMenu",
            "OPAll",
            "OPTeleport",
            "OPWaypoint",
            "OPSpectate",
            "OPSummon",
            "OPKill",
            "OPKick",

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

            // Vehicle Options
            "VOMenu",
            "VOAll",
            "VOGod",
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
            
            // Vehicle Spawner
            "VSMenu",
            "VSAll",
            "VSSpawnByName",
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

            // Misc Settings
            "MSMenu",
            "MSAll",
            "MSTeleportToWp",
            "MSShowCoordinates",
            "MSShowLocation",
            "MSJoinQuitNotifs",
            "MSDeathNotifs",

            // Voice Chat
            "VCMenu",
            "VCAll",
            "VCEnable",
            "VCShowSpeaker",
            "VCStaffChannel",
        };

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
        {
            if (GetCurrentResourceName() != "vMenu")
            {
                Exception InvalidNameException = new Exception("\r\n\r\n[vMenu] INSTALLATION ERROR!\r\nThe name of the resource is not valid. Please change the folder name from '" + GetCurrentResourceName() + "' to 'vMenu' (case sensitive) instead!\r\n\r\n\r\n");
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
                EventHandlers.Add("vMenu:SummonPlayer", new Action<Player, int>(SummonPlayer));
                EventHandlers.Add("vMenu:KillPlayer", new Action<Player, int>(KillPlayer));
                EventHandlers.Add("vMenu:KickPlayer", new Action<Player, int, string>(KickPlayer));
                EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(SendPermissionsAsync));
                EventHandlers.Add("vMenu:UpdateServerWeather", new Action<string, bool, bool>(UpdateWeather));
                EventHandlers.Add("vMenu:UpdateServerWeatherCloudsType", new Action<bool>(UpdateWeatherCloudsType));
                EventHandlers.Add("vMenu:UpdateServerTime", new Action<int, int, bool>(UpdateTime));

                Tick += WeatherLoop;
                Tick += TimeLoop;
            }
        }
        #endregion

        #region Manage weather and time changes.
        /// <summary>
        /// Loop used for syncing and keeping track of the time in-game.
        /// </summary>
        /// <returns></returns>
        private async Task TimeLoop()
        {
            await Delay(4000);
            if (freezeTime)
            {
                TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
            }
            else
            {
                currentMinutes += 2;
                if (currentMinutes > 59)
                {
                    currentMinutes = 0;
                    currentHours++;
                }
                if (currentHours > 23)
                {
                    currentHours = 0;
                }
                TriggerClientEvent("vMenu:SetTime", currentHours, currentMinutes, freezeTime);
            }
        }

        /// <summary>
        /// Task used for syncing and changing weather dynamically.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherLoop()
        {
            await Delay(5000);
            if (dynamicWeather)
            {
                dynamicWeatherTimeLeft -= 10;
                if (dynamicWeatherTimeLeft < 10)
                {
                    dynamicWeatherTimeLeft = 5 * 12 * 10;
                    RefreshWeather();
                    if (debug)
                    {
                        long gameTimer2 = GetGameTimer();
                        Debug.WriteLine($"Duration: {((gameTimer2 - gameTimer) / 100).ToString()}. New Weather Type: {currentWeather}");
                        gameTimer = gameTimer2;
                    }
                }
            }
            else
            {
                dynamicWeatherTimeLeft = 5 * 12 * 10;
            }
            TriggerClientEvent("vMenu:SetWeather", currentWeather, blackout, dynamicWeather);
        }

        /// <summary>
        /// Select a new random weather type, based on the current weather and some patterns.
        /// </summary>
        private void RefreshWeather()
        {
            var random = new Random().Next(10);
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
                        currentWeather = (currentWeather == "EXTRASUNNY" ? "CLEAR" : "EXTRASUNNY");
                        break;
                    case 3:
                        currentWeather = (currentWeather == "SMOG" ? "FOGGY" : "SMOG");
                        break;
                    case 4:
                    case 5:
                    case 6:
                        currentWeather = (currentWeather == "CLOUDS" ? "OVERCAST" : "CLOUDS");
                        break;
                    case 7:
                        currentWeather = (currentWeather == "CLOUDS" ? "EXTRASUNNY" : "RAIN");
                        break;
                    case 8:
                        currentWeather = (currentWeather == "OVERCAST" ? "THUNDER" : "OVERCAST");
                        break;
                    case 9:
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
            // If the player is allowed to be kicked.
            var targetPlayer = new PlayerList()[target];
            if (!IsPlayerAceAllowed(targetPlayer.Handle, "vMenu.DontKickMe"))
            {
                // Kick the player from the server using the specified reason.
                DropPlayer(targetPlayer.Handle, kickReason);
            }
            else
            {
                // Trigger the client event on the source player to let them know that kicking this player is not allowed.
                TriggerClientEvent(player: source, eventName: "vMenu:KickCallback", args: "Sorry, this player can ~r~not ~w~be kicked.");
            }
        }

        /// <summary>
        /// Kill a specific player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void KillPlayer([FromSource] Player source, int target)
        {
            var targetPlayer = new PlayerList()[target];
            // Trigger the client event on the target player to make them kill themselves. R.I.P.
            TriggerClientEvent(player: targetPlayer, eventName: "vMenu:KillMe");
        }

        /// <summary>
        /// Teleport a specific player to another player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void SummonPlayer([FromSource] Player source, int target)
        {
            // Trigger the client event on the target player to make them teleport to the source player.
            var targetPlayer = new PlayerList()[target];
            TriggerClientEvent(player: targetPlayer, eventName: "vMenu:GoToPlayer", args: source.Handle);
        }
        #endregion

        #region Send Permissions & Settings to the requesting client
        /// <summary>
        /// Send the permissions to the client that requested it.
        /// </summary>
        /// <param name="player"></param>
        private async void SendPermissionsAsync([FromSource] Player player)
        {
            // Permissions
            Dictionary<string, bool> perms = new Dictionary<string, bool>();

            foreach (string ace in aceNames)
            {
                var realAceName = GetRealAceName(ace);
                var allowed = IsPlayerAceAllowed(player.Handle, realAceName);
                perms.Add(ace, allowed);
            }
            
            // Settings
            Dictionary<string, string> options = new Dictionary<string, string>
            {
                { "menuKey", GetConvarInt("vMenuToggleMenuKey", 244).ToString() ?? "244" },
                { "noclipKey", GetConvarInt("vMenuNoClipKey", 289).ToString() ?? "289" },
                { "disableSync", GetConvar("vMenuDisableTimeAndWeatherSync", "false") ?? "false"}
            };

            // Send the permissions to the client.
            TriggerClientEvent(player, "vMenu:SetPermissions", perms);

            // Wait 50 ms, then send the settings to the client.
            await Delay(50);
            TriggerClientEvent(player, "vMenu:SetOptions", options);

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
    }
}

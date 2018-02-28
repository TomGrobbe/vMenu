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

        #region List of all permissions
        // List of all permissions.
        private List<string> aceNames = new List<string>() {
            // Grants access to everything in the menu.
            "vMenu.everything",
            // Prevents this player from being kicked.
            "vMenu.dontkick",

            #region Menu Access
            // Menu Access.
            "vMenu.menus.*",
            "vMenu.menus.onlinePlayers",
            "vMenu.menus.playerOptions",
            "vMenu.menus.vehicleOptions",
            "vMenu.menus.vehicleSpawner",
            "vMenu.menus.playerSkin",
            "vMenu.menus.savedVehicles",
            "vMenu.menus.time",
            "vMenu.menus.weather",
            "vMenu.menus.weapons",
            "vMenu.menus.misc",
            "vMenu.menus.voicechat",
            #endregion
            #region Online Players
            // Online Players.
            "vMenu.onlinePlayers.*",
            "vMenu.onlinePlayers.teleport",
            "vMenu.onlinePlayers.waypoint",
            "vMenu.onlinePlayers.spectate",
            "vMenu.onlinePlayers.summon",
            "vMenu.onlinePlayers.kill",
            "vMenu.onlinePlayers.kick",
            #endregion
            #region Player Options
            // Player Options.
            "vMenu.playerOptions.*",
            "vMenu.playerOptions.god",
            "vMenu.playerOptions.invisible",
            "vMenu.playerOptions.stamina",
            "vMenu.playerOptions.fastrun",
            "vMenu.playerOptions.fastswim",
            "vMenu.playerOptions.superjump",
            "vMenu.playerOptions.noragdoll",
            "vMenu.playerOptions.neverwanted",
            "vMenu.playerOptions.setwanted",
            "vMenu.playerOptions.ignore",
            "vMenu.playerOptions.options",
            "vMenu.playerOptions.actions",
            "vMenu.playerOptions.scenarios",
            "vMenu.playerOptions.freeze",
            #endregion
            #region Vehicle Options
            // Vehicle Options.
            "vMenu.vehicleOptions.*",
            "vMenu.vehicleOptions.god",
            "vMenu.vehicleOptions.fix",
            "vMenu.vehicleOptions.clean",
            "vMenu.vehicleOptions.dirt",
            "vMenu.vehicleOptions.plate",
            "vMenu.vehicleOptions.mods",
            "vMenu.vehicleOptions.colors",
            "vMenu.vehicleOptions.liveries",
            "vMenu.vehicleOptions.extras",
            "vMenu.vehicleOptions.doors",
            "vMenu.vehicleOptions.windows",
            "vMenu.vehicleOptions.delete",
            "vMenu.vehicleOptions.freeze",
            "vMenu.vehicleOptions.torque",
            "vMenu.vehicleOptions.power",
            "vMenu.vehicleOptions.flip",
            "vMenu.vehicleOptions.alarm",
            "vMenu.vehicleOptions.cycleseats",
            "vMenu.vehicleOptions.engine",
            "vMenu.vehicleOptions.nosiren",
            "vMenu.vehicleOptions.nohelmet",
            #endregion
            #region Vehicle Spawner
            // Vehicle Spawner.
            "vMenu.vehicleSpawner.*",
            "vMenu.vehicleSpawner.spawninside",
            "vMenu.vehicleSpawner.replace",
            "vMenu.vehicleSpawner.spawnbyname",
            "vMenu.vehicleSpawner.category.*",
            "vMenu.vehicleSpawner.category.compacts",
            "vMenu.vehicleSpawner.category.sedans",
            "vMenu.vehicleSpawner.category.suv",
            "vMenu.vehicleSpawner.category.coupes",
            "vMenu.vehicleSpawner.category.muscle",
            "vMenu.vehicleSpawner.category.sportsclassic",
            "vMenu.vehicleSpawner.category.sports",
            "vMenu.vehicleSpawner.category.super",
            "vMenu.vehicleSpawner.category.motorcycles",
            "vMenu.vehicleSpawner.category.offroad",
            "vMenu.vehicleSpawner.category.industrial",
            "vMenu.vehicleSpawner.category.utility",
            "vMenu.vehicleSpawner.category.vans",
            "vMenu.vehicleSpawner.category.cycles",
            "vMenu.vehicleSpawner.category.boats",
            "vMenu.vehicleSpawner.category.helicopters",
            "vMenu.vehicleSpawner.category.planes",
            "vMenu.vehicleSpawner.category.service",
            "vMenu.vehicleSpawner.category.emergency",
            "vMenu.vehicleSpawner.category.military",
            "vMenu.vehicleSpawner.category.commercial",
            "vMenu.vehicleSpawner.category.trains",
            #endregion
            #region Player Skin
            // Player Skin (only one permission for now).
            "vMenu.playerSkin.*",
	        #endregion
            #region Saved Vehicles
            // Saved Vehicles (only one permission for now).
            "vMenu.savedVehicles.*",
	        #endregion
            #region Time
            // Time.
            "vMenu.time.*",
            "vMenu.time.freeze",
            "vMenu.time.preset",
            "vMenu.time.custom",
	        #endregion
            #region Weather
            // Weather.
            "vMenu.weather.*",
            "vMenu.weather.dynamic",
            "vMenu.weather.preset",
            "vMenu.weather.noclouds",
	        #endregion
            #region Weapons
            // Weapons.
            "vMenu.weapons.*",
            "vMenu.weapons.getall",
            "vMenu.weapons.removeall",
            "vMenu.weapons.modify",
            "vMenu.weapons.unlimited",
            "vMenu.weapons.noreload",
	        #endregion
            #region Misc
            // Misc. (Only one permission, these are simple features nobody should be restricted to, unless you're a stupid server owner)
            // Things like join/quit notifications, death notifications etc.
            "vMenu.misc.*",
	        #endregion
            #region Voicechat
            // Voicechat. 
            "vMenu.voicechat.*",
            // If the user is not allowed to toggle voice chat on/off, it will be enabled by default 
            // (users can disable it in pause menu > settings > voice chat).
            "vMenu.voicechat.enabled",
            "vMenu.voicechat.channels.*",
            "vMenu.voicechat.channels.default",
            "vMenu.voicechat.channels.one",
            "vMenu.voicechat.channels.two",
            "vMenu.voicechat.channels.three",
            // Staff chat is always global.
            "vMenu.voicechat.channels.staff",
            "vMenu.voicechat.proximity.*",
            "vMenu.voicechat.proximity.veryclose",
            "vMenu.voicechat.proximity.close",
            "vMenu.voicechat.proximity.nearby",
            "vMenu.voicechat.proximity.medium",
            "vMenu.voicechat.proximity.far",
            "vMenu.voicechat.proximity.veryfar",
            "vMenu.voicechat.proximity.global",
	        #endregion
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public EventManager()
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
            if (!IsPlayerAceAllowed(targetPlayer.Handle, "vMenu.dontkick"))
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

        #region SendPermissions to all clients
        /// <summary>
        /// Send the permissions to the client that requested it.
        /// </summary>
        /// <param name="player"></param>
        private async void SendPermissionsAsync([FromSource] Player player)
        {
            // Create a new dictionary to store the permissions + allowed/denied values.
            Dictionary<string, bool> permissions = new Dictionary<string, bool>();

            // Loop through the permissions list.
            foreach (string ace in aceNames)
            {
                // Convert the permissions name into a safe format to store everything in a dynamic object (client side).
                var safeName = ace.Replace(".", "_");
                // Get the allowed/not allowed value for each ace.
                var allowed = IsPlayerAceAllowed(player.Handle, aceNames[0]) ? true : IsPlayerAceAllowed(player.Handle, ace);

                // Add the permissions to the dictionary.
                permissions.Add(safeName, allowed);

                // Only if debugging is enabled, print the values to the server console.
                if (debug)
                {
                    Debug.WriteLine($"Permission:\t{ace}\r\nAllowed:\t{(allowed ? "yes" : "no")}");
                    await Delay(0);
                }
            }
            // Send the dictionary containing all permissions to the client.
            TriggerClientEvent(player, "vMenu:SetPermissions", permissions);
            await Delay(0);
        }
        #endregion
    }
}

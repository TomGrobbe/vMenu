using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using System.Dynamic;
using static CitizenFX.Core.Native.API;

namespace vMenuServer
{
    public class VMenuServer : BaseScript
    {
        protected static string _version = "0.1.3\n";
        private bool firstTick = true;
        private int hour = 7;
        private int minute = 0;
        private string weather = "CLEAR";

        /// <summary>
        /// Constructor
        /// </summary>
        public VMenuServer()
        {
            EventHandlers.Add("vMenu:UpdateWeather", new Action<string>(ChangeWeather));
            EventHandlers.Add("vMenu:UpdateTime", new Action<int, int>(ChangeTime));
            EventHandlers.Add("vMenu:RequestPermissions", new Action<Player>(SendPermissions));
            Tick += OnTick;
            Tick += WeatherAndTimeSync;
        }

        private void SendPermissions([FromSource] Player player)
        {   
            Dictionary<string, bool> permissions = new Dictionary<string, bool>
            {
                {"playerOptions", IsPlayerAceAllowed(player.Handle, "vMenu.playerOptions") },
                {"onlinePlayers", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers") },
                {"onlinePlayers_kick", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_kick") },
                {"onlinePlayers_teleport", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_teleport") },
                {"onlinePlayers_waypoint", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_waypoint") },
                {"onlinePlayers_spectate", IsPlayerAceAllowed(player.Handle, "vMenu.onlinePlayers_spectate") },
                {"vehicleOptions", IsPlayerAceAllowed(player.Handle, "vMenu.vehicleOptions") },
                {"spawnVehicle", IsPlayerAceAllowed(player.Handle, "vMenu.spawnVehicle") },
                {"weatherOptions", IsPlayerAceAllowed(player.Handle, "vMenu.weatherOptions") },
                {"timeOptions", IsPlayerAceAllowed(player.Handle, "vMenu.timeOptions") },
                {"noclip", IsPlayerAceAllowed(player.Handle, "vMenu.noclip") },
            };
            TriggerClientEvent(player, "vMenu:SetPermissions", permissions);
        }

        /// <summary>
        /// Update the time to the new values.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        private void ChangeTime(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }

        /// <summary>
        /// Update weather to the new type.
        /// </summary>
        /// <param name="newWeather"></param>
        private void ChangeWeather(string weather)
        {
            this.weather = weather;
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }
        
        
        /// <summary>
        /// OnTick (loops every tick).
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            if (firstTick)
            {
                firstTick = false;
                var result = await Exports[GetCurrentResourceName()].HttpRequest("https://vespura.com/vMenu-version.txt", "GET", "", "");
                if (result.ToString() == _version)
                {
                    Debug.WriteLine("+---------------------------+");
                    Debug.WriteLine("|          [vMenu]          |");
                    Debug.WriteLine("| Your vMenu is up to date. |");
                    Debug.WriteLine("|         Good Job!         |");
                    Debug.WriteLine("+---------------------------+\r");
                }
                else
                {
                    Debug.WriteLine("+---------------------------+");
                    Debug.WriteLine("|          [vMenu]          |");
                    Debug.WriteLine("| A new version of vMenu is |");
                    Debug.WriteLine("| available, please update! |");
                    Debug.WriteLine("+---------------------------+\r");
                }
            }
        }

        /// <summary>
        /// Loops every tick, but is delayed to run the code only once every 4 seconds.
        /// </summary>
        /// <returns></returns>
        private async Task WeatherAndTimeSync()
        {
            await Delay(4000);
            minute += 2;
            if (minute > 59)
            {
                minute = 0;
                hour++;
            }
            if (hour > 23)
            {
                hour = 0;
            }
            foreach (Player player in Players)
            {
                TriggerClientEvent("vMenu:WeatherAndTimeSync", weather, hour, minute);
            }
        }
    }
}

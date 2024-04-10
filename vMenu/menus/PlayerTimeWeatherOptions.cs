using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;
using vMenuShared;

namespace vMenuClient
{
    public class PlayerTimeWeatherOptions
    {

        // Variables
        private Menu menu;

        public MenuCheckboxItem clientSidedEnabled;
        public MenuListItem timeDataList;
        public static Dictionary<uint, MenuItem> weatherHashMenuIndex = new Dictionary<uint, MenuItem>();
        public List<string> weatherListData = new List<string>() { "Clear", "ExtraSunny", "Clouds", "Overcast", "Rain", "Clearing", "Thunder", "Smog", "Foggy", "Xmas", "Snowlight", "Blizzard", "Snow", "Halloween", "Neutral" };
        public MenuListItem weatherList;

        public bool ClientWeatherTimeBool;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu("Time & Weather", "Time & Weather Options");

            clientSidedEnabled = new MenuCheckboxItem("Client-Sided Time & Weather", "Enable or disable client-sided time and weather changes. \n\nPlease do note that this menu will be revamped in a future update, to replicate the World Related Options menu.", false);
            menu.AddMenuItem(clientSidedEnabled);

            List<string> timeData = new List<string>();
            for (var i = 0; i < 24; i++)
            {
                timeData.Add(i.ToString() + ":00");
            }
            timeDataList = new MenuListItem("Change Time", timeData, 12, "Select time of day.");
            menu.AddMenuItem(timeDataList);

            weatherList = new MenuListItem("Change Weather", weatherListData, 0, "Select weather.");
            menu.AddMenuItem(weatherList);



            menu.OnCheckboxChange += (_menu, _item, _index, _checked) => {
                if (_item == clientSidedEnabled)
                {
                    //Debug.WriteLine($"{_checked}");
                    ClientWeatherTimeBool = _checked;
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
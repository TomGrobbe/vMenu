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
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public static Dictionary<uint, MenuItem> weatherHashMenuIndex = new Dictionary<uint, MenuItem>();
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public static readonly List<string> weatherTypes = new List<string>()
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

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Weather Options");

            dynamicWeatherEnabled = new MenuCheckboxItem("Toggle Dynamic Weather", "Enable or disable dynamic weather changes.", EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem("Toggle Blackout", "This disables or enables all lights across the map.", EventManager.IsBlackoutEnabled);
            MenuItem extrasunny = new MenuItem("Extra Sunny", "Set the weather to ~y~extra sunny~s~!");
            MenuItem clear = new MenuItem("Clear", "Set the weather to ~y~clear~s~!");
            MenuItem neutral = new MenuItem("Neutral", "Set the weather to ~y~neutral~s~!");
            MenuItem smog = new MenuItem("Smog", "Set the weather to ~y~smog~s~!");
            MenuItem foggy = new MenuItem("Foggy", "Set the weather to ~y~foggy~s~!");
            MenuItem clouds = new MenuItem("Cloudy", "Set the weather to ~y~clouds~s~!");
            MenuItem overcast = new MenuItem("Overcast", "Set the weather to ~y~overcast~s~!");
            MenuItem clearing = new MenuItem("Clearing", "Set the weather to ~y~clearing~s~!");
            MenuItem rain = new MenuItem("Rainy", "Set the weather to ~y~rain~s~!");
            MenuItem thunder = new MenuItem("Thunder", "Set the weather to ~y~thunder~s~!");
            MenuItem blizzard = new MenuItem("Blizzard", "Set the weather to ~y~blizzard~s~!");
            MenuItem snow = new MenuItem("Snow", "Set the weather to ~y~snow~s~!");
            MenuItem snowlight = new MenuItem("Light Snow", "Set the weather to ~y~light snow~s~!");
            MenuItem xmas = new MenuItem("X-MAS Snow", "Set the weather to ~y~x-mas~s~!");
            MenuItem halloween = new MenuItem("Halloween", "Set the weather to ~y~halloween~s~!");
            MenuItem removeclouds = new MenuItem("Remove All Clouds", "Remove all clouds from the sky!");
            MenuItem randomizeclouds = new MenuItem("Randomize Clouds", "Add random clouds to the sky!");

            var indexOffset = 2;
            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
                indexOffset--;
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
                indexOffset--;
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                weatherHashMenuIndex.Add((uint)GetHashKey("EXTRASUNNY"), extrasunny);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLEAR"), clear);
                weatherHashMenuIndex.Add((uint)GetHashKey("NEUTRAL"), neutral);
                weatherHashMenuIndex.Add((uint)GetHashKey("SMOG"), smog);
                weatherHashMenuIndex.Add((uint)GetHashKey("FOGGY"), foggy);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLOUDS"), clouds);
                weatherHashMenuIndex.Add((uint)GetHashKey("OVERCAST"), overcast);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLEARING"), clearing);
                weatherHashMenuIndex.Add((uint)GetHashKey("RAIN"), rain);
                weatherHashMenuIndex.Add((uint)GetHashKey("THUNDER"), thunder);
                weatherHashMenuIndex.Add((uint)GetHashKey("BLIZZARD"), blizzard);
                weatherHashMenuIndex.Add((uint)GetHashKey("SNOW"), snow);
                weatherHashMenuIndex.Add((uint)GetHashKey("SNOWLIGHT"), snowlight);
                weatherHashMenuIndex.Add((uint)GetHashKey("XMAS"), xmas);
                weatherHashMenuIndex.Add((uint)GetHashKey("HALLOWEEN"), halloween);

                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                var index = index2 + indexOffset;
                // A weather type is selected.
                if (index >= 2 && index <= 16)
                {
                    Notify.Custom($"The weather will be changed to ~y~{item.Text}~s~. This will take {EventManager.WeatherChangeTime} seconds.");
                    UpdateServerWeather(weatherTypes[index - 2], EventManager.IsBlackoutEnabled, EventManager.DynamicWeatherEnabled);
                }
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom($"Dynamic weather changes are now {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.IsBlackoutEnabled, _checked);
                }
                else if (item == blackout)
                {
                    Notify.Custom($"Blackout mode is now {(_checked ? "~g~enabled" : "~r~disabled")}~s~.");
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.DynamicWeatherEnabled);
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

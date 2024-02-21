using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class TimeOptions
    {
        // Variables
        private Menu menu;
        public MenuItem freezeTimeToggle;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Time Options");

            // Create all menu items.
            freezeTimeToggle = new MenuItem("Freeze/Unfreeze Time", "Enable or disable time freezing.");
            var earlymorning = new MenuItem("Early Morning", "Set the time to 06:00.")
            {
                Label = "06:00"
            };
            var morning = new MenuItem("Morning", "Set the time to 09:00.")
            {
                Label = "09:00"
            };
            var noon = new MenuItem("Noon", "Set the time to 12:00.")
            {
                Label = "12:00"
            };
            var earlyafternoon = new MenuItem("Early Afternoon", "Set the time to 15:00.")
            {
                Label = "15:00"
            };
            var afternoon = new MenuItem("Afternoon", "Set the time to 18:00.")
            {
                Label = "18:00"
            };
            var evening = new MenuItem("Evening", "Set the time to 21:00.")
            {
                Label = "21:00"
            };
            var midnight = new MenuItem("Midnight", "Set the time to 00:00.")
            {
                Label = "00:00"
            };
            var night = new MenuItem("Night", "Set the time to 03:00.")
            {
                Label = "03:00"
            };

            var hours = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            var minutes = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (var i = 10; i < 60; i++)
            {
                if (i < 24)
                {
                    hours.Add(i.ToString());
                }
                minutes.Add(i.ToString());
            }
            var manualHour = new MenuListItem("Set Custom Hour", hours, 0);
            var manualMinute = new MenuListItem("Set Custom Minute", minutes, 0);

            // Add all menu items to the menu.
            if (IsAllowed(Permission.TOFreezeTime))
            {
                menu.AddMenuItem(freezeTimeToggle);
            }
            if (IsAllowed(Permission.TOSetTime))
            {
                menu.AddMenuItem(earlymorning);
                menu.AddMenuItem(morning);
                menu.AddMenuItem(noon);
                menu.AddMenuItem(earlyafternoon);
                menu.AddMenuItem(afternoon);
                menu.AddMenuItem(evening);
                menu.AddMenuItem(midnight);
                menu.AddMenuItem(night);
                menu.AddMenuItem(manualHour);
                menu.AddMenuItem(manualMinute);
            }

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If it's the freeze time button.
                if (item == freezeTimeToggle)
                {
                    Subtitle.Info($"Time will now {(EventManager.IsServerTimeFrozen ? "~y~continue" : "~o~freeze")}~s~.", prefix: "Info:");
                    UpdateServerTime(EventManager.GetServerHours, EventManager.GetServerMinutes, !EventManager.IsServerTimeFrozen);
                }
                else
                {
                    // Set the time using the index and some math :)
                    // eg: index = 3 (12:00) ---> 3 * 3 (=9) + 3 [= 12] ---> 12:00
                    // eg: index = 8 (03:00) ---> 8 * 3 (=24) + 3 (=27, >23 so 27-24) [=3] ---> 03:00
                    var newHour = 0;
                    if (IsAllowed(Permission.TOFreezeTime))
                    {
                        newHour = (index * 3) + 3 < 23 ? (index * 3) + 3 : (index * 3) + 3 - 24;
                    }
                    else
                    {
                        newHour = ((index + 1) * 3) + 3 < 23 ? ((index + 1) * 3) + 3 : ((index + 1) * 3) + 3 - 24;
                    }

                    var newMinute = 0;
                    Subtitle.Info($"Time set to ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "Info:");
                    UpdateServerTime(newHour, newMinute, EventManager.IsServerTimeFrozen);
                }

            };

            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                var newHour = EventManager.GetServerHours;
                var newMinute = EventManager.GetServerMinutes;
                if (item == manualHour)
                {
                    newHour = item.ListIndex;
                }
                else if (item == manualMinute)
                {
                    newMinute = item.ListIndex;
                }

                Subtitle.Info($"Time set to ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "Info:");
                UpdateServerTime(newHour, newMinute, EventManager.IsServerTimeFrozen);
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

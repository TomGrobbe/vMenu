using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class TimeOptions
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;
        

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Time Options")
            {
                MouseEdgeEnabled = false,
                MouseControlsEnabled = false,
                ControlDisablingEnabled = false
            };

            UIMenuItem freezeTimeToggle = new UIMenuItem("Toggle Freeze Time", "Enable or disable time freezing.");
            UIMenuItem earlymorning = new UIMenuItem("Early Morning", "Set the time to 06:00.");
            earlymorning.SetRightLabel("06:00");
            UIMenuItem morning = new UIMenuItem("Morning", "Set the time to 09:00.");
            morning.SetRightLabel("09:00");
            UIMenuItem noon = new UIMenuItem("Noon", "Set the time to 12:00.");
            noon.SetRightLabel("12:00");
            UIMenuItem earlyafternoon = new UIMenuItem("Early Afternoon", "Set the time to 15:00.");
            earlyafternoon.SetRightLabel("15:00");
            UIMenuItem afternoon = new UIMenuItem("Afternoon", "Set the time to 18:00.");
            afternoon.SetRightLabel("18:00");
            UIMenuItem evening = new UIMenuItem("Evening", "Set the time to 21:00.");
            evening.SetRightLabel("21:00");
            UIMenuItem midnight = new UIMenuItem("Midnight", "Set the time to 00:00.");
            midnight.SetRightLabel("00:00");
            UIMenuItem night = new UIMenuItem("Night", "Set the time to 03:00.");
            night.SetRightLabel("03:00");

            menu.AddItem(freezeTimeToggle);
            menu.AddItem(earlymorning);
            menu.AddItem(morning);
            menu.AddItem(noon);
            menu.AddItem(earlyafternoon);
            menu.AddItem(afternoon);
            menu.AddItem(evening);
            menu.AddItem(midnight);
            menu.AddItem(night);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == freezeTimeToggle)
                {
                    cf.UpdateServerTime(EventManager.currentHours, EventManager.currentMinutes, !EventManager.freezeTime);
                }
                else
                {
                    if ((index * 3) < 23)
                    cf.UpdateServerTime((index * 3) + , 0, EventManager.freezeTime);
                }

            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class WorldOptionsMenu
    {
        private static UIMenu worldRelatedOptions = null;

        public WorldOptionsMenu()
        {
            worldRelatedOptions = new Objects.vMenu("World Options").Create();
            
            UIMenuItem TimeOptionsButton = new UIMenuItem("Time Options", "Change the time, and edit other time related options.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            TimeOptionsButton.SetRightLabel(">>>");
            UIMenuItem WeatherOptionsButton = new UIMenuItem("Weather Options", "Change all weather related options here.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            WeatherOptionsButton.SetRightLabel(">>>");


            TimeOptionsButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldRelated.TimeOptions.Menu(), inheritOldMenuParams: true);
            };

            WeatherOptionsButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldRelated.WeatherOptions.Menu(), inheritOldMenuParams: true);
            };

            worldRelatedOptions.AddItem(TimeOptionsButton);
            worldRelatedOptions.AddItem(WeatherOptionsButton);

            Main.Menus.Add(worldRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return worldRelatedOptions;
        }
    }
}
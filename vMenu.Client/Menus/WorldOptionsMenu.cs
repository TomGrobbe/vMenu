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
            var MenuLanguage = Languages.Menus["WorldOptionsMenu"];

            worldRelatedOptions = new Objects.vMenu(MenuLanguage.Subtitle ?? "World Options").Create();
            
            UIMenuItem TimeOptionsButton = new UIMenuItem(MenuLanguage.Items["TimeOptionsItem"].Name ?? "Time Options", MenuLanguage.Items["TimeOptionsItem"].Description ?? "Change the time, and edit other time related options.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            TimeOptionsButton.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            TimeOptionsButton.SetRightLabel(">>>");
            UIMenuItem WeatherOptionsButton = new UIMenuItem(MenuLanguage.Items["WeatherOptionsItem"].Name ?? "Weather Options", MenuLanguage.Items["WeatherOptionsItem"].Description ?? "Change all weather related options here.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            WeatherOptionsButton.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            WeatherOptionsButton.SetRightLabel(">>>");


            TimeOptionsButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldSubmenus.TimeOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            WeatherOptionsButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldSubmenus.WeatherOptionsMenu.Menu(), inheritOldMenuParams: true);
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
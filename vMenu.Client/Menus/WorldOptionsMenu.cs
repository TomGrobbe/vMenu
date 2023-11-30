using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using static vMenu.Client.Functions.MenuFunctions;
using vMenu.Shared.Enums;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;
using vMenu.Client.Objects;

namespace vMenu.Client.Menus
{
    public class WorldOptionsMenu
    {
        private static UIMenu worldRelatedOptions = null;

        public WorldOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["WorldOptionsMenu"];

            worldRelatedOptions = new Objects.vMenu(MenuLanguage.Subtitle ?? "World Options").Create();
            
            UIMenuItem TimeOptionsButton = new vMenuItem(MenuLanguage.Items["TimeOptionsItem"], "Time Options", "Change the time, and edit other time related options.").Create();
            TimeOptionsButton.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            TimeOptionsButton.SetRightLabel(">>>");

            UIMenuItem WeatherOptionsButton = new vMenuItem(MenuLanguage.Items["WeatherOptionsItem"], "Weather Options", "Change all weather related options here.").Create();
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

            if (IsAllowed(Permission.WRTimeMenu))
            {
                worldRelatedOptions.AddItem(TimeOptionsButton);
            }

            if (IsAllowed(Permission.WRWeatherMenu))
            {
                worldRelatedOptions.AddItem(WeatherOptionsButton);
            }
            

            Main.Menus.Add(worldRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return worldRelatedOptions;
        }
    }
}
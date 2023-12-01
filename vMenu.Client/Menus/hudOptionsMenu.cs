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
using vMenu.Client.Objects;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class HudOptionsMenu
    {
        private static UIMenu hudOptionsMenu = null;

        public HudOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["HudOptionsMenu"];
            var SMVehicleMenuLanguage = Languages.Menus["VehicleHudMenu"];

            hudOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Hud Options").Create();

            UIMenuItem VehicleHudMenu = new vMenuItem(MenuLanguage.Items["VehicleHudMenu"], "Vehicle Hud Menu", "Vehicle Hud Options Menu").Create();
            VehicleHudMenu.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            VehicleHudMenu.SetRightLabel(">>>");

            VehicleHudMenu.Activated += async (sender, i) =>
            {
                await sender.SwitchTo(HudSubmenus.VehicleHudMenu.Menu(), inheritOldMenuParams: true);
            };

            hudOptionsMenu.AddItem(VehicleHudMenu);
            Main.Menus.Add(hudOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return hudOptionsMenu;
        }
    }
}

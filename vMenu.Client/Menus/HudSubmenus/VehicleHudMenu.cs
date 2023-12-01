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

namespace vMenu.Client.Menus.HudSubmenus
{
    public class VehicleHudMenu
    {
        private static UIMenu vehicleHudMenu = null;
        public static UIMenuCheckboxItem SpeedOMeter = null;
        public static UIMenuCheckboxItem VehicleHealth = null;
        public static UIMenuCheckboxItem Vehiclefuel = null;
        public static UIMenuCheckboxItem VehicleTurbo = null;

        public VehicleHudMenu()
        {
            var MenuLanguage = Languages.Menus["VehicleHudMenu"];

            vehicleHudMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Vehicle Hud Menu").Create();

            SpeedOMeter = new UIMenuCheckboxItem("Speedometer", UIMenuCheckboxStyle.Tick, false, "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            VehicleHealth = new UIMenuCheckboxItem("Health", UIMenuCheckboxStyle.Tick, false, "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            Vehiclefuel = new UIMenuCheckboxItem("Fuel", UIMenuCheckboxStyle.Tick, false, "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            VehicleTurbo = new UIMenuCheckboxItem("Turbo Pressure", UIMenuCheckboxStyle.Tick, false, "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vehicleHudMenu.AddItem(SpeedOMeter);
            vehicleHudMenu.AddItem(VehicleHealth);
            vehicleHudMenu.AddItem(Vehiclefuel);
            vehicleHudMenu.AddItem(VehicleTurbo);


            Main.Menus.Add(vehicleHudMenu);
        }

        public static UIMenu Menu()
        {
            return vehicleHudMenu;
        }
    }
}
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
using static vMenu.Client.Functions.MenuFunctions;

using vMenu.Shared.Enums;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class VehicleOptionsMenu
    {

        public static List<bool> allowedCategories = new List<bool>()
            {
                IsAllowed(Permission.VSCompacts),
                IsAllowed(Permission.VSSedans),
                IsAllowed(Permission.VSSUVs),
                IsAllowed(Permission.VSCoupes),
                IsAllowed(Permission.VSMuscle),
                IsAllowed(Permission.VSSportsClassic),
                IsAllowed(Permission.VSSports),
                IsAllowed(Permission.VSSuper),
                IsAllowed(Permission.VSMotorcycles),
                IsAllowed(Permission.VSOffRoad),
                IsAllowed(Permission.VSIndustrial),
                IsAllowed(Permission.VSUtility),
                IsAllowed(Permission.VSVans),
                IsAllowed(Permission.VSCycles),
                IsAllowed(Permission.VSBoats),
                IsAllowed(Permission.VSHelicopters),
                IsAllowed(Permission.VSPlanes),
                IsAllowed(Permission.VSService),
                IsAllowed(Permission.VSEmergency),
                IsAllowed(Permission.VSMilitary),
                IsAllowed(Permission.VSCommercial),
                IsAllowed(Permission.VSTrains),
                IsAllowed(Permission.VSOpenWheel)
            };

        private static UIMenu vehicleRelatedOptions = null;

        public VehicleOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["VehicleOptionsMenu"];

            vehicleRelatedOptions = new Objects.vMenu(MenuLanguage.Subtitle ?? "Vehicle Options").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };
            UIMenuItem VehicleSpawnerButton = new UIMenuItem("Vehicle Spawner", "Spawn a vehicle by name or choose one from a specific category.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            VehicleSpawnerButton.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            VehicleSpawnerButton.SetRightLabel(">>>");

            button.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            vehicleRelatedOptions.AddItem(VehicleSpawnerButton);
            vehicleRelatedOptions.AddItem(button);


            VehicleSpawnerButton.Activated += (sender, i) =>
            {
                sender.SwitchTo(VehicleSubmenus.VehicleSpawnerMenu.Menu(), inheritOldMenuParams: true);
            };

            Main.Menus.Add(vehicleRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return vehicleRelatedOptions;
        }
    }
}
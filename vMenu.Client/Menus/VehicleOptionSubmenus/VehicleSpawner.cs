using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus.VehicleSubmenus
{
    public class VehicleSpawner
    {
        private static UIMenu VehicleSpawnMenu = null;

        public VehicleSpawner()
        {
            var MenuLanguage = Languages.Menus["TimeOptionsMenu"];

            VehicleSpawnMenu = new Objects.vMenu("Vehicle Spawner").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };
            UIMenuItem SpawnByName = new UIMenuItem("Spawn By Name", "Spawn Vehicle By Name.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            SpawnByName.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            SpawnByName.SetRightLabel(">>>");
            UIMenuCheckboxItem replacevehicle = new UIMenuCheckboxItem("Replace Previous Vehicle", true);
            UIMenuCheckboxItem spawninside = new UIMenuCheckboxItem("Spawn Inside Vehicle", true);


            SpawnByName.Activated += async (sender, i) =>
            {
                await EntitySpawner.SpawnVehicle("custom", spawninside.Checked, replacevehicle.Checked);
            };


            button.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            VehicleSpawnMenu.AddItem(SpawnByName);
            VehicleSpawnMenu.AddItem(replacevehicle);
            VehicleSpawnMenu.AddItem(spawninside);
            VehicleSpawnMenu.AddItem(button);

            Main.Menus.Add(VehicleSpawnMenu);
        }

        public static UIMenu Menu()
        {
            return VehicleSpawnMenu;
        }
    }
}
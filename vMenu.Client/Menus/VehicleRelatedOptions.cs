using CitizenFX.Core;
using ScaleformUI.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;

namespace vMenu.Client.Menus
{
    public class VehicleRelatedOptions : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu vehicleRelatedOptions = null;

        public VehicleRelatedOptions()
        {
            vehicleRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "Vehicle Related Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true);
            
            UIMenuItem button = new UIMenuItem("~r~~h~Under Construction!~h~");

            vehicleRelatedOptions.AddItem(button);

            Main.Menus.Add(vehicleRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return vehicleRelatedOptions;
        }
    }
}
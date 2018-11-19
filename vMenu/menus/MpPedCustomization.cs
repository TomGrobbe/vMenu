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
    public class MpPedCustomization
    {
        // Variables
        private UIMenu menu;
        private readonly CommonFunctions cf = MainMenu.Cf;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("vMenu", "About vMenu", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
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
            //Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(cf.MpPedDataManager.GetHeadBlendData()));
            return menu;
        }



    }
}

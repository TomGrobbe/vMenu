using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScaleformUI.Menu;
using vMenu.Client.Functions;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

namespace vMenu.Client.Objects
{
    internal class vMenuItem
    {
        private static MenuItem MenuLanguageItems;
        private static string AltMenuName;
        private static string AltMenuDesc;

        public vMenuItem(MenuItem menuLanguageItems, string altMenuName, string altMenuDesc)
        {
            MenuLanguageItems = menuLanguageItems;
            AltMenuName = altMenuName;
            AltMenuDesc = altMenuDesc;
        }

        public UIMenuItem Create()
        {
            return new UIMenuItem(MenuLanguageItems.Name ?? AltMenuName, MenuLanguageItems.Description ?? AltMenuDesc, MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
        }
    }
}

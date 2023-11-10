using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Menu;

using vMenu.Client.Menus;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.KeyMappings
{
    public class Commands : BaseScript
    {
        public Commands()
        {
            RegisterKeyMapping("vMenu:OpenMenu", "Open vMenu Toggle", "keyboard", "M");
        }

        [Command("vMenu:OpenMenu")]
        private void MenuOpen()
        {
            if (ScaleformUI.MenuHandler.CurrentMenu != null)
            {
                ScaleformUI.MenuHandler.CurrentMenu.Visible = false;
            }
            else
            {
                UIMenu Menu = MainMenu.Menu();
                Menu.Visible = true;
            }
        }
    }
}

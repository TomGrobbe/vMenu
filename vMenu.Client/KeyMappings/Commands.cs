// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.FiveM.Native.Natives;

// vMenu Namespaces //
using vMenu.Client.Menus;

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
            if (ScaleformUI.MenuHandler.IsAnyMenuOpen)
            {
                ScaleformUI.MenuHandler.CurrentMenu.Visible = false;
            }
            else
            {
                MainMenu.Menu().Visible = true;
            }
        }
    }
}

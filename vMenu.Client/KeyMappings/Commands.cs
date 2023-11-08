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
using ScaleformUI.Menu;

namespace vMenu.Client.KeyMappings
{
    public class Commands : BaseScript
    {
        public static bool NoClipEnabled { get { return Functions.NoClip.IsNoclipActive(); } set { Functions.NoClip.SetNoclipActive(value); } }

        public Commands()
        {
            RegisterKeyMapping("vMenu:OpenMenu", "Open vMenu Toggle", "keyboard", "M");
            RegisterKeyMapping("vMenu:NoClip", "Open vMenu NoClip", "keyboard", "F2");
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
        [Command("vMenu:NoClip")]
        private void NoClip()
        {
            NoClipEnabled = !NoClipEnabled;
        }
    }
}

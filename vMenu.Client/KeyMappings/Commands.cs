// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

// vMenu Namespaces //
using vMenu.Client.Menus;
using ScaleformUI.Menu;
using vMenu.Client.Functions;

namespace vMenu.Client.KeyMappings
{
    public class Commands : BaseScript
    {
        public static bool NoClipEnabled { get { return Functions.NoClip.IsNoclipActive(); } set { Functions.NoClip.SetNoclipActive(value); } }

        public struct CommandStructure
        {
            public string Description;
            public string ControlType;
            public string Button;

            public CommandStructure(string Description, string ControlType, string Button)
            {
                this.Description = Description;
                this.ControlType = ControlType;
                this.Button = Button;
            }
        }
        public Commands()
        {
            string JsonData = LoadResourceFile(GetCurrentResourceName(), "KeyMapping.json") ?? "{}";
            var JsonCommand = JsonConvert.DeserializeObject<Dictionary<string, CommandStructure>>(JsonData);

            foreach (var CommandData in JsonCommand)
            {
                RegisterKeyMapping(CommandData.Key, CommandData.Value.Description, CommandData.Value.ControlType, CommandData.Value.Button);
            }
        }
        [Command("vMenu:NoClip")]
        private void NoClip()
        {
            NoClipEnabled = !NoClipEnabled;
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

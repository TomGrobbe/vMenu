using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using FxEvents.Shared.EventSubsystem;

using Newtonsoft.Json;

using ScaleformUI.Menu;

using vMenu.Client.Events;
using vMenu.Client.Functions;
using vMenu.Client.Menus;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.KeyMappings
{
    public class Commands
    {
        private static readonly object _padlock = new();
        private static Commands _instance;

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

        private Commands()
        {
            string JsonData = LoadResourceFile(GetCurrentResourceName(), "KeyMapping.jsonc") ?? "{}";
            var JsonCommand = JsonConvert.DeserializeObject<Dictionary<string, CommandStructure>>(JsonData);

            foreach (var CommandData in JsonCommand)
            {
                RegisterKeyMapping(CommandData.Key, CommandData.Value.Description, CommandData.Value.ControlType, CommandData.Value.Button);
            }

            RegisterCommand("vMenu:OpenMenu", MenuOpen, false);
            RegisterCommand("vMenu:NoClip", NoClip, false);

            Debug.WriteLine("Commands Initialized");
        }

        internal static Commands Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new Commands();
                }
            }
        }

        private InputArgument NoClip = new Action<int, List<object>, string>((source, args, rawCommand) =>
        {
            NoClipEnabled = !NoClipEnabled;
        });

        private InputArgument MenuOpen = new Action<int, List<object>, string>((source, args, rawCommand) =>
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
        });
    }
}

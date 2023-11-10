using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using FxEvents.Shared.EventSubsystem;

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


        public Commands()
        {
            RegisterKeyMapping("vMenu:OpenMenu", "Open vMenu Toggle", "keyboard", "M");
            RegisterCommand("vMenu:OpenMenu", MenuOpen, false);
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

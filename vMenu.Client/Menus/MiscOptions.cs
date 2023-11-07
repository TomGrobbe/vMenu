﻿using CitizenFX.Core;
using ScaleformUI.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using vMenu.Client.MenuSettings;

namespace vMenu.Client.Menus
{
    public class MiscOptionsMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu miscOptionsMenu = null;

        public MiscOptionsMenu()
        {
            miscOptionsMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Misc. Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, menuSettings.Glare, menuSettings.AlternativeTitle, menuSettings.fadingTime)
            {
                MaxItemsOnScreen = menuSettings.maxItemsOnScreen,
                BuildingAnimation = menuSettings.buildingAnimation,
                ScrollingType = menuSettings.scrollingType,
                Enabled3DAnimations = menuSettings.enabled3DAnimations,
                MouseControlsEnabled = menuSettings.mouseControlsEnabled,
                ControlDisablingEnabled = menuSettings.controlDisablingEnabled,
                EnableAnimation = menuSettings.enableAnimation,
            };

            UIMenuItem toggleMenuAlign = new UIMenuItem("Toggle Menu Align", "Change the Menu Alignment (Left | Right)");
            toggleMenuAlign.SetRightLabel(Main.MenuAlign.ToString());
            miscOptionsMenu.AddItem(toggleMenuAlign);

            toggleMenuAlign.Activated += (sender, i) =>
            {
                if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Right;
                    MenuFunctions.RestartMenu();
                }
                else
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Left;
                    MenuFunctions.RestartMenu();
                }
            };

            Main.Menus.Add(miscOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return miscOptionsMenu;
        }
    }
}

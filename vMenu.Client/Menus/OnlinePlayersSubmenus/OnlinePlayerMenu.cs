using CitizenFX.Core;
using CitizenFX.FiveM;
using CitizenFX.Shared;
using ScaleformUI.Menu;
using ScaleformUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using vMenu.Client.MenuSettings;

namespace vMenu.Client.Menus.OnlinePlayersSubmenus
{
    public class OnlinePlayerMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu onlinePlayerMenu = null;

        public OnlinePlayerMenu()
        {
            onlinePlayerMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Online Players", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, menuSettings.Glare, menuSettings.AlternativeTitle, menuSettings.fadingTime)
            {
                MaxItemsOnScreen = menuSettings.maxItemsOnScreen,
                BuildingAnimation = menuSettings.buildingAnimation,
                ScrollingType = menuSettings.scrollingType,
                Enabled3DAnimations = menuSettings.enabled3DAnimations,
                MouseControlsEnabled = menuSettings.mouseControlsEnabled,
                ControlDisablingEnabled = menuSettings.controlDisablingEnabled,
                EnableAnimation = menuSettings.enableAnimation,
            };
            Main.Menus.Add(onlinePlayerMenu);
        }

        public static UIMenu Menu(CitizenFX.FiveM.Player player, string texture)
        {
            onlinePlayerMenu.Windows.Clear();
            onlinePlayerMenu.MenuItems.Clear();

            UIMenuItem spectatePlayer = new UIMenuItem("Spectate Player", "Click to spectate this player", menuSettings.BackgroundColor, menuSettings.HighlightColor);

            UIMenuDetailsWindow playerDetails = new UIMenuDetailsWindow($"~h~{player.Name}~h~", $"Server ID: {player.ServerId}\nLocal ID: {player.Handle}", $"Steam ID: test\nDiscord ID: yeet", new UIDetailImage()
            {
                Txd = texture,
                Txn = texture,
                Size = new Size(60, 60)
            });

            onlinePlayerMenu.AddWindow(playerDetails);
            onlinePlayerMenu.AddItem(spectatePlayer);

            return onlinePlayerMenu;
        }
    }
}

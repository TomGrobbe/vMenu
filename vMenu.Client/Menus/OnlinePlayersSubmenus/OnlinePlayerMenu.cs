using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using Newtonsoft.Json;

using FxEvents;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Objects;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

namespace vMenu.Client.Menus.OnlinePlayersSubmenus
{
    public class OnlinePlayerMenu
    {
        private static UIMenu onlinePlayerMenu = null;

        public static Menu MenuLanguage = Languages.Menus["OnlinePlayerMenu"];
        private static Dictionary<vMenu.Shared.Enums.Permission, bool> Permissions = new();
        public OnlinePlayerMenu()
        {
            onlinePlayerMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Online Player").Create();

            Main.Menus.Add(onlinePlayerMenu);
        }

        public static UIMenu Menu(OnlinePlayersCB player, string texture)
        {
            onlinePlayerMenu.Windows.Clear();
            onlinePlayerMenu.MenuItems.Clear();

            UIMenuDetailsWindow playerDetails = new UIMenuDetailsWindow($"~h~{player.Player.Name}~h~", $"{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["ServerIdVar"] ?? "Server Id"}: {player.Player.ServerId}\n{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["PedHandleVar"] ?? "Ped Handle"}: {player.Player.CharacterHandle}", $"{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["SteamIdVar"] ?? "Steam Id"}: {player.SteamIdentifier ?? "None"}\n{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["DiscordIdVar"] ?? "Discord Id"}: {player.DiscordIdentifier ?? "None"}", new UIDetailImage()
            {
                Txd = texture,
                Txn = texture,
                Size = new Size(60, 60)
            });

            UIMenuItem spectatePlayer = new vMenuItem(MenuLanguage.Items["SpectatePlayerItem"], "Spectate Player", "Click to spectate this player").Create();
            spectatePlayer.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            UIMenuItem PlayerPermissions = new UIMenuItem("Player Permissions","Click to check/change permissions for this player for this session (will reset if server or script restarts)", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            PlayerPermissions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            PlayerPermissions.Activated += async (sender, i) =>
            {
                PlayerPermissions.Label = "Loading Player Permissions";
                var permsmenu = new Objects.vMenu("Player Permissions").Create();
                string permissions = await EventDispatcher.Get<string>("RequestPermissions", player.Player.ServerId);

                UIMenuItem UpdatePermissions = new UIMenuItem("Update Permissions","Click this to send the permission updates to the server and user", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
                permsmenu.AddItem(UpdatePermissions);

                var Permissions = JsonConvert.DeserializeObject<Dictionary<vMenu.Shared.Enums.Permission, bool>>(permissions);

                foreach ( var perm in Permissions)
                {
                    UIMenuCheckboxItem permbox = new UIMenuCheckboxItem($"{GetAceName(perm.Key)}", UIMenuCheckboxStyle.Tick, perm.Value, "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor)
                    {
                        ItemData = perm.Key
                    };
                    permsmenu.AddItem(permbox);
                }

                UpdatePermissions.Activated += async (sender, i) =>
                {
                    if (UpdatePermissions.Label == "~g~~h~Confirm Update Permissions~h~")
                    {
                        Notify.Success("Updating Permissions.");
                        BaseScript.TriggerServerEvent("vMenu:UpdatePerms",  player.Player.ServerId, JsonConvert.SerializeObject(Permissions));
                        PlayerPermissions.Label = "Update Permissions";
                        return;
                    }
                    UpdatePermissions.Enabled = false;
                    UpdatePermissions.Label = "~r~~h~Confirm Update Permissions~h~";
                    UpdatePermissions.SetRightLabel("~r~3");
                    await BaseScript.Delay(1000);
                    UpdatePermissions.SetRightLabel("~r~2");
                    await BaseScript.Delay(1000);
                    UpdatePermissions.SetRightLabel("~r~1");
                    await BaseScript.Delay(1000);
                    UpdatePermissions.SetRightLabel("~g~Unlocked");
                    UpdatePermissions.Label = "~g~~h~Confirm Update Permissions~h~";
                    UpdatePermissions.Enabled = true;
                };

                permsmenu.OnCheckboxChange += (_sender, _item, _checked) => 
                {
                    Permissions[_item.ItemData] = _checked;

                };
                
                await sender.SwitchTo(permsmenu, inheritOldMenuParams: true);
                UpdatePermissions.Label = "Player Permissions";
            };



            onlinePlayerMenu.AddWindow(playerDetails);
            onlinePlayerMenu.AddItem(spectatePlayer);
            onlinePlayerMenu.AddItem(PlayerPermissions);

            return onlinePlayerMenu;
        }
        public static string GetAceName(vMenu.Shared.Enums.Permission permission)
        {
            var name = permission.ToString();

            var prefix = "";

            switch (name.Substring(0, 2))
            {
                case "WR":
                    prefix += "WorldRelatedOptions";
                    break;
                case "VO":
                    prefix += "VehicleOptions";
                    break;
                case "PO":
                    prefix += "PlayerOptions";
                    break;
                case "VC":
                    prefix += "VoiceChat";
                    break;
                case "VS":
                    prefix += "VehicleSpawner";
                    break;
                default:
                    return prefix + name;
            }

            return prefix + "." + name.Substring(2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using Newtonsoft.Json;

namespace vMenuClient
{
    public class BannedPlayers
    {
        // Variables
        private UIMenu menu;
        private CommonFunctions cf = MainMenu.Cf;

        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
        }

        public List<BanRecord> banlist = new List<BanRecord>();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new UIMenu(GetPlayerName(PlayerId()), "Banned Players Management", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UpdateBans();
        }

        /// <summary>
        /// Updates the list of ban records.
        /// </summary>
        /// <param name="banJsonString"></param>
        public void UpdateBanList(string banJsonString)
        {
            banlist.Clear();
            dynamic obj = JsonConvert.DeserializeObject(banJsonString);
            foreach (dynamic br in obj)
            {
                BanRecord b = JsonToBanRecord(br);
                banlist.Add(b);
            }

            UpdateBans();

        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }

            return menu;
        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.Clear();

            foreach (BanRecord ban in banlist)
            {
                UIMenu bannedPlayer = new UIMenu("Banned Player", "Ban Record: " + ban.playerName, true)
                {
                    ScaleWithSafezone = false,
                    MouseControlsEnabled = false,
                    MouseEdgeEnabled = false,
                    ControlDisablingEnabled = false
                };
                bannedPlayer.SetMenuWidthOffset(50);

                // info items.
                UIMenuItem name = new UIMenuItem("Name:", ban.playerName)
                {
                    Enabled = false
                };
                name.SetRightLabel(ban.playerName);

                UIMenuItem bannedBy = new UIMenuItem("Banned By:", ban.bannedBy)
                {
                    Enabled = false
                };
                bannedBy.SetRightLabel(ban.bannedBy);

                UIMenuItem bannedUntil = new UIMenuItem("Banned Until:", ban.bannedUntil.ToString())
                {
                    Enabled = false
                };
                bannedUntil.SetRightLabel(ban.bannedUntil.ToString());

                var identifierstring = "";
                foreach (string id in ban.identifiers)
                {
                    if (id.Contains("steam:"))
                    {
                        var id2 = id;
                        if (id.Length > 40)
                        {
                            id2 = id.Substring(0, 40) + " " + id.Substring(40, id.Length - 40);
                        }
                        identifierstring += "~o~" + id2 + " ";
                    }
                    else if (id.Contains("license:"))
                    {
                        var id2 = id;
                        if (id.Length > 40)
                        {
                            id2 = id.Substring(0, 40) + " " + id.Substring(40, id.Length - 40);
                        }
                        identifierstring += "~y~" + id2 + " ";
                    }
                    else if (id.Contains("ip:"))
                    {
                        var id2 = id;
                        if (id.Length > 40)
                        {
                            id2 = id.Substring(0, 40) + " " + id.Substring(40, id.Length - 40);
                        }
                        identifierstring += "~g~" + id2 + " ";
                    }

                }
                UIMenuItem identifiers = new UIMenuItem("Player Identifiers", identifierstring)
                {
                    Enabled = false
                };

                UIMenuItem banReason = new UIMenuItem("Ban Reason", ban.banReason)
                {
                    Enabled = false
                };

                UIMenuItem unbanPlayer = new UIMenuItem("~r~Unban Player", "Unbanning the player cannot be undone. " +
                    "You will have to wait for the player to be online to ban them again.");

                // add items to menu.
                bannedPlayer.AddItem(name);
                bannedPlayer.AddItem(bannedBy);
                bannedPlayer.AddItem(bannedUntil);
                bannedPlayer.AddItem(identifiers);
                bannedPlayer.AddItem(banReason);
                bannedPlayer.AddItem(unbanPlayer);

                // refresh index and update scaleform.
                bannedPlayer.RefreshIndex();
                bannedPlayer.UpdateScaleform();

                // create button for this player and add it to the main banned players menu.
                UIMenuItem player = new UIMenuItem(ban.playerName, "View details of this player's ban record.");
                menu.AddItem(player);
                menu.BindMenuToItem(bannedPlayer, player);

                bannedPlayer.OnItemSelect += (sender, item, index) =>
                {
                    if (item == unbanPlayer)
                    {
                        if (item.Description == "Are you sure?")
                        {
                            Notify.Custom($"Player ~o~<C>{banlist[menu.MenuItems.IndexOf(player)].playerName}</C>~s~ has been ~g~unbanned~s~.");
                            UnbanPlayer(menu.MenuItems.IndexOf(player));
                            bannedPlayer.UpdateScaleform();
                            bannedPlayer.GoBack();
                            UpdateBans();
                        }
                        else
                        {
                            item.Description = "Are you sure?";
                            bannedPlayer.UpdateScaleform();
                            bannedPlayer.RefreshIndex();
                            bannedPlayer.CurrentSelection = bannedPlayer.MenuItems.Count - 1;
                        }
                    }
                };

                bannedPlayer.OnMenuClose += (a) =>
                {
                    if (bannedPlayer.MenuItems.Last().Description == "Are you sure?")
                    {
                        bannedPlayer.MenuItems.Last().Description = "Unbanning the player cannot be undone. " +
                    "You will have to wait for the player to be online to ban them again.";
                        bannedPlayer.UpdateScaleform();
                        bannedPlayer.RefreshIndex();
                    }
                };

                // add menu to menu pool.
                MainMenu.Mp.Add(bannedPlayer);
            }

            // update index & scaleform.
            menu.RefreshIndex();
            menu.UpdateScaleform();
        }

        /// <summary>
        /// Sends an event to the server requesting the player to be unbanned.
        /// We'll just assume that worked fine, so remove the item from our local list, we'll re-sync once the menu is re-opened.
        /// </summary>
        /// <param name="index"></param>
        private void UnbanPlayer(int index)
        {
            BanRecord record = banlist[index];
            banlist.Remove(record);
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerUnban", JsonConvert.SerializeObject(record));
        }


        /// <summary>
        /// Converts the ban record (json object) into a BanRecord struct.
        /// </summary>
        /// <param name="banRecordJsonObject"></param>
        /// <returns></returns>
        public static BanRecord JsonToBanRecord(dynamic banRecordJsonObject)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in banRecordJsonObject)
            {
                string key = brValue.Name.ToString();
                var value = brValue.Value;
                if (key == "playerName")
                {
                    newBr.playerName = value.ToString();
                }
                else if (key == "identifiers")
                {
                    var tmpList = new List<string>();
                    foreach (string identifier in value)
                    {
                        tmpList.Add(identifier);
                    }
                    newBr.identifiers = tmpList;
                }
                else if (key == "bannedUntil")
                {
                    newBr.bannedUntil = DateTime.Parse(value.ToString());
                }
                else if (key == "banReason")
                {
                    newBr.banReason = value.ToString();
                }
                else if (key == "bannedBy")
                {
                    newBr.bannedBy = value.ToString();
                }
            }
            return newBr;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class BannedPlayers
    {
        // Variables
        private Menu menu;

        private static LanguageManager LM = new LanguageManager();

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

        BanRecord currentRecord = new BanRecord();

        public List<BanRecord> banlist = new List<BanRecord>();

        Menu bannedPlayer = new Menu(LM.Get("Banned Player"), LM.Get("Ban Record: "));

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, LM.Get("Banned Players Management"));

            menu.InstructionalButtons.Add(Control.Jump, LM.Get("Filter Options"));
            menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(async (a, b) =>
            {
                if (banlist.Count > 1)
                {
                    string filterText = await GetUserInput(LM.Get("Filter List By Username (leave this empty to reset the filter!)"));
                    if (string.IsNullOrEmpty(filterText))
                    {
                        Subtitle.Custom(LM.Get("Filters have been cleared."));
                        menu.ResetFilter();
                        UpdateBans();
                    }
                    else
                    {
                        menu.FilterMenuItems(item => item.ItemData is BanRecord br && br.playerName.ToLower().Contains(filterText.ToLower()));
                        Subtitle.Custom(LM.Get("Username filter has been applied."));
                    }
                }
                else
                {
                    Notify.Error(LM.Get("At least 2 players need to be banned in order to use the filter function."));
                }

                Log($"Button pressed: {a} {b}");
            }), true));

            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("Player Name")));
            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("Banned By")));
            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("Banned Until")));
            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("Player Identifiers")));
            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("Banned For")));
            bannedPlayer.AddMenuItem(new MenuItem(LM.Get("~r~Unban"), LM.Get("~r~Warning, unbanning the player can NOT be undone. You will NOT be able to ban them again until they re-join the server. Are you absolutely sure you want to unban this player? ~s~Tip: Tempbanned players will automatically get unbanned if they log on to the server after their ban date has expired.")));

            // should be enough for now to cover all possible identifiers.
            List<string> colors = new List<string>() { "~r~", "~g~", "~b~", "~o~", "~y~", "~p~", "~s~", "~t~", };

            bannedPlayer.OnMenuClose += (sender) =>
            {
                BaseScript.TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                bannedPlayer.GetMenuItems()[5].Label = "";
                UpdateBans();
            };

            bannedPlayer.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                bannedPlayer.GetMenuItems()[5].Label = "";
            };

            bannedPlayer.OnItemSelect += (sender, item, index) =>
            {
                if (index == 5 && IsAllowed(Permission.OPUnban))
                {
                    if (item.Label == LM.Get("Are you sure?"))
                    {
                        if (banlist.Contains(currentRecord))
                        {
                            UnbanPlayer(banlist.IndexOf(currentRecord));
                            bannedPlayer.GetMenuItems()[5].Label = "";
                            bannedPlayer.GoBack();
                        }
                        else
                        {
                            Notify.Error(LM.Get("Somehow you managed to click the unban button but this ban record you're apparently viewing does not even exist. Weird..."));
                        }
                    }
                    else
                    {
                        item.Label = LM.Get("Are you sure?");
                    }
                }
                else
                {
                    bannedPlayer.GetMenuItems()[5].Label = "";
                }

            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                //if (index < banlist.Count)
                //{
                currentRecord = item.ItemData;

                bannedPlayer.MenuSubtitle = LM.Get("Ban Record: ~y~") + currentRecord.playerName;
                var nameItem = bannedPlayer.GetMenuItems()[0];
                var bannedByItem = bannedPlayer.GetMenuItems()[1];
                var bannedUntilItem = bannedPlayer.GetMenuItems()[2];
                var playerIdentifiersItem = bannedPlayer.GetMenuItems()[3];
                var banReasonItem = bannedPlayer.GetMenuItems()[4];
                nameItem.Label = currentRecord.playerName;
                nameItem.Description = LM.Get("Player name: ~y~") + currentRecord.playerName;
                bannedByItem.Label = currentRecord.bannedBy;
                bannedByItem.Description = LM.Get("Player banned by: ~y~") + currentRecord.bannedBy;
                if (currentRecord.bannedUntil.Date.Year == 3000)
                    bannedUntilItem.Label = LM.Get("Forever");
                else
                    bannedUntilItem.Label = currentRecord.bannedUntil.Date.ToString();
                bannedUntilItem.Description = LM.Get("This player is banned until: ") + currentRecord.bannedUntil.Date.ToString();
                playerIdentifiersItem.Description = "";

                int i = 0;
                foreach (string id in currentRecord.identifiers)
                {
                    // only (admins) people that can unban players are allowed to view IP's.
                    // this is just a slight 'safety' feature in case someone who doesn't know what they're doing
                    // gave builtin.everyone access to view the banlist.
                    if (id.StartsWith("ip:") && !IsAllowed(Permission.OPUnban))
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}ip: (hidden) ";
                    }
                    else
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{id.Replace(":", ": ")} ";
                    }
                    i++;
                }
                banReasonItem.Description = LM.Get("Banned for: ") + currentRecord.banReason;

                var unbanPlayerBtn = bannedPlayer.GetMenuItems()[5];
                unbanPlayerBtn.Label = "";
                if (!IsAllowed(Permission.OPUnban))
                {
                    unbanPlayerBtn.Enabled = false;
                    unbanPlayerBtn.Description = LM.Get("You are not allowed to unban players. You are only allowed to view their ban record.");
                    unbanPlayerBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                bannedPlayer.RefreshIndex();
                //}
            };
            MenuController.AddMenu(bannedPlayer);

        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.ResetFilter();
            menu.ClearMenuItems();

            foreach (BanRecord ban in banlist)
            {
                MenuItem recordBtn = new MenuItem(ban.playerName, $"~y~{ban.playerName}~s~ was banned by ~y~{ban.bannedBy}~s~ until ~y~{ban.bannedUntil}~s~ for ~y~{ban.banReason}~s~.")
                {
                    Label = "→→→",
                    ItemData = ban
                };
                menu.AddMenuItem(recordBtn);
                MenuController.BindMenuItem(menu, bannedPlayer, recordBtn);
            }
            menu.RefreshIndex();
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
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }

            return menu;
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

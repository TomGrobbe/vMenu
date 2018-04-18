using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Text.RegularExpressions;

namespace vMenuServer
{
    public class BanManager : BaseScript
    {
        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
        }

        /// <summary>
        /// List of ban records.
        /// </summary>
        private static List<BanRecord> BannedPlayersList = new List<BanRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public BanManager()
        {
            EventHandlers.Add("vMenu:TempBanPlayer", new Action<Player, int, double, string>(BanPlayer));
            EventHandlers.Add("vMenu:PermBanPlayer", new Action<Player, int, string>(BanPlayer));
            EventHandlers.Add("playerConnecting", new Action<Player, string, CallbackDelegate>(CheckForBans));
            EventHandlers.Add("vMenu:RequestPlayerUnban", new Action<Player, string>(RemoveBanRecord));
            EventHandlers.Add("vMenu:RequestBanList", new Action<Player>(SendBanList));
            BannedPlayersList = GetBanList();
        }

        private void SendBanList([FromSource] Player source)
        {
            if (MainServer.debug)
                Debug.Write("Updating player with new banlist.\n");
            source.TriggerEvent("vMenu:SetBanList", JsonConvert.SerializeObject(GetBanList()).ToString());
        }

        /// <summary>
        /// Gets the ban list from the bans.json file.
        /// </summary>
        /// <returns></returns>
        private static List<BanRecord> GetBanList()
        {
            var banList = new List<BanRecord>();
            string bansJson = LoadResourceFile(GetCurrentResourceName(), "bans.json");
            if (bansJson != null && bansJson != "")
            {
                dynamic banRecords = JsonConvert.DeserializeObject(bansJson);
                if (banRecords != null)
                {
                    foreach (dynamic br in banRecords)
                    {
                        banList.Add(JsonToBanRecord(br));
                    }
                }
            }
            return banList;
        }

        private static BanRecord JsonToBanRecord(dynamic br)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in br)
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

        /// <summary>
        /// Checks if the player is banned and if so how long the ban will remain active. 
        /// If the ban expired in the past, then the ban will be removed and the player will be allowed to join again.
        /// If the ban is not expired yet, then the player will be kicked with a message  displaying how long their ban will remain active.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="playerName"></param>
        /// <param name="kickCallback"></param>
        private void CheckForBans([FromSource]Player source, string playerName, CallbackDelegate kickCallback)
        {
            foreach (BanRecord ban in BannedPlayersList)
            {
                foreach (string identifier in source.Identifiers)
                {
                    if (ban.identifiers.Contains(identifier))
                    {
                        var timeRemaining = ban.bannedUntil.Subtract(DateTime.Now);
                        if (timeRemaining.TotalSeconds > 0)
                        {
                            if (ban.bannedUntil.Year == new DateTime(3000, 1, 1).Year)
                            {
                                kickCallback($"You have been permanently banned from this server. " +
                                             $"Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                            }
                            else
                            {
                                string timeRemainingMessage = GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                                kickCallback($"You are banned from this server. Ban time remaining: {timeRemainingMessage}"
                                          + $". Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                            }
                            if (MainServer.debug)
                                Debug.Write($"Player is still banned for {Math.Round(timeRemaining.TotalHours, 2)} hours.\n");
                            CancelEvent();
                        }
                        else
                        {
                            if (RemoveBan(ban))
                            {
                                if (MainServer.debug)
                                    Debug.WriteLine("Ban time expired, player has been removed from the ban list.");
                            }
                            else
                            {
                                if (MainServer.debug)
                                    Debug.WriteLine("Ban time expired, but an unknown error occurred while removing the player from the banlist!" +
                                                    " They have been allowed to join the server, but please remove them from the ban list manually!");
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Bans the specified player from the server.
        /// </summary>
        /// <param name="source">The player who triggered the event.</param>
        /// <param name="targetPlayer">The player that needs to be banned.</param>
        /// <param name="banReason">The reason why the player is getting banned.</param>
        private void BanPlayer([FromSource] Player source, int targetPlayer, string banReason)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.PermBan") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player target = new PlayerList()[targetPlayer];
                if (!IsPlayerAceAllowed(target.Handle, "vMenu.DontBanMe"))
                {
                    BanRecord ban = new BanRecord()
                    {
                        bannedBy = GetSafePlayerName(source.Name),
                        bannedUntil = new DateTime(3000, 1, 1),
                        banReason = banReason,
                        identifiers = target.Identifiers.ToList<string>(),
                        playerName = GetSafePlayerName(target.Name)
                    };
                    if (AddBan(ban))
                    {
                        if (MainServer.debug)
                            Debug.WriteLine("Ban successfull.");
                    }
                    else
                    {
                        if (MainServer.debug)
                            Debug.Write("Ban not successfull.");
                    }
                    BannedPlayersList = GetBanList();
                    target.Drop($"You have been permanently banned from this server. " +
                                $"Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                }
            }
            else
            {
                BanCheater(source);
            }
        }

        /// <summary>
        /// Bans the specified player for a the specified amount of hours.
        /// </summary>
        /// <param name="source">Player who triggered the event.</param>
        /// <param name="targetPlayer">Player who needs to be banned.</param>
        /// <param name="banDurationHours">Ban duration in hours.</param>
        /// <param name="banReason">Reason for the ban.</param>
        private void BanPlayer([FromSource] Player source, int targetPlayer, double banDurationHours, string banReason)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.TempBan") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player target = new PlayerList()[targetPlayer];
                if (!IsPlayerAceAllowed(target.Handle, "vMenu.DontBanMe"))
                {
                    BanRecord ban = new BanRecord()
                    {
                        bannedBy = GetSafePlayerName(source.Name),
                        bannedUntil = DateTime.Now.AddHours(banDurationHours <= 720.0 ? banDurationHours : 720.0),
                        banReason = banReason,
                        identifiers = target.Identifiers.ToList<string>(),
                        playerName = GetSafePlayerName(target.Name)
                    };
                    if (AddBan(ban))
                    {
                        if (MainServer.debug)
                            Debug.WriteLine("Ban successfull.");
                    }
                    else
                    {
                        if (MainServer.debug)
                            Debug.Write("Ban not successfull.");
                    }
                    BannedPlayersList = GetBanList();
                    string timeRemaining = GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                    target.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}"
                              + $". Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                }
            }
            else
            {
                BanCheater(source);
            }
        }

        /// <summary>
        /// Returns a formatted string displaying exactly how many days, hours and/or minutes a ban remains active.
        /// </summary>
        /// <param name="remainingTime"></param>
        /// <returns></returns>
        private string GetRemainingTimeMessage(TimeSpan remainingTime)
        {
            var message = "";
            if (remainingTime.Days > 0)
            {
                message += $"{remainingTime.Days} day{(remainingTime.Days > 1 ? "s" : "")} ";
            }
            if (remainingTime.Hours > 0)
            {
                message += $"{remainingTime.Hours} hour{(remainingTime.Hours > 1 ? "s" : "")} ";
            }
            if (remainingTime.Minutes > 0)
            {
                message += $"{remainingTime.Minutes} minute{(remainingTime.Minutes > 1 ? "s" : "")}";
            }
            if (remainingTime.Days < 1 && remainingTime.Hours < 1 && remainingTime.Minutes < 1)
            {
                message = "Less than 1 minute";
            }
            return message;
        }

        /// <summary>
        /// Adds a ban manually.
        /// </summary>
        /// <param name="ban"></param>
        /// <returns></returns>
        private static bool AddBan(BanRecord ban)
        {
            BannedPlayersList = GetBanList();
            var found = false;
            foreach (BanRecord b in BannedPlayersList)
            {
                b.identifiers.ForEach(i =>
                {
                    if (ban.identifiers.Contains(i))
                    {
                        found = true;
                    }
                });
                if (found)
                {
                    BannedPlayersList.Remove(b);
                    break;
                }
            }

            BannedPlayersList.Add(ban);

            var output = JsonConvert.SerializeObject(BannedPlayersList);
            return SaveResourceFile(GetCurrentResourceName(), "bans.json", output, output.Length);
        }

        /// <summary>
        /// Removes a ban record from the banned players list.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private bool RemoveBan(BanRecord record)
        {
            BannedPlayersList = GetBanList();
            List<int> itemsToRemove = new List<int>();
            foreach (BanRecord ban in BannedPlayersList)
            {
                if (!itemsToRemove.Contains(BannedPlayersList.IndexOf(ban)))
                {
                    var found = 0;
                    foreach (string s in ban.identifiers)
                    {
                        if (record.identifiers.Contains(s))
                        {
                            found++;
                        }
                    }

                    // if everything matches, we can be sure that this is the correct ban record/player so we can unban.
                    if (found == ban.identifiers.Count && ban.playerName == record.playerName && ban.bannedBy == record.bannedBy
                        && ban.banReason == record.banReason && ban.bannedUntil.ToString() == record.bannedUntil.ToString())
                    {
                        itemsToRemove.Add(BannedPlayersList.IndexOf(ban));
                    }
                }
            }
            for (var i = BannedPlayersList.Count; i > 0; i--)
            {
                if (itemsToRemove.Contains(i - 1) && i - 1 >= 0 && i - 1 < BannedPlayersList.Count)
                {
                    BannedPlayersList.RemoveAt(i - 1);
                }
            }
            var output = JsonConvert.SerializeObject(BannedPlayersList);
            return SaveResourceFile(GetCurrentResourceName(), "bans.json", output, output.Length);
        }

        private void RemoveBanRecord([FromSource]Player source, string banRecordJsonString)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Unban"))
            {
                dynamic obj = JsonConvert.DeserializeObject(banRecordJsonString);
                BanRecord ban = JsonToBanRecord(obj);
                RemoveBan(ban);
            }
            else
            {
                BanCheater(source);
            }
        }

        /// <summary>
        /// Someone trying to trigger fake server events? Well, goodbye idiots.
        /// </summary>
        /// <param name="source"></param>
        public static void BanCheater(Player source)
        {
            AddBan(new BanRecord()
            {
                bannedBy = "vMenu Auto Ban",
                bannedUntil = new DateTime(3000, 1, 1),
                banReason = "You have been automatically banned. If you believe this was done by error, please contact the server owner for support.",
                identifiers = source.Identifiers.ToList(),
                playerName = GetSafePlayerName(source.Name)
            });
            //source.Drop("Enjoy, idiot.");
            source.TriggerEvent("vMenu:GoodBye"); // this is much more fun than just kicking them.
        }


        /// <summary>
        /// Returns the safe playername string to be used in json converter to prevent fuckups when saving the bans file.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static string GetSafePlayerName(string playerName)
        {
            string safeName = playerName.Replace("^", "").Replace("<", "").Replace(">", "").Replace("~", "");
            safeName = Regex.Replace(safeName, @"[^\u0000-\u007F]+", string.Empty);
            return safeName.Trim(new char[] { '.', ',', ' ', '!', '?' });
        }
    }
}

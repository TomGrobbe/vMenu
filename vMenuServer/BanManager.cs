using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Text.RegularExpressions;
using static vMenuServer.DebugLog;

namespace vMenuServer
{
    public class BanManager : BaseScript
    {
        private static bool readingOrWritingToBanFile = false;

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

        /// <summary>
        /// List of ban records.
        /// </summary>
        public static List<BanRecord> BannedPlayersList { get; private set; } = new List<BanRecord>();

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

            //BannedPlayersList = await GetBanList();
        }

        /// <summary>
        /// Sends the banlist (as json string) to the client.
        /// </summary>
        /// <param name="source"></param>
        private async void SendBanList([FromSource] Player source)
        {
            BannedPlayersList = await GetBanList();
            Log("Updating player with new banlist.\n");
            string data = JsonConvert.SerializeObject(BannedPlayersList).ToString();
            //Debug.Write(data + "\n");
            source.TriggerEvent("vMenu:SetBanList", data);
        }

        /// <summary>
        /// Gets the ban list from the bans.json file.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BanRecord>> GetBanList()
        {
            while (readingOrWritingToBanFile)
            {
                await Delay(0);
            }
            readingOrWritingToBanFile = true;
            var banList = new List<BanRecord>();
            string bansJson = LoadResourceFile(GetCurrentResourceName(), "bans.json");
            if (bansJson != null && bansJson != "" && !string.IsNullOrEmpty(bansJson))
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
            readingOrWritingToBanFile = false;
            return banList;
        }

        /// <summary>
        /// Converts a json object into a BanRecord struct.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
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
        private async void CheckForBans([FromSource]Player source, string playerName, CallbackDelegate kickCallback)
        {
            BannedPlayersList = await GetBanList();
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
                            Log($"Player is still banned for {Math.Round(timeRemaining.TotalHours, 2)} hours.\n");
                            CancelEvent();
                        }
                        else
                        {
                            if (await RemoveBan(ban))
                            {
                                BanLog($"The following ban record has been removed (player unbanned). " +
                                    $"The player has been unbanned because their ban duration expired. [Player: {ban.playerName} " +
                                    $"was banned by {ban.bannedBy} for {ban.banReason} until {ban.bannedUntil}.]");
                            }
                            else
                            {
                                BanLog($"The player trying to join right now is on the banlist, their ban duration has expired bu for unknown reasons their" +
                                    $" ban could not be removed from the ban list. Please delete the ban record manually. " +
                                    $"\nBan Record details:\n{JsonConvert.SerializeObject(ban).ToString()}\n");
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
        private async void BanPlayer([FromSource] Player source, int targetPlayer, string banReason)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.PermBan") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") || IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Player target = new PlayerList()[targetPlayer];
                if (target != null)
                {
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
                        if (await AddBan(ban))
                        {
                            BanLog($"A new ban record has been added. Player: '{ban.playerName}' was banned by '{ban.bannedBy}' " +
                                $"for '{ban.banReason}' until '{ban.bannedUntil}' (forever).");
                            target.Drop($"You have been permanently banned from this server. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                            TriggerEvent("vMenu:BanSuccessful", JsonConvert.SerializeObject(ban).ToString());
                            source.TriggerEvent("vMenu:Notify", "~g~Target player successfully banned.");
                            BannedPlayersList = await GetBanList();
                        }
                        else
                        {
                            Log("Saving of new ban failed. Reason: unknown. Maybe the file is broken?\n", LogLevel.error);
                        }
                    }
                    else
                    {
                        Log("Could not ban player because they are exempt from being banned.", LogLevel.error);
                        source.TriggerEvent("vMenu:Notify", "~r~Could not ban this player, they are exempt from being banned.");
                    }
                    return;
                }
                Log("An error occurred while trying to ban someone. Error details: The specified target player is 'null', unknown reason.", LogLevel.error);
                TriggerClientEvent(player: source, eventName: "vMenu:Notify", args: "An unknown error occurred. Report it here: vespura.com/vmenu");
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
        private async void BanPlayer([FromSource] Player source, int targetPlayer, double banDurationHours, string banReason)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.TempBan") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Log("Source player is allowed to ban others.", LogLevel.info);
                Player target = new PlayerList()[targetPlayer];
                if (target != null)
                {
                    Log("Target player is not null so moving on.", LogLevel.info);
                    if (!IsPlayerAceAllowed(target.Handle, "vMenu.DontBanMe"))
                    {
                        Log("Target player (Player) does not have the 'dont ban me' permission, so we can continue to ban them.", LogLevel.info);
                        BanRecord ban = new BanRecord()
                        {
                            bannedBy = GetSafePlayerName(source.Name),
                            bannedUntil = DateTime.Now.AddHours(banDurationHours <= 720.0 ? banDurationHours : 720.0),
                            banReason = banReason,
                            identifiers = target.Identifiers.ToList<string>(),
                            playerName = GetSafePlayerName(target.Name)
                        };

                        Log("Ban record created.", LogLevel.info);
                        if (await AddBan(ban))
                        {
                            BanLog($"A new ban record has been added. Player: '{ban.playerName}' was banned by " +
                                $"'{ban.bannedBy}' for '{ban.banReason}' until '{ban.bannedUntil}'.");
                            TriggerEvent("vMenu:BanSuccessful", JsonConvert.SerializeObject(ban).ToString());
                            BannedPlayersList = await GetBanList();
                            string timeRemaining = GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                            target.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}");
                            source.TriggerEvent("vMenu:Notify", "~g~Target player successfully temp banned.");
                        }
                        else
                        {
                            Log("Saving of new ban failed. Reason: unknown. Maybe the file is broken?", LogLevel.error);
                            source.TriggerEvent("vMenu:Notify", "~r~Could not ban the target player, reason: unknown.");
                        }
                    }
                    else
                    {
                        Log("Player could not be banned because he is exempt from being banned.", LogLevel.error);
                        source.TriggerEvent("vMenu:Notify", "~r~Could not ban this player, they are exempt from being banned.");
                    }
                }
                else
                {
                    Log("Player is invalid (no longer online) and therefor the banning has failed.", LogLevel.error);
                    source.TriggerEvent("vMenu:Notify", "Could not temp-ban this player because they already left the server.");
                }
            }
            else
            {
                Log("If enabled, the source player will be banned now because they are cheating!", LogLevel.warning);
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
        private static async Task<bool> AddBan(BanRecord ban)
        {

            Log("Refreshing banned players list.", LogLevel.info);
            BannedPlayersList = await GetBanList();
            bool found = false;
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
            Log("Player is found as already banned? : " + found.ToString(), found ? LogLevel.warning : LogLevel.info);

            BannedPlayersList.Add(ban);

            var output = JsonConvert.SerializeObject(BannedPlayersList, Formatting.Indented);
            while (readingOrWritingToBanFile)
            {
                await Delay(0);
            }
            readingOrWritingToBanFile = true;
            bool successful = SaveResourceFile(GetCurrentResourceName(), "bans.json", output, -1);
            readingOrWritingToBanFile = false;
            return successful;
        }

        /// <summary>
        /// Removes a ban record from the banned players list.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static async Task<bool> RemoveBan(BanRecord record)
        {
            BannedPlayersList = await GetBanList();
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
            var output = JsonConvert.SerializeObject(BannedPlayersList, Formatting.Indented);
            while (readingOrWritingToBanFile)
            {
                await Delay(0);
            }
            readingOrWritingToBanFile = true;
            bool result = SaveResourceFile(GetCurrentResourceName(), "bans.json", output, -1);
            readingOrWritingToBanFile = false;
            return result;
        }

        /// <summary>
        /// Removes a ban record.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="banRecordJsonString"></param>
        private async void RemoveBanRecord([FromSource]Player source, string banRecordJsonString)
        {
            if (source != null && !string.IsNullOrEmpty(source.Name) && source.Name.ToLower() != "**invalid**" && source.Name.ToLower() != "** invalid **")
            {
                if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Unban") || IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything"))
                {
                    dynamic obj = JsonConvert.DeserializeObject(banRecordJsonString);
                    BanRecord ban = JsonToBanRecord(obj);
                    if (await RemoveBan(ban))
                    {
                        BanLog($"The following ban record has been removed (player unbanned). " +
                            $"[Player: {ban.playerName} was banned by {ban.bannedBy} for {ban.banReason} until {ban.bannedUntil}.]");
                        TriggerEvent("vMenu:UnbanSuccessful", JsonConvert.SerializeObject(ban).ToString());
                    }
                }
                else
                {
                    BanCheater(source);
                    Debug.WriteLine($"^3[vMenu] [WARNING] [BAN] ^7Player {JsonConvert.SerializeObject(source)} did not have the required permissions, but somehow triggered the unban event. Missing permissions: vMenu.OnlinePlayers.Unban (is ace allowed: {IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Unban")})\n");
                }
            }
            else
            {
                Debug.WriteLine("^3[vMenu] [WARNING] ^7The unban event was triggered, but no valid source was provided. Nobody has been unbanned.");
            }

        }

        /// <summary>
        /// Someone trying to trigger fake server events? Well, goodbye idiots.
        /// </summary>
        /// <param name="source"></param>
        public static async void BanCheater(Player source)
        {
            //bool enabled = (GetConvar("vMenuBanCheaters", "false") ?? "false") == "true";
            bool enabled = vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_auto_ban_cheaters);
            if (enabled)
            {
                var ban = new BanRecord()
                {
                    bannedBy = "vMenu Auto Ban",
                    bannedUntil = new DateTime(3000, 1, 1),
                    banReason = "You have been automatically banned. If you believe this was done by error, please contact the server owner for support.",
                    identifiers = source.Identifiers.ToList(),
                    playerName = GetSafePlayerName(source.Name)
                };

                if (await AddBan(ban))
                {
                    TriggerEvent("vMenu:BanCheaterSuccessful", JsonConvert.SerializeObject(ban).ToString());
                    BanLog($"A cheater has been banned. {JsonConvert.SerializeObject(ban).ToString()}");
                }

                source.TriggerEvent("vMenu:GoodBye"); // this is much more fun than just kicking them.
                Log("A cheater has been banned because they attempted to trigger a fake event.", LogLevel.warning);
            }
        }


        /// <summary>
        /// Returns the safe playername string to be used in json converter to prevent fuckups when saving the bans file.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static string GetSafePlayerName(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName))
            {
                string safeName = playerName.Replace("^", "").Replace("<", "").Replace(">", "").Replace("~", "");
                safeName = Regex.Replace(safeName, @"[^\u0000-\u007F]+", string.Empty);
                return safeName.Trim(new char[] { '.', ',', ' ', '!', '?' });
            }
            return "InvalidPlayerName";

        }

        /// <summary>
        /// If enabled using convars, will log all ban actions to the server console as well as an external file.
        /// </summary>
        /// <param name="banActionMessage"></param>
        public static void BanLog(string banActionMessage)
        {
            //if (GetConvar("vMenuLogBanActions", "true") == "true")
            if (vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_log_ban_actions))
            {
                string file = LoadResourceFile(GetCurrentResourceName(), "vmenu.log") ?? "";
                DateTime date = DateTime.Now;
                string formattedDate = (date.Day < 10 ? "0" : "") + date.Day + "-" +
                    (date.Month < 10 ? "0" : "") + date.Month + "-" +
                    (date.Year < 10 ? "0" : "") + date.Year + " " +
                    (date.Hour < 10 ? "0" : "") + date.Hour + ":" +
                    (date.Minute < 10 ? "0" : "") + date.Minute + ":" +
                    (date.Second < 10 ? "0" : "") + date.Second;
                string outputFile = file + $"[\t{formattedDate}\t] [BAN ACTION] {banActionMessage}\n";
                SaveResourceFile(GetCurrentResourceName(), "vmenu.log", outputFile, -1);
                Debug.WriteLine("^2[vMenu] [SUCCESS] [BAN]^7 " + banActionMessage);
            }
        }
    }
}

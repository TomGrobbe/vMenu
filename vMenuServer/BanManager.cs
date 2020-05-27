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
using System.Data.SQLite;

namespace vMenuServer
{
    public class BanManager : BaseScript
    {
        private static bool readingOrWritingToBanFile = false;
        internal static bool useJson = !vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_bans_use_database);
        private static readonly string bansDbFilePath = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_bans_database_filepath) ?? "";
        private const string bansDbFileName = "vmenu_bans.db";

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

            public BanRecord(string playerName, List<string> identifiers, DateTime bannedUntil, string banReason, string bannedBy)
            {
                this.playerName = playerName;
                this.identifiers = identifiers;
                this.bannedUntil = bannedUntil;
                this.banReason = banReason;
                this.bannedBy = bannedBy;
            }
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
            InitializeDbConnection();
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
            source.TriggerEvent("vMenu:SetBanList", data);
        }

        /// <summary>
        /// Gets the ban list from the bans.json file.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BanRecord>> GetBanList()
        {
            if (useJson)
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
            else
            {
                List<BanRecord> bans = new List<BanRecord>();
                try
                {
                    using (SQLiteConnection db = new SQLiteConnection($"Data Source='{bansDbFilePath}{bansDbFileName}';Version=3;"))
                    {
                        db.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM bans;", db))
                        {
                            using (SQLiteDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    string[] identifiers = JsonConvert.DeserializeObject<string[]>(rdr.GetString(0));
                                    string playername = rdr.GetString(1);
                                    string banreason = rdr.GetString(2);
                                    string bannedby = rdr.GetString(3);
                                    DateTime banneduntil = rdr.GetDateTime(4);
                                    var br = new BanRecord()
                                    {
                                        bannedBy = bannedby,
                                        bannedUntil = banneduntil,
                                        banReason = banreason,
                                        identifiers = identifiers.ToList(),
                                        playerName = playername
                                    };
                                    bans.Add(br);
                                }
                            }
                        }
                        db.Close();
                    }
                }
                catch (Exception e)
                {
                    Log("SQLite Exception caught: " + e.Message, LogLevel.error);
                }
                return bans;
            }
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
                    if (string.IsNullOrEmpty(newBr.playerName))
                    {
                        newBr.playerName = "(invalid or no name)";
                    }
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
        /// Checks if the player is banned in the SQLite database, if so then the output ban record list will be filled with all records found matching that player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private static bool IsPlayerSqlBanned(Player source, out List<BanRecord> r)
        {
            if (useJson)
            {
                r = new List<BanRecord>();
                return false;
            }
            string ids = "";
            foreach (string id in source.Identifiers)
            {
                ids += $"identifiers LIKE '%{id}%' OR ";
            }
            ids = ids.Trim(' ', 'R', 'O', ' ');
            ids += "";

            List<BanRecord> banRecordsForPlayer = new List<BanRecord>();

            try
            {
                using (SQLiteConnection db = new SQLiteConnection($"Data Source='{bansDbFilePath}{bansDbFileName}';Version=3;"))
                {
                    db.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand($"SELECT * FROM bans WHERE {ids};", db))
                    {
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string[] identifiers = JsonConvert.DeserializeObject<string[]>(rdr.GetString(0));
                                string playername = rdr.GetString(1);
                                string banreason = rdr.GetString(2);
                                string bannedby = rdr.GetString(3);
                                DateTime banneduntil = rdr.GetDateTime(4);
                                var br = new BanRecord(playername, identifiers.ToList(), banneduntil, banreason, bannedby);
                                banRecordsForPlayer.Add(br);
                            }
                        }
                    }
                    db.Close();
                }
            }
            catch (Exception e)
            {
                Log("SQLite error: " + e.Message, LogLevel.error);
            }

            if (banRecordsForPlayer.Count > 0)
            {
                r = banRecordsForPlayer;
                return true;
            }
            r = new List<BanRecord>();
            return false;
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
            if (!useJson)
            {
                if (IsPlayerSqlBanned(source, out List<BanRecord> records))
                {
                    if (records.Any((record) =>
                    {
                        var duration = record.bannedUntil.Subtract(DateTime.Now);
                        if (duration.TotalSeconds > 0)  // still banned
                        {
                            return true;
                        }
                        return false;
                    }))
                    {
                        var record = records[0];
                        if (record.bannedUntil.Year == 3000)
                        {
                            // banned forever
                            kickCallback($"You have been permanently banned from this server. Banned by: {record.bannedBy}. Ban reason: {record.banReason}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                            CancelEvent();
                            return;
                        }
                        else
                        {
                            // tempbanned.
                            kickCallback($"You are banned from this server. Ban time remaining: {GetRemainingTimeMessage(record.bannedUntil.Subtract(DateTime.Now))}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                            CancelEvent();
                            return;
                        }
                    }
                    else // should be unbanned because ban expired
                    {
                        foreach (var record in records)
                        {
                            RemoveSqlBanRecord(record);
                            BanLog($"The following ban record has been removed (player unbanned). The player has been unbanned because their ban duration expired. [Player: {record.playerName} was banned by {record.bannedBy} for {record.banReason} until {record.bannedUntil}.]");
                        }
                    }
                }
            }
            else
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
                                    kickCallback($"You have been permanently banned from this server. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                                }
                                else
                                {
                                    string timeRemainingMessage = GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                                    kickCallback($"You are banned from this server. Ban time remaining: {timeRemainingMessage}"
                                              + $". Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
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
                                        $"\nBan Record details:\n{JsonConvert.SerializeObject(ban)}\n");
                                }
                            }
                            break;
                        }
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
            BanPlayer(source, targetPlayer, -1.0, banReason);
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
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.TempBan") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") || IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                Log("Source player is allowed to ban others.", LogLevel.info);
                Player target = Players[targetPlayer];
                if (target != null)
                {
                    Log("Target player is not null so moving on.", LogLevel.info);
                    if (!IsPlayerAceAllowed(target.Handle, "vMenu.DontBanMe"))
                    {
                        Log("Target player (Player) does not have the 'dont ban me' permission, so we can continue to ban them.", LogLevel.info);
                        var banduration = (banDurationHours > 0 ?
                                                /* ban temporarily */ (DateTime.Now.AddHours(banDurationHours <= 720.0 ? banDurationHours : 720.0)) :
                                                /* ban forever */ (new DateTime(3000, 1, 1)));
                        if (useJson)
                        {
                            BanRecord ban = new BanRecord()
                            {
                                bannedBy = GetSafePlayerName(source.Name),
                                bannedUntil = banduration,
                                banReason = banReason,
                                identifiers = target.Identifiers.ToList(),
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
                                target.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}. Aditional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                                source.TriggerEvent("vMenu:Notify", "~g~Target player successfully banned.");
                            }
                            else
                            {
                                Log("Saving of new ban failed. Reason: unknown. Maybe the file is broken?", LogLevel.error);
                                source.TriggerEvent("vMenu:Notify", "~r~Could not ban the target player, reason: unknown.");
                            }
                        }
                        else
                        {
                            BanRecord br = new BanRecord(GetSafePlayerName(target.Name), target.Identifiers.ToList(), banduration, banReason, GetSafePlayerName(source.Name));
                            if (AddSqlBan(br))
                            {
                                BanLog($"A new ban record has been added. Player: '{br.playerName}' was banned by " +
                                    $"'{br.bannedBy}' for '{br.banReason}' until '{br.bannedUntil}'.");
                                TriggerEvent("vMenu:BanSuccessful", JsonConvert.SerializeObject(br).ToString());
                                BannedPlayersList = await GetBanList();
                                string timeRemaining = GetRemainingTimeMessage(br.bannedUntil.Subtract(DateTime.Now));
                                target.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}. Banned by: {br.bannedBy}. Ban reason: {br.banReason}. Aditional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                                source.TriggerEvent("vMenu:Notify", "~g~Target player successfully banned.");
                            }
                            else
                            {
                                Log("Saving of new ban failed. Reason: unknown. Maybe the file is broken?", LogLevel.error);
                                source.TriggerEvent("vMenu:Notify", "~r~Could not ban the target player, reason: unknown.");
                            }
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
                    source.TriggerEvent("vMenu:Notify", "Could not ban this player because they already left the server.");
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
        internal static string GetRemainingTimeMessage(TimeSpan remainingTime)
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
        internal static async Task<bool> AddBan(BanRecord ban)
        {
            if (useJson)
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

                var formattingMode = Formatting.None;
                if (BannedPlayersList.Count < 100)
                {
                    formattingMode = Formatting.Indented;
                }
                var output = JsonConvert.SerializeObject(BannedPlayersList, formattingMode);
                while (readingOrWritingToBanFile)
                {
                    await Delay(0);
                }
                readingOrWritingToBanFile = true;
                bool successful = SaveResourceFile(GetCurrentResourceName(), "bans.json", output, -1);
                readingOrWritingToBanFile = false;
                return successful;
            }
            else
            {
                AddSqlBan(ban);
                return true;
            }

        }

        /// <summary>
        /// Adds a new ban record to the SQLite database.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        internal static bool AddSqlBan(BanRecord br)
        {
            return AddSqlBanRange(new List<BanRecord>() { br }, false);
        }

        /// <summary>
        /// Adds a collection of ban records to the SQL database.
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        internal static bool AddSqlBanRange(List<BanRecord> records, bool logOutput)
        {
            try
            {
                using (SQLiteConnection db = new SQLiteConnection($"Data Source='{bansDbFilePath}{bansDbFileName}';Version=3;"))
                {
                    db.Open();

                    foreach (var br in records)
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand($"INSERT INTO bans (identifiers, playername, banreason, bannedby, banneduntil) VALUES (\"{JsonConvert.SerializeObject(br.identifiers).Replace("\"", "'")}\", \"{br.playerName.Replace("\"", "'")}\", \"{br.banReason.Replace("\"", "'")}\", \"{br.bannedBy.Replace("\"", "'")}\", datetime(\"{GetFormattedDate(br.bannedUntil)}\"));", db))
                        {
                            cmd.ExecuteNonQuery();
                            if (logOutput)
                            {
                                Debug.WriteLine($"[vMenu] Adding new ban record to database, record: {JsonConvert.SerializeObject(br)}");
                            }
                        }
                    }
                    db.Dispose();
                    return true;
                }
            }
            catch (Exception e)
            {
                Log($"SQLite Error: {e.Message}", LogLevel.error);
                return false;
            }
        }

        /// <summary>
        /// Removes a ban record from the banned players list.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static async Task<bool> RemoveBan(BanRecord record)
        {
            if (useJson)
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

                var formattingMode = Formatting.None;
                if (BannedPlayersList.Count < 100)
                {
                    formattingMode = Formatting.Indented;
                }
                var output = JsonConvert.SerializeObject(BannedPlayersList, formattingMode);
                while (readingOrWritingToBanFile)
                {
                    await Delay(0);
                }
                readingOrWritingToBanFile = true;
                bool result = SaveResourceFile(GetCurrentResourceName(), "bans.json", output, -1);
                readingOrWritingToBanFile = false;
                return result;
            }
            else
            {
                RemoveSqlBanRecord(record);
                return true;
            }
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
        /// Removes a ban record from the SQLite database.
        /// </summary>
        /// <param name="br"></param>
        private static void RemoveSqlBanRecord(BanRecord br)
        {
            string ids = "";
            foreach (string id in br.identifiers)
            {
                ids += $"identifiers LIKE '%{id}%' OR ";
            }
            ids = ids.Trim(' ', 'R', 'O', ' ');
            ids += "";
            try
            {
                using (SQLiteConnection db = new SQLiteConnection($"Data Source='{bansDbFilePath}{bansDbFileName}';Version=3;"))
                {
                    db.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand($"DELETE FROM bans WHERE {ids};", db))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    db.Close();
                }
            }
            catch (Exception e)
            {
                Log("SQLite error: " + e.Message, LogLevel.error);
            }
        }

        /// <summary>
        /// Someone trying to trigger fake server events? Well, goodbye idiots.
        /// </summary>
        /// <param name="source"></param>
        public static async void BanCheater(Player source)
        {
            bool enabled = vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_auto_ban_cheaters);
            if (enabled)
            {
                string reason = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_auto_ban_cheaters_ban_message);

                if (string.IsNullOrEmpty(reason))
                {
                    reason = $"You have been automatically banned. If you believe this was done by error, please contact the server owner for support. Aditional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.";
                }
                var ban = new BanRecord()
                {
                    bannedBy = "vMenu Auto Ban",
                    bannedUntil = new DateTime(3000, 1, 1),
                    banReason = reason,
                    identifiers = source.Identifiers.ToList(),
                    playerName = GetSafePlayerName(source.Name)
                };

                if (await AddBan(ban))
                {
                    TriggerEvent("vMenu:BanCheaterSuccessful", JsonConvert.SerializeObject(ban).ToString());
                    BanLog($"A cheater has been banned. {JsonConvert.SerializeObject(ban)}");
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
                safeName = safeName.Trim(new char[] { '.', ',', ' ', '!', '?' });
                if (string.IsNullOrEmpty(safeName))
                {
                    safeName = "InvalidPlayerName";
                }
                return safeName;
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

        /// <summary>
        /// Initializes the database file if enabled.
        /// </summary>
        public static void InitializeDbConnection()
        {
            if (!useJson)
            {
                try
                {
                    if (!System.IO.File.Exists(bansDbFilePath + bansDbFileName))
                    {
                        SQLiteConnection.CreateFile(bansDbFilePath + bansDbFileName);
                        Debug.WriteLine("[vMenu] Created bans DB.");
                    }

                    SQLiteConnection db = new SQLiteConnection($"Data Source='{bansDbFilePath}{bansDbFileName}';Version=3;");
                    db.Open();
                    string sql = "CREATE TABLE IF NOT EXISTS bans (identifiers STRING, playername STRING, banreason STRING, bannedby STRING, banneduntil DATETIME);";
                    SQLiteCommand cmd = new SQLiteCommand(sql, db);
                    cmd.ExecuteNonQuery();
                    db.Dispose();
                }
                catch (Exception e)
                {
                    Log("SQLite error: " + e.Message, LogLevel.error);
                }
            }
        }

        /// <summary>
        /// Gets the formatted date to be converted into a proper datetime type for the SQLite DB.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetFormattedDate(DateTime date)
        {
            return $"{date.Year}-{(date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString())}-{(date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString())} {(date.Hour < 10 ? "0" + date.Hour.ToString() : date.Hour.ToString())}:{(date.Minute < 10 ? "0" + date.Minute.ToString() : date.Minute.ToString())}:{(date.Second < 10 ? "0" + date.Second.ToString() : date.Second.ToString())}";
        }

        /// <summary>
        /// Migrates all bans from the bans.json file into the database.
        /// </summary>
        public static async void MigrateBansToDatabase()
        {
            try
            {
                if (!useJson)
                {
                    Log("You need to be using the bans.json if you want to migrate the bans.json to the database! Check the convar in the permissions.cfg, restart the server and try again.", LogLevel.error);
                }
                else
                {
                    useJson = false;
                    InitializeDbConnection();
                    useJson = true;
                    var bans = await GetBanList();
                    Debug.WriteLine($"[vMenu] Migrating {bans.Count} bans from the bans.json file to the vmenu_bans.db database!");
                    AddSqlBanRange(bans, true);
                    Debug.WriteLine("[vMenu] Done migrating all ban records from the bans.json file to the vmenu_bans.db database!");
                    Log("Now that all bans are migrated, please make sure to switch the config option to use the sqlite db instead of the bans.json in the permissions.cfg. Otherwise the database will NOT be used! I recommend that you make a backup of your bans.json (just in case), and then delete the original bans.json to test that your setup is working!", LogLevel.warning);
                }
            }
            catch (Exception e)
            {
                Log("Exception while migrating json bans to database.", LogLevel.error);
                Log("Error details: " + e.Message, LogLevel.error);
            }
        }
    }
}

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
        private const string BAN_KVP_PREFIX = "vmenu_ban_";
        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public class BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
            public Guid uuid;

            public BanRecord(string playerName, List<string> identifiers, DateTime bannedUntil, string banReason, string bannedBy, Guid uuid)
            {
                this.playerName = playerName;
                this.identifiers = identifiers;
                this.bannedUntil = bannedUntil;
                this.banReason = banReason;
                string uuidSuffix = $"\nYour ban id: {uuid}";
                if (!this.banReason.Contains(uuidSuffix) && uuid != Guid.Empty)
                {
                    this.banReason += uuidSuffix;
                }
                this.bannedBy = bannedBy;
                this.uuid = uuid;
            }
        }

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
        }

        /// <summary>
        /// Sends the banlist (as json string) to the client.
        /// </summary>
        /// <param name="source"></param>
        private void SendBanList([FromSource] Player source)
        {
            Log("Updating player with new banlist.\n");
            string data = JsonConvert.SerializeObject(GetBanList()).ToString();
            source.TriggerEvent("vMenu:SetBanList", data);
        }

        private static List<BanRecord> cachedBansList = new List<BanRecord>();
        private static bool bansHaveChanged = true;

        /// <summary>
        /// Gets the cached ban list or refreshes the bans list from the kvp storage when there has been a change.
        /// </summary>
        /// <returns></returns>
        public static List<BanRecord> GetBanList()
        {
            if (bansHaveChanged)
            {
                bansHaveChanged = false;
                int handle = StartFindKvp(BAN_KVP_PREFIX);
                List<string> kvpIds = new List<string>();
                while (true)
                {
                    string id = FindKvp(handle);
                    if (string.IsNullOrEmpty(id)) break;
                    kvpIds.Add(id);
                }
                EndFindKvp(handle);

                List<BanRecord> banRecords = new List<BanRecord>();

                foreach (string kvpId in kvpIds)
                {
                    banRecords.Add(JsonConvert.DeserializeObject<BanRecord>(GetResourceKvpString(kvpId)));
                }
                cachedBansList = banRecords;
                return banRecords;
            }
            else
            {
                return cachedBansList;
            }
        }

        /// <summary>
        /// Checks if the player is banned and if so how long the ban will remain active. 
        /// If the ban expired in the past, then the ban will be removed and the player will be allowed to join again.
        /// If the ban is not expired yet, then the player will be kicked with a message  displaying how long their ban will remain active.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="playerName"></param>
        /// <param name="kickCallback"></param>
        private void CheckForBans([FromSource] Player source, string playerName, CallbackDelegate kickCallback)
        {
            // Take care of expired bans.
            var oldBans = GetBanList().Where(banRecord =>
            {
                return banRecord.bannedUntil.Subtract(DateTime.Now).TotalSeconds <= 0;
            }).ToList();

            oldBans.ForEach(br =>
            {
                RemoveBan(br);
            });

            // Now look for active bans.
            var records = GetBanList().Where((banRecord) =>
            {
                return banRecord.bannedUntil.Subtract(DateTime.Now).TotalSeconds > 0;
            }).ToList();

            // Find any bans with matching player identifiers.
            var record = records.Find(br =>
            {
                return br.identifiers.Any(identifier =>
                {
                    return source.Identifiers.Contains(identifier);
                });
            });

            // If no record is found, stop.
            if (record == null)
            {
                return;
            }

            // Perm banned.
            if (record.bannedUntil.Year >= 3000)
            {
                kickCallback($"You have been permanently banned from this server. Banned by: {record.bannedBy}. Ban reason: {record.banReason}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                CancelEvent();
            }
            // Temp banned
            else
            {
                kickCallback($"You are banned from this server. Ban time remaining: {GetRemainingTimeMessage(record.bannedUntil.Subtract(DateTime.Now))}. Additional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                CancelEvent();
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
        private void BanPlayer([FromSource] Player source, int targetPlayer, double banDurationHours, string banReason)
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

                        BanRecord ban = new BanRecord(
                            GetSafePlayerName(target.Name),
                            target.Identifiers.ToList(),
                            banduration,
                            banReason,
                            GetSafePlayerName(source.Name),
                            Guid.NewGuid()
                        );

                        AddBan(ban);
                        Log("Ban record created.", LogLevel.info);
                        BanLog($"A new ban record has been added. Player: '{ban.playerName}' was banned by " +
                            $"'{ban.bannedBy}' for '{ban.banReason}' until '{ban.bannedUntil}'.");
                        TriggerEvent("vMenu:BanSuccessful", JsonConvert.SerializeObject(ban).ToString());

                        string timeRemaining = GetRemainingTimeMessage(ban.bannedUntil.Subtract(DateTime.Now));
                        target.Drop($"You are banned from this server. Ban time remaining: {timeRemaining}. Banned by: {ban.bannedBy}. Ban reason: {ban.banReason}. Aditional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.");
                        source.TriggerEvent("vMenu:Notify", "~g~Target player successfully banned.");
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
        internal static void AddBan(BanRecord ban)
        {
            string existingRecord = GetResourceKvpString(BAN_KVP_PREFIX + ban.uuid.ToString());
            if (string.IsNullOrEmpty(existingRecord))
            {
                SetResourceKvp(BAN_KVP_PREFIX + ban.uuid.ToString(), JsonConvert.SerializeObject(ban));
                bansHaveChanged = true;
            }
            else
            {
                Log("Ban record already exists, this is very odd.", LogLevel.error);
            }
        }

        /// <summary>
        /// Removes a ban record from the banned players list.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static void RemoveBan(BanRecord record)
        {
            DeleteResourceKvp(BAN_KVP_PREFIX + record.uuid.ToString());
            bansHaveChanged = true;
        }

        /// <summary>
        /// Removes a ban record.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="banRecordJsonString"></param>
        private void RemoveBanRecord([FromSource] Player source, string uuid)
        {
            if (source != null && !string.IsNullOrEmpty(source.Name) && source.Name.ToLower() != "**invalid**" && source.Name.ToLower() != "** invalid **")
            {
                if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Unban") || IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything"))
                {
                    var banRecord = GetBanList().Find((ban) =>
                    {
                        return ban.uuid.ToString() == uuid;
                    });
                    if (banRecord != null)
                    {
                        RemoveBan(banRecord);

                        BanLog($"The following ban record has been removed (player unbanned). " +
                            $"[Player: {banRecord.playerName} was banned by {banRecord.bannedBy} for {banRecord.banReason} until {banRecord.bannedUntil}.]");
                        TriggerEvent("vMenu:UnbanSuccessful", JsonConvert.SerializeObject(banRecord).ToString());
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
        public static void BanCheater(Player source)
        {
            bool enabled = vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_auto_ban_cheaters);
            if (enabled)
            {
                string reason = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_auto_ban_cheaters_ban_message);

                if (string.IsNullOrEmpty(reason))
                {
                    reason = $"You have been automatically banned. If you believe this was done by error, please contact the server owner for support. Aditional information: {vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_default_ban_message_information)}.";
                }
                var ban = new BanRecord(
                    GetSafePlayerName(source.Name),
                    source.Identifiers.ToList(),
                    new DateTime(3000, 1, 1),
                    reason,
                    "vMenu Auto Ban",
                    Guid.NewGuid()
                );

                AddBan(ban);

                TriggerEvent("vMenu:BanCheaterSuccessful", JsonConvert.SerializeObject(ban).ToString());
                BanLog($"A cheater has been banned. {JsonConvert.SerializeObject(ban)}");

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
        /// Gets the formatted date to be converted into a proper datetime type for the SQLite DB.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetFormattedDate(DateTime date)
        {
            return $"{date.Year}-{(date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString())}-{(date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString())} {(date.Hour < 10 ? "0" + date.Hour.ToString() : date.Hour.ToString())}:{(date.Minute < 10 ? "0" + date.Minute.ToString() : date.Minute.ToString())}:{(date.Second < 10 ? "0" + date.Second.ToString() : date.Second.ToString())}";
        }
    }
}

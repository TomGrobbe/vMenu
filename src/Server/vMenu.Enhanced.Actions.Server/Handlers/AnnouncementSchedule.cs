using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;
using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;
using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class AnnouncementSchedule
{
    private const string ConfigFile = "config/announcements.json";

    private const int TickIntervalMs = 10_000;

    private const int NameLimit = 40;

    private const int TextLimit = 200;

    private const int MinEveryMinutes = 1;

    private const int MaxEveryMinutes = 1440;

    private static readonly ActionRateLimit Limit = new(
        "announcement schedule edit",
        OnlinePlayerSettings.ActionLimit,
        OnlinePlayerSettings.ActionLimitSeconds);

    private static readonly List<Scheduled> Entries = [];

    private static TickHandle? _tick;

    public static void Register()
    {
        Load();

        ActionRegistry.Register(
            ActionIds.Admin.GetAnnouncements,
            AdminPermissions.ManageAnnouncements,
            GetAnnouncements);

        ActionRegistry.Register(
            ActionIds.Admin.AddAnnouncement,
            AdminPermissions.ManageAnnouncements,
            AddAnnouncement,
            Limit);

        ActionRegistry.Register(
            ActionIds.Admin.RemoveAnnouncement,
            AdminPermissions.ManageAnnouncements,
            RemoveAnnouncement,
            Limit);

        _tick = ServerTickRegistry.Register(
            "Admin.Announcements",
            Tick,
            TickRate.Every(TickIntervalMs),
            condition: IsWanted);

        ServerConfig.AddEventListenerFor([AdminSettings.ScheduledAnnouncements], _tick.Reevaluate);
    }

    private static bool IsWanted() =>
        Entries.Count > 0 && ServerConfig.Value(AdminSettings.ScheduledAnnouncements);

    private static void Tick()
    {
        var realNow = DateTimeOffset.Now;
        var realSecondOfDay = (realNow.Hour * 3600) + (realNow.Minute * 60) + realNow.Second;
        var gameSecondOfDay = ServerClock.InGameSecondOfDay();
        var uptime = Native.GetGameTimer();

        foreach (var entry in Entries)
        {
            if (!entry.IsDue(uptime, realSecondOfDay, gameSecondOfDay))
            {
                continue;
            }

            var reached = AdminActions.Broadcast(entry.Text);

            Log.Debug($"[Admin] Scheduled announcement '{entry.Name}' went out to {reached} player(s).");
        }
    }

    private static ActionResponse GetAnnouncements(Player source, string[] args)
    {
        var rows = new string[Entries.Count];

        for (var index = 0; index < Entries.Count; index++)
        {
            var entry = Entries[index];

            rows[index] = AnnouncementRow.Format(
                index,
                entry.Name,
                entry.Text,
                entry.EveryMinutes,
                entry.At,
                entry.Clock);
        }

        return ActionResponse.Ok(rows);
    }

    private static ActionResponse AddAnnouncement(Player source, string[] args)
    {
        if (args.Length < 5
            || Trimmed(args[0]) is not { } name
            || Trimmed(args[1]) is not { } text
            || !int.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var everyMinutes))
        {
            return ActionResponse.InvalidRequest();
        }

        var at = Trimmed(args[3]) ?? string.Empty;
        var clock = AnnouncementClock.IsGame(args[4]) ? AnnouncementClock.Game : AnnouncementClock.Real;

        var added = new Scheduled
        {
            Name = Cap(name, NameLimit),
            Text = Cap(text, TextLimit),
            EveryMinutes = everyMinutes,
            At = at,
            Clock = clock,
        };

        if (!added.IsUsable(out var complaint))
        {
            Log.Warning($"[Admin] {source.Name} tried to add an announcement that {complaint}.");

            return ActionResponse.InvalidRequest();
        }

        added.Prime(Native.GetGameTimer());

        Entries.Add(added);

        if (!Save())
        {
            Entries.Remove(added);

            return ActionResponse.Failed();
        }

        _tick?.Reevaluate();

        Log.Info($"[Admin] {source.Name} added the scheduled announcement '{added.Name}'.");

        return ActionResponse.Ok();
    }

    private static ActionResponse RemoveAnnouncement(Player source, string[] args)
    {
        if (args.Length < 2
            || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
            || Trimmed(args[1]) is not { } name)
        {
            return ActionResponse.InvalidRequest();
        }

        if (index < 0 || index >= Entries.Count || !string.Equals(Entries[index].Name, name, StringComparison.Ordinal))
        {
            return ActionResponse.NotFound();
        }

        var removed = Entries[index];

        Entries.RemoveAt(index);

        if (!Save())
        {
            Entries.Insert(index, removed);

            return ActionResponse.Failed();
        }

        _tick?.Reevaluate();

        Log.Info($"[Admin] {source.Name} removed the scheduled announcement '{removed.Name}'.");

        return ActionResponse.Ok();
    }

    private static void Load()
    {
        var contents = Native.LoadResourceFile(Native.GetCurrentResourceName(), ConfigFile);

        if (string.IsNullOrWhiteSpace(contents))
        {
            Log.Info($"[Admin] No {ConfigFile} found, so no announcements send themselves.");

            return;
        }

        if (!ServerJson.TryDeserialize<ScheduleFile>(contents, out var read, out var error))
        {
            Log.Error($"[Admin] {ConfigFile} could not be parsed, so no announcements send themselves: {error}");

            return;
        }

        if (read is null)
        {
            Log.Error($"[Admin] {ConfigFile} has to hold an object with an announcements list, so none send themselves.");

            return;
        }

        var uptime = Native.GetGameTimer();

        foreach (var entry in read.Announcements ?? [])
        {
            if (!entry.IsUsable(out var complaint))
            {
                Log.Warning($"[Admin] Skipping the announcement '{entry.Name}' in {ConfigFile}: it {complaint}.");

                continue;
            }

            entry.Name = Cap(entry.Name.Trim(), NameLimit);
            entry.Text = Cap(entry.Text.Trim(), TextLimit);
            entry.At = entry.At?.Trim() ?? string.Empty;
            entry.Clock = AnnouncementClock.IsGame(entry.Clock) ? AnnouncementClock.Game : AnnouncementClock.Real;

            entry.Prime(uptime);

            Entries.Add(entry);
        }

        Log.Debug($"[Admin] {Entries.Count} scheduled announcement(s) loaded from {ConfigFile}.");

        WarnAboutGameClock();
    }

    private static void WarnAboutGameClock()
    {
        if (ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            return;
        }

        foreach (var entry in Entries)
        {
            if (AnnouncementClock.IsGame(entry.Clock))
            {
                Log.Warning(
                    $"[Admin] The announcement '{entry.Name}' is set against the in game clock, but "
                    + $"{TimeOptionsSettings.Enabled.Name} is off, so vMenu is not driving that clock and the "
                    + "announcement will not land at the time players see.");
            }
        }
    }

    private static bool Save()
    {
        try
        {
            var json = ServerJson.SerializeIndented(new ScheduleFile { Announcements = Entries });

            if (Native.SaveResourceFile(Native.GetCurrentResourceName(), ConfigFile, Encoding.UTF8.GetBytes(json)))
            {
                return true;
            }

            Log.Error($"[Admin] {ConfigFile} could not be written, so nothing was changed.");
        }
        catch (Exception exception)
        {
            Log.Error($"[Admin] Writing {ConfigFile} threw, so nothing was changed: {exception}");
        }

        return false;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Cap(string value, int limit) =>
        value.Length <= limit ? value : value[..limit];

    private sealed class ScheduleFile
    {
        public List<Scheduled>? Announcements { get; set; }
    }

    public sealed class Scheduled
    {
        private double _atSecondOfDay = -1;

        private long _nextAt;

        private double _gameSecondsAccrued;

        private double _lastSecondOfDay = -1;

        public string Name { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int EveryMinutes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string At { get; set; } = string.Empty;

        public string Clock { get; set; } = AnnouncementClock.Real;

        public bool IsUsable(out string complaint)
        {
            complaint = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                complaint = "has no name";

                return false;
            }

            if (string.IsNullOrWhiteSpace(Text))
            {
                complaint = "has nothing to say";

                return false;
            }

            if (!AnnouncementClock.IsKnown(Clock))
            {
                complaint = $"asks for a '{Clock}' clock, which is neither 'real' nor 'game'";

                return false;
            }

            var repeating = EveryMinutes > 0;
            var timed = TryReadAt(At, out _);

            if (repeating && !string.IsNullOrWhiteSpace(At))
            {
                complaint = "sets both everyMinutes and at, and can only have one";

                return false;
            }

            if (repeating)
            {
                if (EveryMinutes is < MinEveryMinutes or > MaxEveryMinutes)
                {
                    complaint = $"repeats every {EveryMinutes} minutes, which is outside {MinEveryMinutes} to {MaxEveryMinutes}";

                    return false;
                }

                return true;
            }

            if (!timed)
            {
                complaint = "sets neither everyMinutes nor a valid at time like 20:00";

                return false;
            }

            return true;
        }

        public void Prime(long uptime)
        {
            if (EveryMinutes > 0)
            {
                _nextAt = uptime + (EveryMinutes * 60L * 1000L);
                _gameSecondsAccrued = 0;
                _lastSecondOfDay = -1;

                return;
            }

            TryReadAt(At, out _atSecondOfDay);

            _lastSecondOfDay = -1;
        }

        public bool IsDue(long uptime, double realSecondOfDay, double gameSecondOfDay)
        {
            if (EveryMinutes > 0)
            {
                return AnnouncementClock.IsGame(Clock)
                    ? EveryGameMinutesDue(gameSecondOfDay)
                    : EveryRealMinutesDue(uptime);
            }

            if (_atSecondOfDay < 0)
            {
                return false;
            }

            var now = AnnouncementClock.IsGame(Clock) ? gameSecondOfDay : realSecondOfDay;
            var previous = _lastSecondOfDay;

            _lastSecondOfDay = now;

            if (previous < 0)
            {
                return false;
            }

            return Passed(previous, now, _atSecondOfDay);
        }

        private bool EveryRealMinutesDue(long uptime)
        {
            if (uptime < _nextAt)
            {
                return false;
            }

            _nextAt = uptime + (EveryMinutes * 60L * 1000L);

            return true;
        }

        private bool EveryGameMinutesDue(double gameSecondOfDay)
        {
            var previous = _lastSecondOfDay;

            _lastSecondOfDay = gameSecondOfDay;

            if (previous < 0)
            {
                return false;
            }

            var elapsed = GameClock.Mod(gameSecondOfDay - previous, GameClock.SecondsPerGameDay);

            _gameSecondsAccrued += elapsed;

            var wanted = EveryMinutes * 60.0;

            if (_gameSecondsAccrued < wanted)
            {
                return false;
            }

            // Reset, never subtract. A fast clock overshoots by more than a whole interval easily, and
            // subtracting would leave it due again on the next tick, and the one after that.
            _gameSecondsAccrued = 0;

            return true;
        }

        // Asks whether the target was passed between ticks rather than whether the clock is near it now.
        // The game clock moves minutes per tick, so any fixed window is stepped clean over on most days.
        private static bool Passed(double previous, double now, double target)
        {
            if (now == previous)
            {
                return false;
            }

            if (now > previous)
            {
                return target > previous && target <= now;
            }

            return target > previous || target <= now;
        }

        private static bool TryReadAt(string? at, out double secondOfDay)
        {
            secondOfDay = -1;

            if (string.IsNullOrWhiteSpace(at))
            {
                return false;
            }

            var parts = at.Trim().Split(':', 2);

            if (parts.Length != 2
                || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hour)
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minute)
                || hour is < 0 or > 23
                || minute is < 0 or > 59)
            {
                return false;
            }

            secondOfDay = (hour * 3600) + (minute * 60);

            return true;
        }
    }
}

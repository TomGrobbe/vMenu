using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.Updates;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Ticks.Server;

using UpdateSettings = vMenu.Enhanced.Data.Configuration.Settings.Updates;

namespace vMenu.Enhanced.Updates.Server;

/// <summary>
/// Looks for a newer vMenu Enhanced, says so in the console, and tells staff who are online.
/// </summary>
public static class UpdateChecker
{
    private const string CheckCommand = "vmenu_checkupdates";

    private const long IntervalMs = 6L * 60L * 60L * 1000L;

    private const int TimeoutMs = 15000;

    private const string VersionKey = "version";

    /// <summary>What CI replaces on a real build. A build made locally still says this.</summary>
    private const string UnstampedVersion = "versiongoeshere";

    /// <summary>Stands in for a build with no version, so it loses to every real release.</summary>
    private const string DevelopmentVersion = "0.0.0";

    /// <summary>Staff already told about whatever is currently in <see cref="_available"/>.</summary>
    private static readonly HashSet<int> Told = [];

    private static SemanticVersion? _current;

    private static bool _readCurrent;

    private static bool _unstamped;

    private static KnownUpdate? _available;

    private static bool _running;

    private static bool _warned;

    private static string? _reportedMode;

    public static void Initialize()
    {
        API.OnNetEvent(UpdateEvents.Request, new Action<Player>(OnRequested), false);

        var tick = ServerTickRegistry.Register(
            "Updates.Check",
            CheckAsync,
            TickRate.Every(IntervalMs),
            condition: IsWanted);

        ServerConfig.AddEventListenerFor([UpdateSettings.CheckMode], tick.Reevaluate);

        // Not behind the debug gate. It does something rather than report on state, and an owner who
        // has just been told an update exists needs it whether or not they are debugging, which is
        // the same call ServerClock makes for vmenu_resettime.
        SharedAPI.Commands.RegisterCommand(CheckCommand, true, new Action(CheckNow));
    }

    /// <summary>Identifies vMenu to github.com and nuget.org, both of which want one.</summary>
    // "dev" rather than the stand-in version, so a request from an unreleased build is recognisable
    // as one in anybody's logs rather than looking like a real 0.0.0 release.
    public static string UserAgent()
    {
        var current = Current();

        return "vMenu.Enhanced/" + (_unstamped ? "dev" : current.Text);
    }

    private static void CheckNow()
    {
        if (Channel() == UpdateChannel.Off)
        {
            Log.Info(
                $"[Updates] Update checking is off. Set {UpdateSettings.CheckMode.Name} to stable or " +
                "prerelease to turn it on.");

            return;
        }

        if (_running)
        {
            Log.Info("[Updates] A check is already running.");

            return;
        }

        Log.Info("[Updates] Checking for a newer version.");

        _ = CheckNowAsync();
    }

    // The suppression in Warn is there for the schedule, not for somebody who just typed the command
    // and is owed an answer either way, so this reports its own outcome at Info regardless.
    private static async Task CheckNowAsync()
    {
        var before = _available;

        await CheckAsync();

        if (_available is { } found && !ReferenceEquals(found, before))
        {
            Log.Info($"[Updates] v{found.Version} is available. {found.Url}");
        }
        else if (_available is { } known)
        {
            Log.Info($"[Updates] v{known.Version} is still the newest, and you already knew.");
        }
        else if (_warned)
        {
            Log.Info("[Updates] Nothing answered, so this check found nothing. Turn on debug logging to see why.");
        }
        else
        {
            // An unstamped build loses to every real release, so landing here means the channel had
            // nothing at all rather than that this copy is up to date.
            Log.Info(
                _unstamped
                    ? $"[Updates] Nothing on the {Describe(Channel())} channel to compare against yet."
                    : $"[Updates] v{Current()} is the newest {Describe(Channel())}. Nothing to do.");
        }
    }

    private static async Task CheckAsync()
    {
        // The tick waits after the handler returns rather than on a timer, so it cannot re-enter.
        // This guard is for vmenu_checkupdates landing on top of a scheduled run.
        if (_running)
        {
            return;
        }

        _running = true;

        try
        {
            var channel = Channel();

            if (channel == UpdateChannel.Off)
            {
                return;
            }

            var current = Current();

            var userAgent = UserAgent();

            var result = await GitHubSource.LatestAsync(channel, userAgent, TimeoutMs);

            // nuget.org is asked both when github.com could not be read and when it read but had
            // nothing, because every enhanced release starts life as a draft that an unauthenticated
            // caller cannot see at all.
            if (!result.Reached || result.Update is null)
            {
                var fallback = await NugetSource.LatestAsync(channel, userAgent, TimeoutMs);

                if (fallback.Reached)
                {
                    result = fallback;
                }
            }

            if (!result.Reached)
            {
                Warn();

                return;
            }

            _warned = false;

            if (result.Update is not { } found)
            {
                // The stable channel lands here every time until 1.0.0. Not a failure, never warned.
                Log.Debug($"[Updates] Nothing on the {Describe(channel)} channel to compare against yet.");

                return;
            }

            if (!found.Version.IsNewerThan(current))
            {
                Log.Debug($"[Updates] v{current} is the newest {Describe(channel)} ({found.Source}).");

                return;
            }

            // A later check finding the same version again is not news and must not re-notify.
            if (_available is { } already && !found.Version.IsNewerThan(already.Version))
            {
                return;
            }

            _available = found;

            Log.Info(
                _unstamped
                    ? $"[Updates] A newer vMenu Enhanced is out: v{found.Version}."
                    : $"[Updates] A newer vMenu Enhanced is out: v{found.Version}. You are on v{current}.");
            Log.Info($"[Updates] {found.Url}");

            Announce(found);
        }
        finally
        {
            _running = false;
        }
    }

    private static bool IsWanted() => Channel() != UpdateChannel.Off;

    /// <summary>This build's version, or <see cref="DevelopmentVersion"/> when it has none.</summary>
    // Read once. The manifest cannot change under a running resource.
    private static SemanticVersion Current()
    {
        if (_readCurrent)
        {
            return _current!;
        }

        _readCurrent = true;

        var resource = Native.GetCurrentResourceName();

        var text = Native.GetNumResourceMetadata(resource, VersionKey) == 0
            ? null
            : Native.GetResourceMetadata(resource, VersionKey, 0)?.Trim();

        var stamped = !string.IsNullOrEmpty(text)
            && !string.Equals(text, UnstampedVersion, StringComparison.Ordinal);

        if (stamped && SemanticVersion.TryParse(text, out _current) && _current is not null)
        {
            return _current;
        }

        // A build made on a developer's machine still says versiongoeshere, because CI is what
        // replaces it. Counting that as older than everything is what makes the check actually run
        // locally, which is the only way to exercise the rest of this without publishing a release.
        _unstamped = true;

        if (stamped)
        {
            Log.Debug($"[Updates] '{text}' in fxmanifest.lua is not a version this can compare, so this counts as an old build.");
        }

        SemanticVersion.TryParse(DevelopmentVersion, out _current);

        return _current!;
    }

    private static UpdateChannel Channel()
    {
        var raw = ServerConfig.Value(UpdateSettings.CheckMode);
        var value = raw.Trim();

        // An empty setr is an owner clearing the line rather than asking for something unknown, so it
        // takes the default in silence rather than earning a warning.
        if (value.Length == 0)
        {
            return UpdateChannel.Prerelease;
        }

        if (Is(value, "off") || Is(value, "none") || Is(value, "false") || Is(value, "0"))
        {
            return UpdateChannel.Off;
        }

        if (Is(value, "stable") || Is(value, "release"))
        {
            return UpdateChannel.Stable;
        }

        if (Is(value, "prerelease") || Is(value, "pre-release") || Is(value, "alpha") || Is(value, "beta")
            || Is(value, "true"))
        {
            return UpdateChannel.Prerelease;
        }

        Report(
            ref _reportedMode,
            raw,
            $"{UpdateSettings.CheckMode.Name} is set to '{raw}', which is not off, stable or prerelease. Using prerelease.");

        return UpdateChannel.Prerelease;

        static bool Is(string value, string name) => string.Equals(value, name, StringComparison.OrdinalIgnoreCase);
    }

    private static string Describe(UpdateChannel channel) =>
        channel == UpdateChannel.Stable ? "stable" : "prerelease";

    // The one line the owner asked for, and the only Log.Warning in this feature. Everything beneath
    // it, in both sources and the HTTP layer, logs at Debug, so a failed check is one line and not
    // one per source. Suppressed while it stays broken: four identical lines a day about a firewall
    // helps nobody.
    private static void Warn()
    {
        if (_warned)
        {
            return;
        }

        _warned = true;

        Log.Warning("[Updates] Could not check whether a newer vMenu Enhanced is out. It will try again on the next check.");
    }

    /// <summary>The client asks once it has its permissions. This answers only if there is news.</summary>
    // Answered rather than pushed on join, matching every other sync on this side: the client knows
    // when it is ready and the server does not.
    private static void OnRequested([FromSource] Player source)
    {
        if (_available is not { } update)
        {
            return;
        }

        // Global.Everything passes this by the ancestor walk, so an owner does not need both aces.
        if (!ServerPermissions.IsPlayerAllowed(source, Global.Staff))
        {
            return;
        }

        if (!Told.Add(source.Handle))
        {
            return;
        }

        API.EmitClient(source.Handle, UpdateEvents.Available, update.Version.Text, update.Url);
    }

    /// <summary>For staff who were already online when a later check turned something up.</summary>
    private static void Announce(KnownUpdate update)
    {
        // Rebuilt rather than pruned. ConnectedPlayers.All asks DoesPlayerExist for every index it
        // walks, so the set this leaves behind has already had everybody who left dropped out of it.
        Told.Clear();

        var sent = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

            if (!ServerPermissions.IsPlayerAllowed(handle, Global.Staff))
            {
                continue;
            }

            Told.Add(player.ServerId);

            API.EmitClient(player.ServerId, UpdateEvents.Available, update.Version.Text, update.Url);

            sent++;
        }

        Log.Debug($"[Updates] Told {sent} staff member(s) about v{update.Version}.");
    }

    // Once per distinct value rather than once per check, so a typo in the config does not bury the
    // console.
    private static void Report(ref string? reported, string raw, string message)
    {
        if (string.Equals(reported, raw, StringComparison.Ordinal))
        {
            return;
        }

        reported = raw;

        Log.Warning($"[Updates] {message}");
    }
}

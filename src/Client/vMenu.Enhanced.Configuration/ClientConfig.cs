using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Configuration;

/// <summary>Client side of the configuration module.</summary>
// No handshake with the server, unlike permissions: settings are replicated convars, so an owner
// changing one reaches every client through the runtime.
public static class ClientConfig
{
    private const string DumpCommand = "vmenu_config";

    private static readonly ConfigStore Store = new(Native.GetConvar, ForwardLog);

    /// <summary>Call once, before the menus are built, so the first gate pass reads real values.</summary>
    public static void Initialize()
    {
        Store.Prime();

        ApplyDebugMode();

        Store.Watch([Debugging.Client], ApplyDebugMode);

        Listen(Store.Tracked);

        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
    }

    /// <summary>Starts watching convars that are not settings, so listeners can be added for them.</summary>
    /// <remarks>See <see cref="ConfigStore.Track" /> for why these are not in the catalog.</remarks>
    public static void Track(IReadOnlyList<string> convars) => Listen(Store.Track(convars));

    /// <summary>Calls <paramref name="handler"/> whenever any of these settings changes, and nothing else.</summary>
    public static void AddEventListenerFor(IReadOnlyList<Setting> settings, Action handler) =>
        Store.Watch(settings, handler);

    /// <summary>The same, for convars registered through <see cref="Track" /> rather than catalogued settings.</summary>
    public static void AddEventListenerFor(IReadOnlyList<string> convars, Action handler) =>
        Store.Watch(convars, handler);

    /// <summary>
    /// Calls <paramref name="handler"/> whenever any setting other than these changes. For a
    /// subscriber that really does react to almost anything, where naming the settings it reads
    /// would mean one added later silently never reaching it.
    /// </summary>
    public static void AddEventListenerExcept(IReadOnlyList<Setting> settings, Action handler) =>
        Store.WatchExcept(settings, handler);

    public static void RemoveEventListenerFor(IReadOnlyList<Setting> settings, Action handler) =>
        Store.Unwatch(settings, handler);

    public static void RemoveEventListenerFor(IReadOnlyList<string> convars, Action handler) =>
        Store.Unwatch(convars, handler);

    public static void RemoveEventListenerExcept(Action handler) => Store.UnwatchExcept(handler);

    /// <summary>Prints what this client currently reads for every setting.</summary>
    public static void Dump()
    {
        Log.Info("[Config] Current values:");

        foreach (var line in Store.Describe())
        {
            Log.Info("[Config]   " + line);
        }
    }

    public static bool? GetBool(string convar) => Store.GetBool(convar);

    public static int? GetInt(string convar) => Store.GetInt(convar);

    public static float? GetFloat(string convar) => Store.GetFloat(convar);

    public static string? GetString(string convar) => Store.GetString(convar);

    public static bool? GetBool(BoolSetting setting) => Store.Get(setting);

    public static int? GetInt(IntSetting setting) => Store.Get(setting);

    public static float? GetFloat(FloatSetting setting) => Store.Get(setting);

    public static string? GetString(StringSetting setting) => Store.Get(setting);

    public static bool Value(BoolSetting setting) => Store.Value(setting);

    public static int Value(IntSetting setting) => Store.Value(setting);

    public static float Value(FloatSetting setting) => Store.Value(setting);

    public static string Value(StringSetting setting) => Store.Value(setting);

    // One listener per convar rather than a wildcard filter, which if it matched nothing would look
    // like the module quietly not working.
    private static void Listen(IReadOnlyList<string> convars)
    {
        foreach (var convar in convars)
        {
            NativeFixer.AddConvarChangeListener(convar, OnConvarChanged);
        }
    }

    private static void OnConvarChanged(string convar, object? reserved) => Store.NotifyChanged(convar);

    private static void ApplyDebugMode() => Log.SetDebug(Store.Value(Debugging.Client));

    private static void ForwardLog(ConfigLog level, string message)
    {
        switch (level)
        {
            case ConfigLog.Error:
                Log.Error($"[Config] {message}");
                break;
            case ConfigLog.Warn:
                Log.Warning($"[Config] {message}");
                break;
            case ConfigLog.Info:
                Log.Info($"[Config] {message}");
                break;
            default:
                Log.Debug($"[Config] {message}");
                break;
        }
    }
}

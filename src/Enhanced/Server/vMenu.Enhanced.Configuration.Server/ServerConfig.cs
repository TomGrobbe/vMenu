using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Configuration.Server;

public static class ServerConfig
{
    private const string DumpCommand = "vmenu_config";

    private static readonly ConfigStore Store = new(Native.GetConvar, Write, includeServerOnly: true);

    // Call once, first, from the server entry point.
    public static void Initialize()
    {
        Store.Prime();

        ApplyDebugMode();

        Store.Watch([Debugging.Server], ApplyDebugMode);

        // One listener per convar rather than a single wildcard filter: an exact name cannot be matched
        // wrongly, and a filter that silently matches nothing would look like the module quietly not working.
        foreach (var convar in Store.Tracked)
        {
            NativeFixer.AddConvarChangeListener(convar, OnConvarChanged);
        }

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, DebugCommands.Gate(Dump));
    }

    // Calls the handler whenever any of these settings changes, and nothing else.
    public static void AddEventListenerFor(IReadOnlyList<Setting> settings, Action handler) =>
        Store.Watch(settings, handler);

    // For a subscriber that really does react to almost anything, where naming the settings it reads
    // would mean one added later silently never reaching it.
    public static void AddEventListenerExcept(IReadOnlyList<Setting> settings, Action handler) =>
        Store.WatchExcept(settings, handler);

    public static void RemoveEventListenerFor(IReadOnlyList<Setting> settings, Action handler) =>
        Store.Unwatch(settings, handler);

    public static void RemoveEventListenerExcept(Action handler) => Store.UnwatchExcept(handler);

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

    private static void OnConvarChanged(string convar, object? reserved) => Store.NotifyChanged(convar);

    private static void ApplyDebugMode() => Log.SetDebug(Store.Value(Debugging.Server));

    private static void Write(ConfigLog level, string message)
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

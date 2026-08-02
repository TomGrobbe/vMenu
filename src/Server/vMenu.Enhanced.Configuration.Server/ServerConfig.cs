using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Configuration;

namespace vMenu.Enhanced.Configuration.Server;

public static class ServerConfig
{
    private const string DumpCommand = "vmenu_config";

    private static readonly ConfigStore Store = new(Native.GetConvar, Write);

    public static event Action? Changed
    {
        add => Store.Changed += value;
        remove => Store.Changed -= value;
    }

    /// <summary>Call once, first, from the server entry point.</summary>
    public static void Initialize()
    {
        Store.Prime();

        // One listener per convar rather than a single wildcard filter: an exact name cannot be
        // matched wrongly, and a filter that silently matches nothing would look like the whole
        // module quietly not working.
        foreach (var convar in Store.Tracked)
        {
            NativeFixer.AddConvarChangeListener(convar, OnConvarChanged);
        }

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, new Action(Dump));
    }

    /// <summary>Prints what the server currently reads for every setting.</summary>
    public static void Dump()
    {
        API.Log.Info("[Config] Current values:");

        foreach (var line in Store.Describe())
        {
            API.Log.Info("[Config]   " + line);
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

    private static void Write(ConfigLog level, string message)
    {
        switch (level)
        {
            case ConfigLog.Error:
                API.Log.Error($"[Config] {message}");
                break;
            case ConfigLog.Warn:
                API.Log.Warn($"[Config] {message}");
                break;
            case ConfigLog.Info:
                API.Log.Info($"[Config] {message}");
                break;
            default:
                API.Log.Debug($"[Config] {message}");
                break;
        }
    }
}

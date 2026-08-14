namespace vMenu.Enhanced.Data.Configuration;

public enum ConfigLog
{
    Debug,
    Info,
    Warn,
    Error,
}

/// <summary>
/// Reads settings from convars and reports when one changes. Both sides own an instance of this and
/// supply their own convar native and logger, so the parsing, caching and change detection behave
/// identically on the client and the server.
/// </summary>
public sealed class ConfigStore(Func<string, string, string> readConvar, Action<ConfigLog, string> log)
{
    /// <remarks>
    /// A sentinel default is what makes an unset convar distinguishable from one set to an empty
    /// value. The typed <c>GetConvarBool/Int/Float</c> natives cannot do this: they collapse both
    /// cases into whatever default they were handed.
    /// </remarks>
    private const string Unset = "vMenu.Enhanced.Unset";

    private readonly Dictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _reported = new(StringComparer.OrdinalIgnoreCase);

    private string[] _tracked = [];

    /// <summary>Raised once per actual change, whichever setting moved.</summary>
    public event Action? Changed;

    /// <summary>The convars worth listening to, known only after <see cref="Prime"/>.</summary>
    public IReadOnlyList<string> Tracked => _tracked;

    public void Prime()
    {
        _cache.Clear();
        _reported.Clear();

        foreach (var setting in ConfigCatalog.All)
        {
            if (!ConfigPath.IsValidName(setting.Name))
            {
                log(ConfigLog.Error, $"'{setting.Name}' is not a usable convar name, so it can never be set.");
                continue;
            }

            _cache[setting.Name] = Raw(setting.Name);
        }

        _tracked = [.. _cache.Keys];

        log(ConfigLog.Debug, $"Tracking {_tracked.Length} setting(s).");
    }

    /// <summary>Re-reads <paramref name="convar"/> and raises <see cref="Changed"/> if it moved.</summary>
    public void NotifyChanged(string convar)
    {
        if (!_cache.TryGetValue(convar, out var previous))
        {
            return;
        }

        var current = Raw(convar);

        if (string.Equals(previous, current, StringComparison.Ordinal))
        {
            return;
        }

        _cache[convar] = current;
        _reported.Remove(convar);

        log(ConfigLog.Info, $"{convar} changed to {Quote(current)}.");

        Changed?.Invoke();
    }

    /// <summary>One line per setting, for the <c>vmenu_config</c> command.</summary>
    public IEnumerable<string> Describe()
    {
        foreach (var setting in ConfigCatalog.All)
        {
            var raw = Raw(setting.Name);
            var tracked = _cache.ContainsKey(setting.Name) ? string.Empty : "  [not tracked]";

            yield return $"{setting.Name} = {Quote(raw)} (default {setting.DefaultText}){tracked}";
        }
    }

    public string? GetString(string convar) => ConvarValue.Normalise(Raw(convar));

    public bool? GetBool(string convar) => Typed(convar, ConvarValue.ParseBool, "true or false");

    public int? GetInt(string convar) => Typed(convar, ConvarValue.ParseInt, "a whole number");

    public float? GetFloat(string convar) => Typed(convar, ConvarValue.ParseFloat, "a number");

    public string? Get(StringSetting setting) => GetString(setting.Name);

    public bool? Get(BoolSetting setting) => GetBool(setting.Name);

    public int? Get(IntSetting setting) => GetInt(setting.Name);

    public float? Get(FloatSetting setting) => GetFloat(setting.Name);

    public string Value(StringSetting setting) => GetString(setting.Name) ?? setting.Default;

    public bool Value(BoolSetting setting) => GetBool(setting.Name) ?? setting.Default;

    public int Value(IntSetting setting) => GetInt(setting.Name) ?? setting.Default;

    public float Value(FloatSetting setting) => GetFloat(setting.Name) ?? setting.Default;

    private T? Typed<T>(string convar, Func<string?, T?> parse, string expected) where T : struct
    {
        var raw = Raw(convar);
        var parsed = parse(raw);

        if (parsed is null && ConvarValue.Normalise(raw) is { } text && _reported.Add(convar))
        {
            log(ConfigLog.Warn, $"{convar} is set to '{text}', which is not {expected}. Treating it as unset.");
        }

        return parsed;
    }

    private string? Raw(string convar)
    {
        var value = readConvar(convar, Unset);

        return string.Equals(value, Unset, StringComparison.Ordinal) ? null : value;
    }

    private static string Quote(string? value) => value is null ? "unset" : $"'{value}'";
}

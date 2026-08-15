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
    private const string Unset = "vMenu.Enhanced.Unset";

    private readonly Dictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _reported = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, List<Action>> _watchers = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<ExceptWatcher> _exceptWatchers = [];

    private readonly HashSet<string> _quiet = new(StringComparer.OrdinalIgnoreCase);

    private string[] _tracked = [];

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

    /// <summary>
    /// Starts watching convars that are not settings, so <see cref="Watch(IReadOnlyList{string}, Action)"/>
    /// reaches them too.
    /// </summary>
    /// <returns>The names actually taken on, which is what the caller registers a native listener for.</returns>
    /// <remarks>
    /// For the convars the server publishes world state through. Those cannot live in
    /// <see cref="ConfigCatalog"/>, which is owner authored configuration and drives the generated
    /// example file, so listing them there would invite editing state the server overwrites. They
    /// are kept quiet as well: the clock moves once a second, so announcing every change would bury
    /// the console, and <see cref="WatchExcept"/> never sees them so a subscriber that meant "any
    /// setting an owner might change" is not woken once a second by the clock.
    /// </remarks>
    public IReadOnlyList<string> Track(IReadOnlyList<string> convars)
    {
        var taken = new List<string>();

        foreach (var convar in convars)
        {
            if (!ConfigPath.IsValidName(convar))
            {
                log(ConfigLog.Error, $"'{convar}' is not a usable convar name, so it can never be set.");
                continue;
            }

            if (!_cache.ContainsKey(convar))
            {
                _cache[convar] = Raw(convar);
            }

            _quiet.Add(convar);
            taken.Add(convar);
        }

        return taken;
    }

    /// <summary>Calls <paramref name="handler"/> whenever any of <paramref name="convars"/> changes.</summary>
    public void Watch(IReadOnlyList<string> convars, Action handler)
    {
        foreach (var convar in convars)
        {
            // Staying silent would read as the listener working and the convar never moving, which
            // is the one failure this module goes out of its way not to have.
            if (!_cache.ContainsKey(convar))
            {
                log(ConfigLog.Error, $"'{convar}' is not being watched, so a listener on it can never fire.");
                continue;
            }

            if (!_watchers.TryGetValue(convar, out var handlers))
            {
                handlers = [];
                _watchers[convar] = handlers;
            }

            handlers.Add(handler);
        }
    }

    public void Watch(IReadOnlyList<Setting> settings, Action handler) => Watch(Names(settings), handler);

    /// <summary>Calls <paramref name="handler"/> whenever any setting other than these changes.</summary>
    public void WatchExcept(IReadOnlyList<Setting> settings, Action handler)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var setting in settings)
        {
            excluded.Add(setting.Name);
        }

        _exceptWatchers.Add(new ExceptWatcher(excluded, handler));
    }

    public void Unwatch(IReadOnlyList<string> convars, Action handler)
    {
        foreach (var convar in convars)
        {
            if (_watchers.TryGetValue(convar, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public void Unwatch(IReadOnlyList<Setting> settings, Action handler) => Unwatch(Names(settings), handler);

    public void UnwatchExcept(Action handler) => _exceptWatchers.RemoveAll(watcher => watcher.Handler == handler);

    /// <summary>Re-reads <paramref name="convar"/> and tells its listeners, if it moved.</summary>
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

        var quiet = _quiet.Contains(convar);

        if (!quiet)
        {
            log(ConfigLog.Info, $"{convar} changed to {Quote(current)}.");
        }

        // The listeners that named this convar go first, so one that caches the value has already
        // refreshed it by the time a broad subscriber reads it back.
        if (_watchers.TryGetValue(convar, out var handlers))
        {
            foreach (var handler in handlers)
            {
                Invoke(convar, handler);
            }
        }

        if (quiet)
        {
            return;
        }

        foreach (var watcher in _exceptWatchers)
        {
            if (!watcher.Excluded.Contains(convar))
            {
                Invoke(convar, watcher.Handler);
            }
        }
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

    private static string[] Names(IReadOnlyList<Setting> settings)
    {
        var names = new string[settings.Count];

        for (var index = 0; index < settings.Count; index++)
        {
            names[index] = settings[index].Name;
        }

        return names;
    }

    // A dispatch pass has to reach every listener, so one throwing must not take the rest with it.
    private void Invoke(string convar, Action handler)
    {
        try
        {
            handler();
        }
        catch (Exception exception)
        {
            log(ConfigLog.Error, $"A listener for {convar} threw: {exception}");
        }
    }

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

    private sealed class ExceptWatcher(HashSet<string> excluded, Action handler)
    {
        public HashSet<string> Excluded { get; } = excluded;

        public Action Handler { get; } = handler;
    }
}

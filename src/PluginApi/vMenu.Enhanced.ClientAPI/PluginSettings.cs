using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>One of your plugin's convar settings. The full convar name is
/// <c>vMenu.Enhanced.Plugins.&lt;Id&gt;.&lt;Name&gt;</c>, set by the server owner with <c>setr</c>,
/// and readable here because replicated convars reach every resource.</summary>
public abstract class PluginSetting
{
    private protected PluginSetting(string name, string fullName)
    {
        Name = name;
        FullName = fullName;
    }

    /// <summary>The short name you declared.</summary>
    public string Name { get; }

    /// <summary>The composed convar name a server owner sets.</summary>
    public string FullName { get; }

    private protected string Raw(string fallback) => Native.GetConvar(FullName, fallback);
}

public sealed class PluginBoolSetting : PluginSetting
{
    internal PluginBoolSetting(string name, string fullName, bool defaultValue)
        : base(name, fullName) => Default = defaultValue;

    public bool Default { get; }

    public bool Value => string.Equals(Raw(Default ? "true" : "false"), "true", StringComparison.OrdinalIgnoreCase);
}

public sealed class PluginIntSetting : PluginSetting
{
    internal PluginIntSetting(string name, string fullName, int defaultValue)
        : base(name, fullName) => Default = defaultValue;

    public int Default { get; }

    public int Value =>
        int.TryParse(Raw(string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : Default;
}

public sealed class PluginFloatSetting : PluginSetting
{
    internal PluginFloatSetting(string name, string fullName, float defaultValue)
        : base(name, fullName) => Default = defaultValue;

    public float Default { get; }

    public float Value =>
        float.TryParse(Raw(string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : Default;
}

public sealed class PluginStringSetting : PluginSetting
{
    internal PluginStringSetting(string name, string fullName, string defaultValue)
        : base(name, fullName) => Default = defaultValue;

    public string Default { get; }

    public string Value
    {
        get
        {
            var raw = Raw(Default);

            return raw.Length == 0 ? Default : raw;
        }
    }
}

/// <summary>Your plugin's settings. Declaring one here lets your menu gate on it and lets vMenu
/// track it for live refresh. Declare the same settings in your server script through the ServerAPI
/// too, so they appear in the template vMenu writes for the server owner.</summary>
public sealed class PluginSettings
{
    private readonly string _prefix;

    private readonly List<SettingNode> _nodes = new();

    internal PluginSettings(string pluginId) => _prefix = "vMenu.Enhanced.Plugins." + pluginId + ".";

    internal IReadOnlyList<SettingNode> Nodes => _nodes;

    public PluginBoolSetting Bool(string name, bool defaultValue, string description)
    {
        Declare(name, SettingTypes.Bool, defaultValue ? "true" : "false", description);

        return new PluginBoolSetting(name, _prefix + name, defaultValue);
    }

    public PluginIntSetting Int(string name, int defaultValue, string description)
    {
        Declare(name, SettingTypes.Int, defaultValue.ToString(CultureInfo.InvariantCulture), description);

        return new PluginIntSetting(name, _prefix + name, defaultValue);
    }

    public PluginFloatSetting Float(string name, float defaultValue, string description)
    {
        Declare(name, SettingTypes.Float, defaultValue.ToString("0.0###", CultureInfo.InvariantCulture), description);

        return new PluginFloatSetting(name, _prefix + name, defaultValue);
    }

    public PluginStringSetting String(string name, string defaultValue, string description)
    {
        Declare(name, SettingTypes.String, defaultValue, description);

        return new PluginStringSetting(name, _prefix + name, defaultValue);
    }

    private void Declare(string name, string type, string defaultText, string description) =>
        _nodes.Add(new SettingNode
        {
            Name = name,
            Type = type,
            Default = defaultText,
            Description = description,
        });
}

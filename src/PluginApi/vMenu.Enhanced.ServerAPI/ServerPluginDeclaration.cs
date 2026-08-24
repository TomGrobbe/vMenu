using System.Globalization;

using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ServerAPI;

/// <summary>Everything a plugin's server side declares with vMenu: a display name for the generated
/// example files, the permission names it wants under its own scope, and the convar settings vMenu
/// should describe to server owners. Names are short: vMenu composes the full
/// <c>vMenu.Enhanced.Plugins.&lt;Id&gt;.&lt;Name&gt;</c> form from the resource name.</summary>
public sealed class ServerPluginDeclaration
{
    private readonly List<PermissionDeclaration> _permissions = new();

    private readonly List<SettingNode> _settings = new();

    public ServerPluginDeclaration(string displayName)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public ServerPluginDeclaration AddPermission(string name, string description, bool staffOnly = false)
    {
        _permissions.Add(new PermissionDeclaration { Name = name, Description = description, StaffOnly = staffOnly });

        return this;
    }

    public ServerPluginDeclaration AddBoolSetting(string name, bool defaultValue, string description)
    {
        _settings.Add(new SettingNode
        {
            Name = name,
            Type = SettingTypes.Bool,
            Default = defaultValue ? "true" : "false",
            Description = description,
        });

        return this;
    }

    public ServerPluginDeclaration AddIntSetting(string name, int defaultValue, string description)
    {
        _settings.Add(new SettingNode
        {
            Name = name,
            Type = SettingTypes.Int,
            Default = defaultValue.ToString(CultureInfo.InvariantCulture),
            Description = description,
        });

        return this;
    }

    public ServerPluginDeclaration AddFloatSetting(string name, float defaultValue, string description)
    {
        _settings.Add(new SettingNode
        {
            Name = name,
            Type = SettingTypes.Float,
            Default = defaultValue.ToString("0.0###", CultureInfo.InvariantCulture),
            Description = description,
        });

        return this;
    }

    public ServerPluginDeclaration AddStringSetting(string name, string defaultValue, string description)
    {
        _settings.Add(new SettingNode
        {
            Name = name,
            Type = SettingTypes.String,
            Default = defaultValue,
            Description = description,
        });

        return this;
    }

    internal ServerRegisterRequest ToRequest() => new()
    {
        ProtocolVersion = PluginProtocol.Version,
        DisplayName = DisplayName,
        Permissions = new List<PermissionDeclaration>(_permissions),
        Settings = new List<SettingNode>(_settings),
    };
}

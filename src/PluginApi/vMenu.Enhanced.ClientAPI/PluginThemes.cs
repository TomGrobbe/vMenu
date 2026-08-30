using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>One of vMenu's looks. Hand the <see cref="Id"/> back to
/// <see cref="PluginThemes.Set"/> to put the menus in it, and show the <see cref="Name"/> to the
/// player.</summary>
public sealed class PluginTheme
{
    internal PluginTheme(string id, string name, bool isCurrent)
    {
        Id = id;
        Name = name;
        IsCurrent = isCurrent;
    }

    public string Id { get; }

    public string Name { get; }

    /// <summary>Whether this is the theme on screen right now.</summary>
    public bool IsCurrent { get; }
}

/// <summary>The look of vMenu's menus, as seen by this player only. vMenu hands the list over right
/// after the plugin registers and again on every change, so read <see cref="Available"/> from
/// <see cref="Changed"/> rather than straight after connecting.
///
/// A theme set here lasts as long as the client runs. Nothing is written to disk, so reconnecting or
/// restarting the game puts the server's own setting back.</summary>
public sealed class PluginThemes
{
    private static readonly PluginTheme[] None = Array.Empty<PluginTheme>();

    private readonly VMenuPlugin _plugin;

    internal PluginThemes(VMenuPlugin plugin) => _plugin = plugin;

    /// <summary>Every theme vMenu offers, in the order it lists them. Empty until vMenu has sent them,
    /// and empty for good against a vMenu too old to know about themes.</summary>
    public IReadOnlyList<PluginTheme> Available { get; private set; } = None;

    /// <summary>The id of the theme on screen, null until vMenu has said what it is.</summary>
    public string? CurrentId { get; private set; }

    /// <summary>The id the server's own setting asks for, which is where <see cref="Reset"/> goes.</summary>
    public string? ConfiguredId { get; private set; }

    /// <summary>Whether a plugin is overriding the server's setting for this player right now.</summary>
    public bool IsOverridden { get; private set; }

    /// <summary>Raised whenever the list or the theme on screen changed, including the first time vMenu
    /// sends them and when somebody else changed the theme.</summary>
    public event Action? Changed;

    /// <summary>Puts vMenu's menus in a theme for this player. The id comes from
    /// <see cref="Available"/>. An id vMenu does not know is ignored, with a line in the client log.</summary>
    public void Set(string themeId) => Send(themeId);

    /// <summary>Drops the override and goes back to the theme the server's setting asks for.</summary>
    public void Reset() => Send(null);

    internal void Handle(string json)
    {
        if (!PluginJson.TryDeserialize<ThemeList>(json, out var list) || list is null)
        {
            return;
        }

        var themes = new PluginTheme[list.Themes.Count];

        for (var index = 0; index < themes.Length; index++)
        {
            var theme = list.Themes[index];

            themes[index] = new PluginTheme(
                theme.Id,
                theme.Name,
                string.Equals(theme.Id, list.Current, StringComparison.OrdinalIgnoreCase));
        }

        Available = themes;
        CurrentId = list.Current;
        ConfiguredId = list.Configured;
        IsOverridden = list.Overridden;

        try
        {
            Changed?.Invoke();
        }
        catch (Exception exception)
        {
            SharedAPI.Log.Error($"[{_plugin.Resource}] A Themes.Changed handler threw: {exception}");
        }
    }

    private void Send(string? themeId)
    {
        if (!_plugin.IsConnected)
        {
            return;
        }

        PluginEmit.Local(PluginEvents.SetTheme, PluginJson.Serialize(new ThemeRequest { Theme = themeId }));
    }
}

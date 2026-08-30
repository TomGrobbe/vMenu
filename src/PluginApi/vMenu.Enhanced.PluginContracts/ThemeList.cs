namespace vMenu.Enhanced.PluginContracts;

/// <summary>Every theme vMenu knows about plus which one is on screen. vMenu sends this to a plugin
/// right after it registers and again whenever the theme changes, whoever changed it.</summary>
public class ThemeList
{
    public List<ThemeInfo> Themes { get; set; } = new();

    public string? Current { get; set; }

    public string? Configured { get; set; }

    public bool Overridden { get; set; }
}

namespace vMenu.Enhanced.PluginContracts;

public class RegisterThemesRequest
{
    public List<ThemeSource> Themes { get; set; } = new();
}

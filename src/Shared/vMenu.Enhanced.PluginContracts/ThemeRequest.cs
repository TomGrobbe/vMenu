namespace vMenu.Enhanced.PluginContracts;

/// <summary>Asks vMenu to put its menus in a theme for this player, for as long as the client runs.
/// Nothing is saved, so a reconnect starts from the server's own setting again. An empty theme drops
/// the override and goes back to that setting.</summary>
public class ThemeRequest
{
    public string? Theme { get; set; }
}

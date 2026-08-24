namespace vMenu.Enhanced.PluginContracts;

/// <summary>Opt in event subscriptions on a menu or item node. Interaction callbacks such as a
/// button press always fire; these are the chatty ones a plugin must ask for, so an idle plugin
/// costs nothing while the player scrolls.</summary>
public static class NodeEvents
{
    // Menu level.
    public const string Opened = "opened";
    public const string Closed = "closed";
    public const string IndexChanged = "indexChanged";

    // Item level.
    public const string Highlighted = "highlighted";
}

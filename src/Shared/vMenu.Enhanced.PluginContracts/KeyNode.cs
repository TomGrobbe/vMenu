namespace vMenu.Enhanced.PluginContracts;

/// <summary>A rebindable key that works while its menu is open, with an instructional button at the
/// bottom of the screen. Ids are plugin chosen and unique within the plugin. Keep them stable: the
/// id names the binding in the player's key settings, so a changed id loses their custom key.</summary>
public class KeyNode
{
    /// <summary>Letters, digits and underscores only.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>The instructional button's label. Empty shows no button, the key still works.</summary>
    public TextRef? Text { get; set; }

    /// <summary>What the player reads in the game's key settings. Falls back to <see cref="Text"/>.</summary>
    public TextRef? Description { get; set; }

    /// <summary>A keyboard key name as the game's key mapper knows it, for example "X" or "F5".</summary>
    public string DefaultKey { get; set; } = string.Empty;

    /// <summary>Optional controller button, for example "RUP_INDEX". Without one the key has no
    /// instructional button while the player uses a controller.</summary>
    public string? DefaultButton { get; set; }

    /// <summary>A game control index to suppress while the menu is open, for a default key the game
    /// already uses for something else.</summary>
    public int? ShadowedControl { get; set; }

    public GateNode? Gate { get; set; }

    /// <summary>Null means enabled. A disabled key does nothing and shows no button.</summary>
    public bool? Enabled { get; set; }
}

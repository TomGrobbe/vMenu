namespace vMenu.Enhanced.ClientAPI;

/// <summary>How hard an open request may push. vMenu's own menus always count as <see cref="None"/>.</summary>
public enum MenuPriority
{
    /// <summary>Only opens when no vMenu or plugin menu is open.</summary>
    None,

    /// <summary>Replaces vMenu's menus and other menus at low priority.</summary>
    Low,

    /// <summary>Replaces anything at low or medium priority.</summary>
    Medium,

    /// <summary>Replaces anything that is not locked. Only use this in extreme situations, prevents vMenu or other
    /// plugins with a lower priority from opening any menus while yours is open. Use responsibly!</summary>
    High,
}

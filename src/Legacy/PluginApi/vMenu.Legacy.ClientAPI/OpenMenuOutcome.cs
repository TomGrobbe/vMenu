namespace vMenu.Enhanced.ClientAPI;

/// <summary>What happened to a request to open a menu.</summary>
public enum OpenMenuOutcome
{
    /// <summary>The menu is on screen.</summary>
    Opened,

    /// <summary>A menu with a higher priority is open, or any menu is open for a request at <see cref="MenuPriority.None"/>.</summary>
    BlockedByPriority,

    /// <summary>Another menu asked not to be closed.</summary>
    BlockedByLock,

    /// <summary>vMenu does not know this menu, for example an empty root menu, which has no row yet.</summary>
    UnknownMenu,

    /// <summary>The player is not allowed into this menu, for example because a gate on the row leading to it fails.</summary>
    NotAllowed,

    /// <summary>A later open or close in the same batch took its place.</summary>
    Superseded,

    /// <summary>The vMenu this plugin talks to predates open priorities. Nothing was opened, use
    /// <see cref="PluginMenu.Open"/> there instead.</summary>
    NotSupported,

    /// <summary>The plugin is not connected to vMenu, or vMenu stopped before it answered.</summary>
    NotConnected,
}

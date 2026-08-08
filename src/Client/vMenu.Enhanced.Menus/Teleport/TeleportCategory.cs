namespace vMenu.Enhanced.Menus.Teleport;

/// <summary>A group of teleport locations, as the server read them out of the config file.</summary>
// Classes rather than records: the generated equality routes through EqualityComparer<T>.Default,
// which the sandbox refuses to load.
public sealed class TeleportCategory
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<TeleportLocation> Locations { get; set; } = [];
}

public sealed class TeleportLocation
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TeleportPosition Position { get; set; } = new();

    /// <summary>Which way to face on arrival. Null keeps whichever way the player already faces.</summary>
    public float? Heading { get; set; }
}

public sealed class TeleportPosition
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}

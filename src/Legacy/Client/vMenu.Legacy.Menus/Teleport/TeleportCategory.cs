namespace vMenu.Enhanced.Menus.Teleport;

// A group of teleport locations, as the server read them out of the config file. Classes rather than
// records: generated equality routes through EqualityComparer<T>.Default, which the sandbox refuses.
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

    // Which way to face on arrival. Null keeps whichever way the player already faces.
    public float? Heading { get; set; }
}

public sealed class TeleportPosition
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public sealed class SavedAutoPilotPoint
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}

public sealed class SavedAutoPilotPointEntry(SavedAutoPilotPoint point, int storedVersion)
{
    public SavedAutoPilotPoint Point { get; } = point;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > SavedAutoPilotPoint.SchemaVersion;
}

public sealed class SavedAutoPilotPath
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<AutoPilotPathPoint> Points { get; set; } = [];
}

public sealed class AutoPilotPathPoint
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}

public sealed class SavedAutoPilotPathEntry(SavedAutoPilotPath path, int storedVersion)
{
    public SavedAutoPilotPath Path { get; } = path;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > SavedAutoPilotPath.SchemaVersion;
}

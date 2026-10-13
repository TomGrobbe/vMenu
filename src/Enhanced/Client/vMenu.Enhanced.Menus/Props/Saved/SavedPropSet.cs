namespace vMenu.Enhanced.Menus.Props.Saved;

// A class, not a record: the sandbox refuses the generated EqualityComparer<T>.Default.
public sealed class SavedPropSet
{
    public const int SchemaVersion = 1;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<SavedProp> Props { get; set; } = [];
}

public sealed class SavedProp
{
    public string Model { get; set; } = string.Empty;

    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }

    public float Heading { get; set; }

    public bool Networked { get; set; }

    public bool Frozen { get; set; } = true;
}

public sealed class SavedPropSetEntry(SavedPropSet set, int storedVersion)
{
    public SavedPropSet Set { get; } = set;

    public int StoredVersion { get; } = storedVersion;

    public bool IsFromNewerBuild => StoredVersion > SavedPropSet.SchemaVersion;
}

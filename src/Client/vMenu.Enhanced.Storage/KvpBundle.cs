namespace vMenu.Enhanced.Storage;

/// <summary>Everything vMenu keeps on this client, in a shape another client can read back.</summary>
public sealed class KvpBundle
{
    /// <summary>Tells a vMenu code apart from any other text somebody pastes in.</summary>
    public const string FormatName = "vmenu.enhanced.kvp";

    public const int CurrentVersion = 1;

    // Both left empty rather than defaulted, so JSON that never named itself a vMenu code cannot
    // pass the check by inheriting the answer.
    public string Format { get; init; } = string.Empty;

    public int Version { get; init; }

    /// <summary>Written by the page on the way out, and only ever read back as text.</summary>
    // A string rather than a date on purpose. Nothing here needs to do arithmetic on it, and the
    // client sandbox cannot construct a DateTime at all.
    public string CreatedAt { get; init; } = string.Empty;

    public List<KvpBundleEntry> Entries { get; init; } = [];
}

public sealed class KvpBundleEntry
{
    public string Key { get; init; } = string.Empty;

    // The envelope as a string rather than a nested object: reading one back as an object needs
    // LINQ to JSON, which the sandbox refuses. Compression pays back what the escaping costs.
    public string Raw { get; init; } = string.Empty;
}

public enum KvpImportMode
{
    /// <summary>Writes what is in the code and leaves anything else alone.</summary>
    Merge,

    /// <summary>Clears everything vMenu owns first, so the result is exactly what was exported.</summary>
    Replace,
}

public sealed class KvpImportResult
{
    public int Applied { get; set; }

    public int Deleted { get; set; }

    /// <summary>Left alone because what is stored here was written by a newer vMenu.</summary>
    public int SkippedNewer { get; set; }

    public int SkippedMalformed { get; set; }

    public int SkippedDuplicate { get; set; }

    public int Skipped => SkippedNewer + SkippedMalformed + SkippedDuplicate;
}

/// <summary>What a code would hold, for a menu that wants to say so before making one.</summary>
public sealed class KvpInventory
{
    public int Vehicles { get; set; }

    public int Peds { get; set; }

    public int Characters { get; set; }

    public int Loadouts { get; set; }

    public int Settings { get; set; }

    public int Total { get; set; }
}

namespace vMenu.Enhanced.Storage;

public sealed class KvpBundle
{
    // Tells a vMenu code apart from any other text somebody pastes in.
    public const string FormatName = "vmenu.enhanced.kvp";

    public const int CurrentVersion = 1;

    // Both left empty rather than defaulted, so JSON that never named itself a vMenu code cannot pass
    // the check by inheriting the answer.
    public string Format { get; init; } = string.Empty;

    public int Version { get; init; }

    // Written by the page on the way out, and only ever read back as text. A string rather than a date:
    // nothing here needs arithmetic on it, and the client sandbox cannot construct a DateTime at all.
    public string CreatedAt { get; init; } = string.Empty;

    public List<KvpBundleEntry> Entries { get; init; } = [];
}

public sealed class KvpBundleEntry
{
    public string Key { get; init; } = string.Empty;

    // The envelope as a string rather than a nested object: reading one back as an object needs LINQ to
    // JSON, which the sandbox refuses. Compression pays back what the escaping costs.
    public string Raw { get; init; } = string.Empty;
}

public enum KvpImportMode
{
    // Writes what is in the code and leaves anything else alone.
    Merge,

    // Clears everything vMenu owns first, so the result is exactly what was exported.
    Replace,
}

public sealed class KvpImportResult
{
    public int Applied { get; set; }

    public int Deleted { get; set; }

    // Left alone because what is stored here was written by a newer vMenu.
    public int SkippedNewer { get; set; }

    public int SkippedMalformed { get; set; }

    public int SkippedDuplicate { get; set; }

    public int Skipped => SkippedNewer + SkippedMalformed + SkippedDuplicate;
}

public sealed class KvpInventory
{
    public int Vehicles { get; set; }

    public int Peds { get; set; }

    public int Characters { get; set; }

    public int Loadouts { get; set; }

    public int Settings { get; set; }

    public int Total { get; set; }
}

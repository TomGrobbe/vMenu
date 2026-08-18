namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// One mutation of a live plugin tree. A single flat shape instead of one class per
/// operation keeps the JSON free of type discriminator machinery, unused fields simply
/// stay null. Which fields matter depends on <see cref="Op"/>, one of the
/// <see cref="UpdateOps"/> names. Unknown operations are logged and skipped by vMenu.
/// </summary>
public class UpdateOp
{
    public string Op { get; set; } = string.Empty;

    /// <summary>Target item id for item operations.</summary>
    public string? ItemId { get; set; }

    /// <summary>Target menu id for menu operations, addItems and clearMenu.</summary>
    public string? MenuId { get; set; }

    public TextRef? TextValue { get; set; }

    public string? LeftIcon { get; set; }

    public string? RightIcon { get; set; }

    public bool? Flag { get; set; }

    public int? Index { get; set; }

    public string? Value { get; set; }

    public List<TextRef>? Options { get; set; }

    public GateNode? Gate { get; set; }

    public List<ItemNode>? Items { get; set; }

    public List<string>? ItemIds { get; set; }

    /// <summary>Language code for mergeTranslations.</summary>
    public string? Language { get; set; }

    public Dictionary<string, string>? Entries { get; set; }
}

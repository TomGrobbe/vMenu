namespace vMenu.Enhanced.PluginContracts;

/// <summary>One mutation of a live plugin tree. A single flat shape instead of one class per
/// operation keeps the JSON free of type discriminator machinery: unused fields simply stay null.
/// Which fields matter depends on <see cref="Op"/>, one of the <see cref="UpdateOps"/> names.
/// Unknown operations are logged and skipped by vMenu.</summary>
public class UpdateOp
{
    public string Op { get; set; } = string.Empty;

    /// <summary>Target item id for item operations.</summary>
    public string? ItemId { get; set; }

    /// <summary>Target menu id for menu operations, addItems and clearMenu.</summary>
    public string? MenuId { get; set; }

    /// <summary>Target key id for key operations.</summary>
    public string? KeyId { get; set; }

    public TextRef? TextValue { get; set; }

    public string? LeftIcon { get; set; }

    public string? RightIcon { get; set; }

    public bool? Flag { get; set; }

    public int? Index { get; set; }

    public string? Value { get; set; }

    public List<TextRef>? Options { get; set; }

    public GateNode? Gate { get; set; }

    public List<ItemNode>? Items { get; set; }

    /// <summary>The menu for addStandaloneMenu.</summary>
    public MenuNode? Menu { get; set; }

    public List<string>? ItemIds { get; set; }

    public string? BeforeItemId { get; set; }

    public List<string>? Events { get; set; }

    public List<KeyNode>? Keys { get; set; }

    /// <summary>Language code for mergeTranslations.</summary>
    public string? Language { get; set; }

    public Dictionary<string, string>? Entries { get; set; }

    /// <summary>One of the <see cref="MenuPriorities"/> values for openMenu. Absent means low, which is how a plugin
    /// from before priorities existed is treated.</summary>
    public string? Priority { get; set; }

    /// <summary>For openMenu at high priority: nothing may close or replace the menu until the plugin leaves it.</summary>
    public bool? DoNotClose { get; set; }

    /// <summary>For openMenu: when set, vMenu answers with an <see cref="OpenMenuResult"/> carrying it.</summary>
    public int? RequestId { get; set; }
}

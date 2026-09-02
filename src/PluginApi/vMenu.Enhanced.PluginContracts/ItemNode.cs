namespace vMenu.Enhanced.PluginContracts;

/// <summary>One menu item in a plugin's tree. Which optional fields matter depends on
/// <see cref="Type"/>, one of the <see cref="EntryTypes"/> values. Ids are chosen by the plugin and
/// must be unique within it: they are how update operations and callbacks refer to the item.</summary>
public class ItemNode
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = EntryTypes.Button;

    public TextRef? Text { get; set; }

    public TextRef? Description { get; set; }

    public TextRef? Label { get; set; }

    /// <summary>Description shown instead of <see cref="Description"/> while the gate locks the item.</summary>
    public TextRef? LockedDescription { get; set; }

    public GateNode? Gate { get; set; }

    /// <summary>"lock" or "hide". What a failing gate does, lock when omitted.</summary>
    public string? Behaviour { get; set; }

    /// <summary>Icon names from the MenuFramework icon set, for example "LOCK".</summary>
    public string? LeftIcon { get; set; }

    public string? RightIcon { get; set; }

    /// <summary>Opt in <see cref="NodeEvents"/> subscriptions for this item.</summary>
    public List<string>? Events { get; set; }

    /// <summary>Whether the row is shown at all. Null means visible.</summary>
    public bool? Visible { get; set; }

    /// <summary>Whether the row can be used. Null means enabled. A disabled row is greyed out.</summary>
    public bool? Enabled { get; set; }

    /// <summary>Ask vMenu to log use of this row to the server owner's webhook. Only honoured for an id
    /// the plugin's server half declared with <c>AddLoggedItem</c>.</summary>
    public bool? Log { get; set; }

    // Checkbox.
    public bool? Checked { get; set; }

    /// <summary>"tick" or "cross".</summary>
    public string? CheckStyle { get; set; }

    // List and confirm list.
    public List<TextRef>? Options { get; set; }

    public int? SelectedIndex { get; set; }

    // Slider.
    public int? Min { get; set; }

    public int? Max { get; set; }

    public int? Position { get; set; }

    public bool? ShowDivider { get; set; }

    // Dynamic list.
    public string? Value { get; set; }

    // Confirm entries.
    public TextRef? ConfirmationDescription { get; set; }

    // Submenu.
    public MenuNode? Menu { get; set; }
}

namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// One interaction flowing from vMenu back to the owning plugin, multiplexed over a
/// single per resource event. <see cref="Type"/> is one of the
/// <see cref="CallbackTypes"/> values and decides which optional fields are filled.
/// </summary>
public class PluginCallback
{
    public string Type { get; set; } = string.Empty;

    public string? MenuId { get; set; }

    public string? ItemId { get; set; }

    // Checkbox.
    public bool? Checked { get; set; }

    // List and index changes.
    public int? OldIndex { get; set; }

    public int? NewIndex { get; set; }

    public int? SelectedIndex { get; set; }

    // Slider.
    public int? OldPosition { get; set; }

    public int? NewPosition { get; set; }

    public int? Position { get; set; }

    // Dynamic list.
    public string? Value { get; set; }

    public string? CurrentValue { get; set; }

    public bool? Left { get; set; }

    // Player actions.
    public int? TargetServerId { get; set; }

    public string? TargetName { get; set; }
}

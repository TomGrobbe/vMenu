namespace vMenu.Enhanced.PluginContracts;

/// <summary>
/// A group of update operations applied in order, with a single menu refresh at the end,
/// so many small changes cost one repaint.
/// </summary>
public class UpdateBatch
{
    public List<UpdateOp> Ops { get; set; } = new();
}

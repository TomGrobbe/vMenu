namespace vMenu.Enhanced.Permissions.Server;

// The two parent kinds are kept apart on purpose. Structural edges follow the dotted name, so the
// client can reconstruct them from a string alone, and the emit walk descends only those. Extra
// parents cannot be expressed by the name, so they are read when evaluating a node but never traversed.
public sealed class PermissionNode
{
    public required string Name { get; init; }

    // The config file this permission came from, or null when vMenu declares it itself.
    public string? Source { get; init; }

    // Only steers which principal the generated example suggests, never a live check.
    public bool IsStaffOnly { get; internal set; }

    // Whether being staff only carries down to everything nested underneath.
    public bool CascadesStaffOnly { get; init; } = true;

    public required IReadOnlyList<string> ExtraParents { get; init; }

    public PermissionNode? StructuralParent { get; internal set; }

    public List<PermissionNode> StructuralChildren { get; } = [];
}

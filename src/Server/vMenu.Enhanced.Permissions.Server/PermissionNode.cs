namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// A permission in the tree the server walks when computing a player's set.
/// </summary>
/// <remarks>
/// The two parent kinds are kept apart on purpose. Structural edges follow the dotted name, so the
/// client can reconstruct them from a string alone, and the emit walk descends only those. Extra
/// parents cannot be expressed by the name, so they are read when evaluating a node but never
/// traversed.
/// </remarks>
public sealed class PermissionNode
{
    public required string Name { get; init; }

    /// <summary>The config file this permission came from, or null when vMenu declares it itself.</summary>
    public string? Source { get; init; }

    /// <summary>Only steers which principal the generated example suggests, never a live check.</summary>
    public bool IsStaffOnly { get; internal set; }

    public required IReadOnlyList<string> ExtraParents { get; init; }

    public PermissionNode? StructuralParent { get; internal set; }

    public List<PermissionNode> StructuralChildren { get; } = [];
}

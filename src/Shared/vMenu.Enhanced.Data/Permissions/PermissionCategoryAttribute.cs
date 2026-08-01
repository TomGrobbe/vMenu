namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Marks a static class as a permission category. The server registers every
/// <see langword="public"/> <see langword="const"/> <see cref="string"/> on it that is deeper than
/// the category prefix; anything shallower is treated as a helper and ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PermissionCategoryAttribute : Attribute
{
    /// <summary>
    /// When null, derived from the type: its namespace with <c>.Data.Permissions</c> removed, plus
    /// the type name. Set it when the type name does not match the path segment.
    /// </summary>
    public string? Prefix { get; init; }

    /// <summary>
    /// Permissions outside this container that also grant everything in it. Must be written as
    /// <c>new[] { ... }</c>; a collection expression is not a constant and will not compile here.
    /// </summary>
    public string[] AdditionalParents { get; init; } = [];
}

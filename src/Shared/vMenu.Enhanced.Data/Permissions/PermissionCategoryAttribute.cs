namespace vMenu.Enhanced.Data.Permissions;

// Marks a static class as a permission category. The server registers every public const string on
// it that is deeper than the category prefix; anything shallower is a helper and is ignored.
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PermissionCategoryAttribute : Attribute
{
    // When null, derived from the type: its namespace with .Data.Permissions removed, plus the type
    // name. Set it when the type name does not match the path segment.
    public string? Prefix { get; init; }

    // Permissions outside this container that also grant everything in it. Must be written as
    // new[] { ... }; a collection expression is not a constant and will not compile here.
    public string[] AdditionalParents { get; init; } = [];
}

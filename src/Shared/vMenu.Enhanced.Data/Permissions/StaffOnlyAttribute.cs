namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Marks a permission, or a whole category, as one the generated example should hand to staff
/// rather than to everybody. It only picks the principal written into
/// <c>permissions.cfg.example</c>; nothing is enforced by it, since a server owner is free to edit
/// their copy however they like.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class StaffOnlyAttribute : Attribute;

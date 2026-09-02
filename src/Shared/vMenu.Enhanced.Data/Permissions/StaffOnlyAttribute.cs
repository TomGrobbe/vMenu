namespace vMenu.Enhanced.Data.Permissions;

// Marks a permission, or a whole category, as one the generated example should hand to staff rather
// than to everybody. It only picks the principal written into permissions.cfg.example; nothing is
// enforced by it, since a server owner is free to edit their copy however they like.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class StaffOnlyAttribute : Attribute
{
    // A container that should stay with staff while what sits under it is still suggested to everybody
    // one line at a time, so granting the lot stays a deliberate choice without hiding the parts.
    public bool Cascades { get; init; } = true;
}

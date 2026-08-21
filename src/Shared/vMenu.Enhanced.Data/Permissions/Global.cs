namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Permissions that sit directly under <see cref="PermissionPath.Root"/>.
/// </summary>
[PermissionCategory(Prefix = PermissionPath.Root)]
public static class Global
{
    /// <summary>Grants every permission in vMenu, including any registered at runtime.</summary>
    [StaffOnly]
    public const string Everything = "vMenu.Enhanced.Everything";

    /// <summary>
    /// Marks somebody as a member of your staff team, which is what turns on the parts of vMenu that
    /// are only there to help staff read the world around them.
    /// </summary>
    [StaffOnly]
    public const string Staff = "vMenu.Enhanced.Staff";

    public const string NoClip = "vMenu.Enhanced.NoClip";
}

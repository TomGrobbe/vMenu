namespace vMenu.Enhanced.Data.Permissions;

// Permissions that sit directly under PermissionPath.Root.
[PermissionCategory(Prefix = PermissionPath.Root)]
public static class Global
{
    // Grants every permission in vMenu, including any registered at runtime.
    [StaffOnly]
    public const string Everything = "vMenu.Enhanced.Everything";

    // Marks somebody as a member of your staff team, which is what turns on the parts of vMenu that are
    // only there to help staff read the world around them.
    [StaffOnly]
    public const string Staff = "vMenu.Enhanced.Staff";

    public const string NoClip = "vMenu.Enhanced.NoClip";
}

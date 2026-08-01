namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Permissions that sit directly under <see cref="PermissionPath.Root"/>.
/// </summary>
[PermissionCategory(Prefix = PermissionPath.Root)]
public static class Global
{
    /// <summary>Grants every permission in vMenu, including any registered at runtime.</summary>
    public const string Everything = "vMenu.Enhanced.Everything";
}

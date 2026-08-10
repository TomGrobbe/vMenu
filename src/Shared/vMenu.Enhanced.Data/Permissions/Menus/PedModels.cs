namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the ped models menu. Per category permissions live one container deeper in
/// <see cref="PedModelCategories"/>.
/// </summary>
[PermissionCategory]
public static class PedModels
{
    public const string All = "vMenu.Enhanced.Menus.PedModels.All";

    public const string Menu = "vMenu.Enhanced.Menus.PedModels.Menu";

    public const string SpawnByName = "vMenu.Enhanced.Menus.PedModels.SpawnByName";
}

using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PropSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.PropSpawner;

namespace vMenu.Enhanced.Menus.Props;

public static class PropSpawnOptions
{
    public const int MinDistance = 1;

    public const int MaxDistance = 50;

    public static bool Networked => UserDefaults.PropSpawnerNetworked.Value && CanNetwork;

    public static bool CanNetwork => ClientPermissions.IsAllowed(PropSpawnerPermissions.Networked);

    public static bool Frozen => UserDefaults.PropSpawnerFrozen.Value;

    public static bool SnapToGround => UserDefaults.PropSpawnerSnapGround.Value;

    public static int Distance
    {
        get => Math.Clamp(UserDefaults.PropSpawnerDistance.Value, MinDistance, MaxDistance);

        set => UserDefaults.PropSpawnerDistance.Value = Math.Clamp(value, MinDistance, MaxDistance);
    }

    public static void SetNetworked(bool networked)
    {
        if (networked && !CanNetwork)
        {
            return;
        }

        UserDefaults.PropSpawnerNetworked.Value = networked;
    }

    public static void SetFrozen(bool frozen) => UserDefaults.PropSpawnerFrozen.Value = frozen;

    public static void SetSnapToGround(bool snap) => UserDefaults.PropSpawnerSnapGround.Value = snap;
}

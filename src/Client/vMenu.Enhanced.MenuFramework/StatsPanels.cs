namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Values for MenuAPI's vehicle stats panel, each 0..1.
/// </summary>
/// <remarks>
/// Modelled here because the panel is a MenuAPI feature, not vMenu domain logic. Doing so keeps
/// <c>MenuItem.ItemData</c> free: it is <see langword="dynamic"/>, and every access to it emits a
/// DLR call site that needs <c>Microsoft.CSharp.dll</c> — which the resource manifest does not ship.
/// <para>
/// The upgrade overlay is a second value of this same type rather than a property, because a struct
/// cannot contain itself. See <c>MenuEntry.VehicleUpgradeStats</c>.
/// </para>
/// </remarks>
public readonly record struct VehicleStats(float TopSpeed, float Acceleration, float Braking, float Traction)
{
    public static VehicleStats None => default;
}

/// <summary>Values for MenuAPI's weapon stats panel, each 0..1.</summary>
public readonly record struct WeaponStats(float Damage, float FireRate, float Accuracy, float Range)
{
    public static WeaponStats None => default;
}

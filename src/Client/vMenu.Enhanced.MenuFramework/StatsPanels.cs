namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Values for MenuAPI's vehicle stats panel, each 0..1.
/// </summary>
// Modelled here to keep MenuItem.ItemData free. It is dynamic, and every access emits a DLR call
// site needing Microsoft.CSharp.dll, which the manifest does not ship. The upgrade overlay is a
// second value of this type rather than a property, because a struct cannot contain itself.
public readonly record struct VehicleStats(float TopSpeed, float Acceleration, float Braking, float Traction)
{
    public static VehicleStats None => default;
}

/// <summary>Values for MenuAPI's weapon stats panel, each 0..1.</summary>
public readonly record struct WeaponStats(float Damage, float FireRate, float Accuracy, float Range)
{
    public static WeaponStats None => default;
}

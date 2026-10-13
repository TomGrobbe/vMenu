namespace vMenu.Enhanced.Menus.Vehicles;

// How a vehicle came to be the one the player means.
public enum VehicleTargetKind
{
    None,

    Driving,

    Passenger,

    InFront,
}

// The vehicle an option should act on. The kind is reported rather than judged, because seat rules
// differ per option.
public readonly record struct VehicleTarget(int Handle, VehicleTargetKind Kind)
{
    public static VehicleTarget None { get; } = new(0, VehicleTargetKind.None);

    public bool Found => Kind is not VehicleTargetKind.None;
}

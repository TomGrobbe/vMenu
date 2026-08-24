using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

public static class VehicleModSlots
{
    // Every slot, so a caller can ask the game about all of them.
    public static IReadOnlyList<VehicleModSlot> All { get; } =
        [.. Enum.GetValues<VehicleModSlot>()];

    // Slots that are switched on or off rather than chosen from a list. The game reports zero options
    // for these, so they never turn up in the generic upgrade list.
    public static IReadOnlyList<VehicleModSlot> Toggles { get; } =
    [
        VehicleModSlot.Nitrous,
        VehicleModSlot.Turbo,
        VehicleModSlot.Subwoofer,
        VehicleModSlot.TyreSmoke,
        VehicleModSlot.Hydraulics,
        VehicleModSlot.XenonLights,
    ];

    // Slots the wheels and tyres menu owns, so the generic upgrade list leaves them alone.
    public static IReadOnlyList<VehicleModSlot> WheelSlots { get; } =
    [
        VehicleModSlot.Wheels,
        VehicleModSlot.RearWheels,
    ];

    public static bool IsToggle(VehicleModSlot slot) => slot
        is VehicleModSlot.Nitrous
        or VehicleModSlot.Turbo
        or VehicleModSlot.Subwoofer
        or VehicleModSlot.TyreSmoke
        or VehicleModSlot.Hydraulics
        or VehicleModSlot.XenonLights;

    public static bool IsWheelSlot(VehicleModSlot slot) =>
        slot is VehicleModSlot.Wheels or VehicleModSlot.RearWheels;

    // Call SetVehicleModKit first, or the game answers zero for everything.
    public static List<VehicleModSlot> Available(int handle, bool includeWheelSlots)
    {
        var slots = new List<VehicleModSlot>();

        foreach (var slot in All)
        {
            if (IsToggle(slot))
            {
                continue;
            }

            if (!includeWheelSlots && IsWheelSlot(slot))
            {
                continue;
            }

            if (Native.GetNumVehicleMods(handle, (int)slot) > 0)
            {
                slots.Add(slot);
            }
        }

        return slots;
    }

    // The name to show when the game has no label for a slot, which happens on add-on vehicles whose
    // authors did not fill one in. Deliberately not translated: it names a technical slot, and a player
    // who sees one is better served by the game's own word than by a guess in their language.
    public static string TechnicalName(VehicleModSlot slot) => slot switch
    {
        VehicleModSlot.FrontBumper => "Front Bumper",
        VehicleModSlot.RearBumper => "Rear Bumper",
        VehicleModSlot.SideSkirt => "Side Skirt",
        VehicleModSlot.RollCage => "Roll Cage",
        VehicleModSlot.LeftFender => "Left Fender",
        VehicleModSlot.RightFender => "Right Fender",
        VehicleModSlot.XenonLights => "Xenon Lights",
        VehicleModSlot.RearWheels => "Rear Wheels",
        VehicleModSlot.PlateHolder => "Plate Holder",
        VehicleModSlot.VanityPlate => "Vanity Plate",
        VehicleModSlot.SteeringWheel => "Steering Wheel",
        VehicleModSlot.ShiftKnob => "Shift Knob",
        VehicleModSlot.EngineBay1 => "Engine Bay 1",
        VehicleModSlot.EngineBay2 => "Engine Bay 2",
        VehicleModSlot.EngineBay3 => "Engine Bay 3",
        VehicleModSlot.Chassis2 => "Chassis 2",
        VehicleModSlot.Chassis3 => "Chassis 3",
        VehicleModSlot.Chassis4 => "Chassis 4",
        VehicleModSlot.Chassis5 => "Chassis 5",
        VehicleModSlot.LeftDoor => "Left Door",
        VehicleModSlot.RightDoor => "Right Door",
        VehicleModSlot.LiveryMod => "Livery",
        VehicleModSlot.TyreSmoke => "Tyre Smoke",
        _ => slot.ToString(),
    };
}

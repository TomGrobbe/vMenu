namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// The game's modification slots, in the order the natives number them. Written out in full rather
// than taken from the CitizenFX wrapper, which does not name every slot. Which slots a particular
// vehicle actually offers is a question for GetNumVehicleMods, not for this list.
public enum VehicleModSlot
{
    Spoiler = 0,
    FrontBumper = 1,
    RearBumper = 2,
    SideSkirt = 3,
    Exhaust = 4,
    RollCage = 5,
    Grille = 6,
    Hood = 7,
    LeftFender = 8,
    RightFender = 9,
    Roof = 10,
    Engine = 11,
    Brakes = 12,
    Transmission = 13,
    Horn = 14,
    Suspension = 15,
    Armour = 16,

    // Seventeen through twenty two are the game's MOD_TOGGLE_ block: switched on or off, never picked
    // from a list.
    Nitrous = 17,
    Turbo = 18,
    Subwoofer = 19,
    TyreSmoke = 20,
    Hydraulics = 21,
    XenonLights = 22,

    // The rims. On a bike this is the front wheel only.
    Wheels = 23,

    // A bike's rear wheel. On anything else the game reuses this slot for hydraulics.
    RearWheels = 24,

    PlateHolder = 25,
    VanityPlate = 26,
    Interior1 = 27,
    Interior2 = 28,
    Interior3 = 29,
    Interior4 = 30,
    Interior5 = 31,
    Seats = 32,
    SteeringWheel = 33,
    ShiftKnob = 34,
    Plaque = 35,
    Speakers = 36,
    Trunk = 37,
    Hydro = 38,
    EngineBay1 = 39,
    EngineBay2 = 40,
    EngineBay3 = 41,
    Chassis2 = 42,
    Chassis3 = 43,
    Chassis4 = 44,
    Chassis5 = 45,
    LeftDoor = 46,
    RightDoor = 47,

    // Liveries that came in with the mod kit, separate from the vehicle's own liveries.
    LiveryMod = 48,

    Lightbar = 49,
}

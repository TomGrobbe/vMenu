namespace vMenu.Enhanced.Data.VehicleData;

public static class AircraftWeaponEvents
{
    public const string ConfigRequest = "vMenu.Enhanced:AircraftWeapons:Config:Request";

    public const string ConfigSet = "vMenu.Enhanced:AircraftWeapons:Config:Set";

    // Sent to the network owner, the only client the game lets change these.
    public const string Apply = "vMenu.Enhanced:AircraftWeapons:Apply";

    public const string OpenBombBay = "bombbay:open";

    public const string CloseBombBay = "bombbay:close";

    public const string AllowLockOn = "lockon:allow";

    public const string BlockLockOn = "lockon:block";

    public static bool IsKnown(string effect) =>
        effect is OpenBombBay or CloseBombBay or AllowLockOn or BlockLockOn;
}

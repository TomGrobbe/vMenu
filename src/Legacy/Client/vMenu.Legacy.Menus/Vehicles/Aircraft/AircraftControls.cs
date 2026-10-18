namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class AircraftControls
{
    public const int Group = 0;

    public static bool Muted { get; set; }

    public const int BombBay = 355;

    public const int Countermeasure = 356;

    public const int Drop = 114;

    public const int CinematicCamera = 80;

    public const int Headlight = 74;

    public const int LookLeftRight = 1;

    public const int LookUpDown = 2;

    // blocked controls while bombing, so a drop key cannot also fire the guns.
    public static readonly int[] BlockedWhileBombing = [99, 100, 68, 69, 70, 114, 331, 91, 92];
}

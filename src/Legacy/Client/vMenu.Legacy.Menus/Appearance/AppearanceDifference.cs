namespace vMenu.Enhanced.Menus.Appearance;

// Restoring writes every stored setting onto the vehicle or ped and then reads the whole thing back
// out of the game to see what actually stuck. Whatever came back different is recorded as one of
// these, usually because the save was made on a server with a DLC pack this one does not have. The
// menus only count them, so spawning can say "restored, but four things could not be applied";
// vmenu_vehicle_diff and vmenu_ped_diff print them in full.
public sealed class AppearanceDifference(string field, string expected, string actual)
{
    // Named for a person to read rather than as a slot number.
    public string Field { get; } = field;

    public string Expected { get; } = expected;

    public string Actual { get; } = actual;

    public override string ToString() => $"{Field}: expected {Expected}, got {Actual}";
}

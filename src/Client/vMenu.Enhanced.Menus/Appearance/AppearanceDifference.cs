namespace vMenu.Enhanced.Menus.Appearance;

/// <summary>
/// One piece of a saved vehicle or saved ped that did not survive being restored.
/// </summary>
/// <remarks>
/// Restoring works by writing every stored setting onto the vehicle or ped and then reading the whole
/// thing back out of the game to see what actually stuck. Whatever came back different is recorded as
/// one of these. The usual cause is that the save was made on a server with a DLC pack this one does
/// not have, so the piece simply is not there to put on.
///
/// <para>
/// The menus only count them, so that spawning something can say "restored, but four things could not
/// be applied" instead of quietly handing back a vehicle or ped that is not the one that was saved.
/// The <c>vmenu_vehicle_diff</c> and <c>vmenu_ped_diff</c> console commands print them in full, which
/// is how you find out which four.
/// </para>
///
/// <para>
/// One reads like this: <c>Prop 0 (hats): expected drawable 58, texture 0, got nothing worn</c>.
/// </para>
///
/// <para>
/// Vehicles and peds both produce these, which is the only reason this sits in a namespace of its own
/// rather than next to one of them.
/// </para>
/// </remarks>
public sealed class AppearanceDifference(string field, string expected, string actual)
{
    /// <summary>What was being restored, named for a person to read rather than as a slot number.</summary>
    public string Field { get; } = field;

    /// <summary>What the save asked for.</summary>
    public string Expected { get; } = expected;

    /// <summary>What the game ended up with instead.</summary>
    public string Actual { get; } = actual;

    public override string ToString() => $"{Field}: expected {Expected}, got {Actual}";
}

using System.Runtime.InteropServices;

using CitizenFX.Base;

namespace vMenu.Enhanced.BrokenNatives;

/// <summary>
/// The buffer the game fills in for <c>Native.GetWeaponHudStats</c> and
/// <c>Native.GetWeaponComponentHudStats</c>, which write the same layout. A weapon's bars are
/// percentages, 0 to 100; a component's are the signed amount it adds to them.
/// </summary>
/// <remarks>
/// The runtime allocates a buffer of <em>Marshal.SizeOf</em> for the <see cref="INativeStruct" />
/// argument, hands the game that, and copies it back into this instance at the start of its next
/// tick. So the values are readable from the following frame onwards, not from the call itself.
/// </remarks>
// Each bar sits in its own eight byte slot, so the buffer is forty bytes rather than five. Declared
// as ulongs because Marshal.SizeOf is what the runtime allocates from, and only these offsets
// produce the size the game writes.
[StructLayout(LayoutKind.Sequential)]
public sealed class WeaponHudStatsData : INativeStruct
{
    // Set by the runtime's marshaller during its next tick, never from C#, which is the assignment
    // the compiler is looking for here. Not readonly: the JIT is allowed to fold a readonly field
    // the marshaller writes behind its back.
#pragma warning disable CS0649
#pragma warning disable IDE0044
    private ulong _damage;
    private ulong _speed;
    private ulong _capacity;
    private ulong _accuracy;
    private ulong _range;
#pragma warning restore IDE0044
#pragma warning restore CS0649

    // Read as int rather than byte because a component's numbers are signed: a suppressor costs
    // damage. Taking the low byte of one would turn -5 into 251.
    public int Damage => (int)_damage;

    public int Speed => (int)_speed;

    public int Capacity => (int)_capacity;

    public int Accuracy => (int)_accuracy;

    public int Range => (int)_range;
}

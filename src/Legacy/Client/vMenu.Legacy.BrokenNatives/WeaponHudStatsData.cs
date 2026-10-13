namespace vMenu.Enhanced.BrokenNatives;

public sealed class WeaponHudStatsData
{
    private ulong _damage;
    private ulong _speed;
    private ulong _capacity;
    private ulong _accuracy;
    private ulong _range;

    public int Damage => (int)_damage;

    public int Speed => (int)_speed;

    public int Capacity => (int)_capacity;

    public int Accuracy => (int)_accuracy;

    public int Range => (int)_range;

    public const int Slots = 5;

    internal void Load(ulong[] slots)
    {
        _damage = slots[0];
        _speed = slots[1];
        _capacity = slots[2];
        _accuracy = slots[3];
        _range = slots[4];
    }
}

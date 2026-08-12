namespace vMenu.Enhanced.Data.Weapons;

public sealed class WeaponCategory
{
    public string Name { get; set; } = string.Empty;

    public List<WeaponEntry> Weapons { get; set; } = [];
}

public sealed class WeaponEntry
{
    public string SpawnName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public sealed class WeaponComponentEntry
{
    public string SpawnName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

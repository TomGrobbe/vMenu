using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Weapons.Saved;

public sealed class WeaponLoadout
{
    public const int SchemaVersion = KvpStore.InitialVersion;

    public string Name { get; set; } = string.Empty;

    public List<SavedWeapon> Weapons { get; set; } = [];
}

public sealed class SavedWeapon
{
    // The spawn name rather than the hash, so a loadout can still say which weapon it meant after that
    // weapon has left the server, and so its category permission can be looked up.
    public string SpawnName { get; set; } = string.Empty;

    public int Ammo { get; set; }

    public int Tint { get; set; }

    public List<string> Components { get; set; } = [];
}

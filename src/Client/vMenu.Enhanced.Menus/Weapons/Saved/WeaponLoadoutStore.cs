using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Weapons.Saved;

// The player's own machine, not the server. Keyed on the resource name, so the same loadouts show up
// on every server running vMenu Enhanced, which is also why the version check matters: one of those
// servers may be older than the build that wrote a save.
public static class WeaponLoadoutStore
{
    public const string LoadoutPrefix = "vmenu_weaponloadout_";

    // The snapshot taken before a respawn. Deliberately not a suffix of the loadout prefix, so listing
    // one never finds the other.
    public const string PendingKey = "vmenu_pendingweaponloadout";

    public static List<WeaponLoadout> All()
    {
        var loadouts = new List<WeaponLoadout>();

        foreach (var key in KvpStore.Keys(LoadoutPrefix))
        {
            if (Read(key) is { } loadout)
            {
                loadouts.Add(loadout);
            }
        }

        loadouts.Sort(static (left, right) =>
            string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

        return loadouts;
    }

    public static WeaponLoadout? Load(string name) => Read(Key(name));

    public static bool Exists(string name) => Load(name) is not null;

    // replacing is the difference between "replace this one" and "save a new one".
    public static SaveOutcome Save(WeaponLoadout loadout, bool replacing)
    {
        var key = Key(loadout.Name);

        if (Read(key) is not null && !replacing)
        {
            return SaveOutcome.NameTaken;
        }

        return KvpStore.TryWrite(key, KvpValueType.Json, WeaponLoadout.SchemaVersion, loadout)
            ? SaveOutcome.Saved
            : SaveOutcome.Refused;
    }

    public static void Delete(string name)
    {
        KvpStore.Delete(Key(name));

        // Otherwise the respawn restore would keep pointing at a loadout that is no longer there.
        if (string.Equals(UserDefaults.WeaponLoadoutDefaultName.Value, name, StringComparison.Ordinal))
        {
            UserDefaults.WeaponLoadoutDefaultName.Value = string.Empty;
        }
    }

    // False when the new name is taken.
    public static bool Rename(WeaponLoadout loadout, string newName)
    {
        if (Exists(newName))
        {
            return false;
        }

        var oldName = loadout.Name;

        loadout.Name = newName;

        if (Save(loadout, replacing: false) is not SaveOutcome.Saved)
        {
            loadout.Name = oldName;

            return false;
        }

        var wasDefault = string.Equals(UserDefaults.WeaponLoadoutDefaultName.Value, oldName, StringComparison.Ordinal);

        KvpStore.Delete(Key(oldName));

        // Moved rather than cleared, so renaming the default loadout does not quietly unset it.
        if (wasDefault)
        {
            UserDefaults.WeaponLoadoutDefaultName.Value = newName;
        }

        return true;
    }

    public static bool Duplicate(WeaponLoadout loadout, string newName)
    {
        var copy = new WeaponLoadout
        {
            Name = newName,
            Weapons = [.. loadout.Weapons],
        };

        return Save(copy, replacing: false) is SaveOutcome.Saved;
    }

    public static bool IsDefault(string name) =>
        name.Length > 0 && string.Equals(UserDefaults.WeaponLoadoutDefaultName.Value, name, StringComparison.Ordinal);

    public static void SetDefault(string name) => UserDefaults.WeaponLoadoutDefaultName.Value = name;

    public static WeaponLoadout Capture(string name)
    {
        var loadout = new WeaponLoadout { Name = name };

        foreach (var (spawnName, _) in WeaponInventory.Listed())
        {
            var hash = API.Hash(spawnName);

            if (!WeaponInventory.Has(hash))
            {
                continue;
            }

            var components = new List<string>();

            foreach (var component in WeaponComponentProbe.For(hash))
            {
                if (WeaponInventory.HasComponent(hash, API.Hash(component.SpawnName)))
                {
                    components.Add(component.SpawnName);
                }
            }

            loadout.Weapons.Add(new SavedWeapon
            {
                SpawnName = spawnName,
                Ammo = WeaponInventory.Ammo(hash),
                Tint = WeaponInventory.Tint(hash),
                Components = components,
            });
        }

        return loadout;
    }

    public static void SavePending()
    {
        var loadout = Capture(string.Empty);

        KvpStore.TryWrite(PendingKey, KvpValueType.Json, WeaponLoadout.SchemaVersion, loadout);

        // Counted here as well as where it is handed back, so a restore that comes up short says which of
        // the two lost it: the snapshot taken off a ped that has just died, or the giving.
        Log.Debug(
            $"[Weapons] Put {loadout.Weapons.Count} weapon(s) aside for the respawn, "
            + $"{loadout.Weapons.Sum(weapon => weapon.Components.Count)} component(s) between them.");
    }

    public static WeaponLoadout? LoadPending() =>
        KvpStore.TryRead<WeaponLoadout>(PendingKey, KvpValueType.Json, WeaponLoadout.SchemaVersion, out var loadout, out _)
            ? loadout
            : null;

    public static void ClearPending() => KvpStore.Delete(PendingKey);

    private static string Key(string name) => LoadoutPrefix + name;

    // A save that will not read is skipped rather than taking the whole list with it. KvpStore already
    // logs the key it could not read, so nothing is lost silently.
    private static WeaponLoadout? Read(string key) =>
        KvpStore.TryRead<WeaponLoadout>(key, KvpValueType.Json, WeaponLoadout.SchemaVersion, out var loadout, out _)
            ? loadout
            : null;
}

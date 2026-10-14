using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Weapons;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus;

// Peds and weapons the owner listed but the game does not know. Checked again on every permission refresh,
// so an add-on that loads after vMenu unlocks without a restart.
public static class CatalogValidity
{
    // A big ped list is thousands of native calls, so it is spread over frames.
    private const int ChecksPerFrame = 50;

    private static HashSet<string> _invalidPeds = new(StringComparer.OrdinalIgnoreCase);

    private static HashSet<string> _invalidWeapons = new(StringComparer.OrdinalIgnoreCase);

    private static bool _running;

    private static bool _pending;

    public static bool IsValidPed(string model) => !_invalidPeds.Contains(model);

    public static bool IsValidWeapon(string spawnName) => !_invalidWeapons.Contains(spawnName);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Refresh;

        Refresh();
    }

    private static async void Refresh()
    {
        if (_running)
        {
            _pending = true;
            return;
        }

        _running = true;

        try
        {
            do
            {
                _pending = false;

                var peds = await CollectInvalidAsync(
                    [.. PedModelSync.Categories.SelectMany(category => category.Peds).Select(ped => ped.Model)],
                    PedSpawning.IsSpawnable);

                var weapons = await CollectInvalidAsync(
                    [.. WeaponSync.Categories.SelectMany(category => category.Weapons).Select(weapon => weapon.SpawnName)],
                    spawnName => Native.IsWeaponValid(API.Hash(spawnName)));

                var changed = !peds.SetEquals(_invalidPeds) || !weapons.SetEquals(_invalidWeapons);

                _invalidPeds = peds;
                _invalidWeapons = weapons;

                if (changed)
                {
                    MenuRegistry.RefreshAll();
                }
            }
            while (_pending);
        }
        catch (Exception exception)
        {
            Log.Error($"[Catalog] Checking the ped and weapon lists failed: {exception}");
        }
        finally
        {
            _running = false;
        }
    }

    private static async Task<HashSet<string>> CollectInvalidAsync(List<string> names, Func<string, bool> isValid)
    {
        var invalid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < names.Count; index++)
        {
            // Also yields before the first check, so no native runs inside the permission event itself.
            if (index % ChecksPerFrame == 0)
            {
                await API.Delay(0);
            }

            if (!isValid(names[index]))
            {
                invalid.Add(names[index]);
            }
        }

        return invalid;
    }
}

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Weapons;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// This client's copy of the weapons and components the server owner defined.
/// </summary>
public static class WeaponSync
{
    /// <summary>
    /// How long the menu waits for the list before giving up and building itself empty. Long enough
    /// to cover a slow answer, short enough that a server which never answers does not hang the
    /// whole menu tree behind it.
    /// </summary>
    private const int WaitTimeout = 10000;

    private const int RequestRetryDelay = 1000;

    private static readonly List<WeaponCategory> CachedCategories = [];

    private static readonly List<WeaponComponentEntry> CachedComponents = [];

    public static IReadOnlyList<WeaponCategory> Categories => CachedCategories;

    public static IReadOnlyList<WeaponComponentEntry> Components => CachedComponents;

    public static bool HasReceived { get; private set; }

    /// <summary>The listed weapon with this spawn name, or null when the owner never listed it.</summary>
    // Here rather than in the menu that browses the list, because the loadouts menu has to ask the
    // same question to know which category permission a saved weapon answers to.
    public static (string SpawnName, string Label, string Category)? Find(string spawnName)
    {
        foreach (var category in CachedCategories)
        {
            foreach (var weapon in category.Weapons)
            {
                if (string.Equals(weapon.SpawnName, spawnName, StringComparison.OrdinalIgnoreCase))
                {
                    return (weapon.SpawnName, weapon.Label, category.Name);
                }
            }
        }

        return null;
    }

    /// <summary>Call before building menus, so a list arriving during startup is not dropped.</summary>
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(WeaponEvents.Set, new Action<string, string>(OnReceived), false);

    /// <summary>Call once this client has its permissions.</summary>
    public static void Request() => API.EmitServer(WeaponEvents.Request);

    /// <summary>
    /// Waits for the first list to land. These decide the menu's own shape, so it cannot be built
    /// before they are here.
    /// </summary>
    public static async Task WaitForFirstAsync()
    {
        var waited = 0;

        while (!HasReceived && waited < WaitTimeout)
        {
            await API.Delay(RequestRetryDelay);

            waited += RequestRetryDelay;

            // Asked again rather than only waiting, so a request that went out before the server
            // resource had its handler up does not cost the player the whole menu.
            if (!HasReceived)
            {
                Request();
            }
        }

        if (!HasReceived)
        {
            API.Log.Error($"[Weapons] No weapons received after {WaitTimeout}ms, so the menu is being built empty.");
        }
    }

    private static void OnReceived(string categories, string components)
    {
        if (!ClientJson.TryDeserialize<List<WeaponCategory>>(categories, out var readCategories) || readCategories is null)
        {
            API.Log.Error("[Weapons] The weapons the server sent could not be read.");

            return;
        }

        // A component list that will not read costs the components, not the whole menu, so it is
        // reported and left empty rather than dropping the weapons with it.
        if (!ClientJson.TryDeserialize<List<WeaponComponentEntry>>(components, out var readComponents) || readComponents is null)
        {
            API.Log.Error("[Weapons] The weapon components the server sent could not be read, so no weapon offers any.");

            readComponents = [];
        }

        CachedCategories.Clear();
        CachedCategories.AddRange(readCategories);

        CachedComponents.Clear();
        CachedComponents.AddRange(readComponents);

        // The probe answers "which of these components does this weapon take", so a new list makes
        // every answer it already gave meaningless.
        WeaponComponentProbe.Forget();

        WeaponHashNames.Forget();

        HasReceived = true;

        API.Log.Debug($"[Weapons] Received {CachedCategories.Count} category/categories and {CachedComponents.Count} component(s).");
    }
}

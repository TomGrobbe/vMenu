using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Players;

public static class PedModelSync
{
    // Long enough to cover a slow answer, short enough that a server which never answers does not hang
    // the whole menu tree behind it.
    private const int WaitTimeout = 10000;

    private const int RequestRetryDelay = 1000;

    private static readonly List<PedModelCategory> Cached = [];

    public static IReadOnlyList<PedModelCategory> Categories => Cached;

    public static bool HasReceived { get; private set; }

    // Here rather than in the menu that browses the list, because the saved peds menu has to ask the
    // same question to know which category permission a saved ped answers to.
    public static (string Model, string Label, string Category)? Find(string model)
    {
        foreach (var category in Cached)
        {
            foreach (var ped in category.Peds)
            {
                if (string.Equals(ped.Model, model, StringComparison.OrdinalIgnoreCase))
                {
                    return (ped.Model, ped.Label, category.Name);
                }
            }
        }

        return null;
    }

    // Call before building menus, so a list arriving during startup is not dropped.
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(PedModelEvents.Set, new Action<string>(OnReceived), false);

    // Call once this client has its permissions.
    public static void Request() => API.EmitServer(PedModelEvents.Request);

    // Unlike the teleport locations these decide the menu's own shape, so it cannot be built before they
    // are here.
    public static async Task WaitForFirstAsync()
    {
        var waited = 0;

        while (!HasReceived && waited < WaitTimeout)
        {
            await API.Delay(RequestRetryDelay);

            waited += RequestRetryDelay;

            // Asked again rather than only waiting, so a request that went out before the server resource had
            // its handler up does not cost the player the whole menu.
            if (!HasReceived)
            {
                Request();
            }
        }

        if (!HasReceived)
        {
            Log.Error($"[PedModels] No ped models received after {WaitTimeout}ms, so the menu is being built empty.");
        }
    }

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<List<PedModelCategory>>(payload, out var read) || read is null)
        {
            Log.Error("[PedModels] The ped models the server sent could not be read.");

            return;
        }

        Cached.Clear();
        Cached.AddRange(read);

        HasReceived = true;

        Log.Debug($"[PedModels] Received {Cached.Count} category/categories.");
    }
}

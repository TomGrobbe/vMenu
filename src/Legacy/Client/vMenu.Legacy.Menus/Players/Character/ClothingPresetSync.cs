using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Clothing;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class ClothingPresetSync
{
    private static readonly List<ClothingPresetCategory> Cached = [];

    public static IReadOnlyList<ClothingPresetCategory> Categories => Cached;

    public static event Action? Changed;

    public static void RegisterEventHandlers() =>
        Script.EventHandlers[ClothingPresetEvents.Set] += new Action<string>(OnReceived);

    public static void Request() => BaseScript.TriggerServerEvent(ClothingPresetEvents.Request);

    public static ClothingPresetCategory? Find(string name)
    {
        foreach (var category in Cached)
        {
            if (string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return null;
    }

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<List<ClothingPresetCategory>>(payload, out var read) || read is null)
        {
            Log.Error($"[Presets] The outfits the server sent could not be read: {payload}");

            return;
        }

        Cached.Clear();
        Cached.AddRange(read);

        Log.Debug($"[Presets] Received {Cached.Count} category/categories.");

        Changed?.Invoke();
    }
}

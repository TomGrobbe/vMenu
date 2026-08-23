using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleExtraLabels
{
    private static readonly Dictionary<uint, Dictionary<int, string>> Cached = [];

    public static void RegisterEventHandlers() =>
        API.OnNetEvent(VehicleEvents.ExtraLabelsSet, new Action<string>(OnReceived), false);

    public static void Request() => API.EmitServer(VehicleEvents.ExtraLabelsRequest);

    public static string? For(uint model, int extra) =>
        Cached.TryGetValue(model, out var named) && named.TryGetValue(extra, out var label) ? label : null;

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<Dictionary<string, Dictionary<string, string>>>(payload, out var read)
            || read is null)
        {
            Log.Error($"[Vehicles] The extra names the server sent could not be read: {payload}");

            return;
        }

        Cached.Clear();

        foreach (var vehicle in read)
        {
            var named = new Dictionary<int, string>();

            foreach (var extra in vehicle.Value)
            {
                if (int.TryParse(extra.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                {
                    named[id] = extra.Value;
                }
            }

            if (named.Count > 0)
            {
                Cached[API.Hash(vehicle.Key)] = named;
            }
        }

        Log.Debug($"[Vehicles] Received extra names for {Cached.Count} vehicle model(s).");
    }
}

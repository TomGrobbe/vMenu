using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

public static class AircraftWeaponConfig
{
    private static readonly Dictionary<uint, AircraftWeaponOverride> Cached = [];

    public static event Action? Changed;

    public static void RegisterEventHandlers() =>
        API.OnNetEvent(AircraftWeaponEvents.ConfigSet, new Action<string>(OnReceived), false);

    public static void Request() => API.EmitServer(AircraftWeaponEvents.ConfigRequest);

    internal static AircraftWeaponOverride? For(uint model) => Cached.TryGetValue(model, out var entry) ? entry : null;

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<Dictionary<string, AircraftWeaponOverride>>(payload, out var read) || read is null)
        {
            Log.Error($"[Vehicles] The aircraft weapon offsets the server sent could not be read: {payload}");

            return;
        }

        Cached.Clear();

        foreach (var aircraft in read)
        {
            Cached[API.Hash(aircraft.Key)] = aircraft.Value;
        }

        Log.Debug($"[Vehicles] Received aircraft weapon offsets for {Cached.Count} model(s).");

        Changed?.Invoke();
    }
}

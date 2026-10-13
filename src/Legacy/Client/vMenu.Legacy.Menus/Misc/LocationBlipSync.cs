using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Misc;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Misc;

public static class LocationBlipSync
{
    public static LocationBlipFile File { get; private set; } = new();

    public static event Action? Changed;

    public static void RegisterEventHandlers() =>
        Script.EventHandlers[LocationBlipEvents.Set] += new Action<string>(OnReceived);

    public static void Request() => BaseScript.TriggerServerEvent(LocationBlipEvents.Request);

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<LocationBlipFile>(payload, out var read) || read is null)
        {
            Log.Error($"[Blips] The map blips the server sent could not be read: {payload}");

            return;
        }

        File = read;

        Log.Debug($"[Blips] Received {read.AlwaysOn.Count} always on and {read.Toggleable.Count} toggleable blip(s).");

        Changed?.Invoke();
    }
}

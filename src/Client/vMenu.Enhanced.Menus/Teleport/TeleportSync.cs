using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Teleport;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Teleport;

/// <summary>
/// This client's copy of the teleport locations, which the server keeps current.
/// </summary>
// Held for the whole session rather than fetched when the menu opens: a fetch per open landed its
// rows several frames after the menu was already drawn. Handlers are registered imperatively because
// attribute discovery only scans the assembly named as the client_script, and this one is a
// referenced assembly.
public static class TeleportSync
{
    private static readonly List<TeleportCategory> Cached = [];

    public static IReadOnlyList<TeleportCategory> Categories => Cached;

    /// <summary>Raised whenever the server sends a new list, including the first one.</summary>
    public static event Action? Changed;

    /// <summary>Call before building menus, so a list arriving during startup is not dropped.</summary>
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(TeleportEvents.Set, new Action<string>(OnReceived), false);

    /// <summary>Call once this client has its permissions, which decide whether it gets an answer.</summary>
    public static void Request() => API.EmitServer(TeleportEvents.Request);

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<List<TeleportCategory>>(payload, out var read) || read is null)
        {
            Log.Error($"[Teleport] The locations the server sent could not be read: {payload}");

            return;
        }

        Cached.Clear();
        Cached.AddRange(read);

        Log.Debug($"[Teleport] Received {Cached.Count} category/categories.");

        Changed?.Invoke();
    }
}

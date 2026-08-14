using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// This client's copy of the walking styles the server owner defined.
/// </summary>
/// <remarks>
/// Waited for before the menus are built, the same way the ped models are. The row that offers them
/// sits on a menu that owns child menus, and MenuAPI cannot take one of those back out, so that menu
/// cannot be rebuilt later to pick up a list that turned up late.
/// </remarks>
public static class WalkingStyleSync
{
    /// <summary>
    /// How long to wait for the list before building the row without it. Shorter than the ped models
    /// wait, because a menu with no walking styles in it still works.
    /// </summary>
    private const int WaitTimeout = 5000;

    private const int RequestRetryDelay = 1000;

    private static readonly List<WalkingStyle> Cached = [];

    public static IReadOnlyList<WalkingStyle> Styles => Cached;

    public static bool HasReceived { get; private set; }

    /// <summary>Waits for the first list to land, or gives up and leaves the row with only Normal.</summary>
    public static async Task WaitForFirstAsync()
    {
        var waited = 0;

        while (!HasReceived && waited < WaitTimeout)
        {
            await API.Delay(RequestRetryDelay);

            waited += RequestRetryDelay;

            // Asked again rather than only waiting, so a request that went out before the server
            // resource had its handler up does not cost the player the whole row.
            if (!HasReceived)
            {
                Request();
            }
        }
    }

    /// <summary>Call before building menus, so a list arriving during startup is not dropped.</summary>
    public static void RegisterEventHandlers() =>
        API.OnNetEvent(WalkingStyleEvents.Set, new Action<string>(OnReceived), false);

    /// <summary>Call once this client has its permissions.</summary>
    public static void Request() => API.EmitServer(WalkingStyleEvents.Request);

    /// <summary>The style with this clip set, or null when the owner no longer offers it.</summary>
    public static WalkingStyle? Find(string clipset)
    {
        foreach (var style in Cached)
        {
            if (string.Equals(style.Clipset, clipset, StringComparison.OrdinalIgnoreCase))
            {
                return style;
            }
        }

        return null;
    }

    private static void OnReceived(string payload)
    {
        if (!ClientJson.TryDeserialize<List<WalkingStyle>>(payload, out var read) || read is null)
        {
            Log.Error("[WalkingStyles] The walking styles the server sent could not be read.");

            return;
        }

        Cached.Clear();
        Cached.AddRange(read);

        HasReceived = true;

        Log.Debug($"[WalkingStyles] Received {Cached.Count} style(s).");
    }
}

using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;


using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Events;

// Used instead of entity decorators. A decorator is a fixed-size slot from a pool the whole server
// shares, so a resource registering too many breaks every other one, and the value only exists while
// the entity is streamed in. Reads stay name based, because the point of this layer is reading another
// player's bag by server id whether or not they are around. Writes go through the local player's StateBag.
public static class StateBags
{
    // The number is a server id, not a local index.
    private const string PlayerBagPrefix = "player:";


    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    public static string LocalPlayerBag => PlayerBag(API.GetPlayerServerId(API.PlayerId()));

    // Not being used yet.
    public static int? PlayerFromBag(string bagName) =>
        bagName.StartsWith(PlayerBagPrefix, StringComparison.Ordinal)
        && int.TryParse(
            bagName.Substring(PlayerBagPrefix.Length),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var serverId)
            ? serverId
            : null;

    // replicated false keeps the value on this machine, which is only useful for something a client
    // works out for itself and wants to remember.
    public static bool Set<T>(string bagName, string key, T value, bool replicated = true)
    {
        try
        {
            var bag = bagName == LocalPlayerBag ? Game.Player.State : bagName == "global" ? Script.GlobalState : null;

            if (bag is null)
            {
                Log.Error($"[StateBags] '{bagName}' is not a bag this client can write.");

                return false;
            }

            object? payload = value;

            if (value is Enum)
            {
                payload = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }

            bag.Set(key, payload, replicated);

            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[StateBags] Could not write '{key}' on '{bagName}': {exception}");

            return false;
        }
    }

    public static T? Get<T>(string bagName, string key)
    {
        // A bag nobody has written to does not exist yet, and asking it for a key is not an error.
        if (!API.StateBagHasKey(bagName, key))
        {
            return default;
        }

        try
        {
            return NativeFixer.GetStateBagValue<T>(bagName, key);
        }
        catch (Exception exception)
        {
            Log.Error($"[StateBags] Could not read '{key}' from '{bagName}': {exception}");

            return default;
        }
    }

    public static T? GetPlayer<T>(int serverId, string key) => Get<T>(PlayerBag(serverId), key);

    // The new value is deliberately not passed to the handler, so it reads the same as the Enhanced side.
    // StopWatching is not being used yet.
    public static int Watch(string? key, string? bagName, Action<string, string> handler)
    {
        try
        {
            return NativeFixer.AddStateBagChangeHandler(key, bagName, handler);
        }
        catch (Exception exception)
        {
            Log.Error($"[StateBags] Could not watch '{key ?? "*"}' on '{bagName ?? "*"}': {exception}");

            return 0;
        }
    }

    public static void StopWatching(int cookie)
    {
        if (cookie != 0)
        {
            API.RemoveStateBagChangeHandler(cookie);
        }
    }
}

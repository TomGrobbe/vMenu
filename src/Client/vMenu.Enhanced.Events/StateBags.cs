using System.Globalization;

using CitizenFX.FiveM.Client;

using MessagePack;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Events;

// Used instead of entity decorators. A decorator is a fixed-size slot from a pool the whole server
// shares, so a resource registering too many breaks every other one, and the value only exists while
// the entity is streamed in. 0.0.4's managed StateBag type does not fit either: it can only reach a
// Player it can resolve locally, and the point of this layer is reading another player's bag by
// server id whether or not they are around, so it stays name based.
public static class StateBags
{
    // The number is a server id, not a local index.
    private const string PlayerBagPrefix = "player:";

    // Reused for every write, the options being fixed for the lifetime of the resource.
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard;

    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    public static string LocalPlayerBag => PlayerBag(Native.GetPlayerServerId(Native.PlayerId()));

    // Not being used yet.
    public static int? PlayerFromBag(string bagName) =>
        bagName.StartsWith(PlayerBagPrefix, StringComparison.Ordinal)
        && int.TryParse(
            bagName.AsSpan(PlayerBagPrefix.Length),
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
            var data = MessagePackSerializer.Serialize(value, Options);

            return Native.SetStateBagValue(bagName, key, data, replicated);
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
        if (!Native.StateBagHasKey(bagName, key))
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

    // The new value is deliberately not passed to the handler, even though the game offers it. It arrives
    // as a bare MessagePack blob with no type attached, and turning one into a C# object needs a resolver
    // the runtime does not set up. Watch and StopWatching are not being used yet.
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
            Native.RemoveStateBagChangeHandler(cookie);
        }
    }
}

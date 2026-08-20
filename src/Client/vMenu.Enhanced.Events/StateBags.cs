using System.Globalization;

using CitizenFX.FiveM.Client;

using MessagePack;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Events;

/// <summary>
/// State bags: the game's own way of hanging a named value off a player or an entity and having the
/// server keep every client's copy of it up to date.
/// </summary>
/// <remarks>
/// This is what vMenu uses instead of entity decorators. A decorator is a fixed-size slot from a
/// pool the whole server shares, so a resource that registers too many of them breaks every other
/// resource, and the value only exists while the entity is streamed in. A state bag has neither
/// problem.
///
/// <para>
/// 0.0.4 added a managed <c>StateBag</c> type, reached through <c>Player.State</c>, an entity's
/// <c>State</c>, or <c>StateBag.GetForEntity</c>. None of those fit here: the whole point of this
/// layer is to read another player's bag by server id, and the managed API can only reach a
/// <c>Player</c> it can resolve locally, which a streamed out player is not. Addressing the bag by
/// name works whether or not the player is around, so this stays name based, with
/// <see cref="NativeFixer" /> underneath for the one native the generator still shapes wrong.
/// </para>
/// </remarks>
public static class StateBags
{
    /// <summary>What the game calls a player's bag. The number is a server id, not a local index.</summary>
    private const string PlayerBagPrefix = "player:";

    /// <summary>Reused for every write, the options being fixed for the lifetime of the resource.</summary>
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard;

    /// <summary>The bag belonging to a player, by server id.</summary>
    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    /// <summary>The bag belonging to this player.</summary>
    public static string LocalPlayerBag => PlayerBag(Native.GetPlayerServerId(Native.PlayerId()));

    /// <summary>The server id a bag name belongs to, or null when it is not a player bag.</summary>
    public static int? PlayerFromBag(string bagName) =>
        bagName.StartsWith(PlayerBagPrefix, StringComparison.Ordinal)
        && int.TryParse(
            bagName.AsSpan(PlayerBagPrefix.Length),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var serverId)
            ? serverId
            : null;

    /// <summary>Writes a value, replacing whatever was there.</summary>
    /// <param name="replicated">
    /// Whether every other machine is told. False keeps the value on this one, which is only useful
    /// for something a client works out for itself and wants to remember.
    /// </param>
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

    /// <summary>Reads a value, or <see langword="default"/> when the key was never written.</summary>
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

    /// <summary>Reads a value from a player's bag, by server id.</summary>
    public static T? GetPlayer<T>(int serverId, string key) => Get<T>(PlayerBag(serverId), key);

    /// <summary>Watches for writes and calls back with the bag and the key that changed.</summary>
    /// <param name="key">The key to watch, or <see langword="null"/> for every key.</param>
    /// <param name="bagName">The bag to watch, or <see langword="null"/> for every bag.</param>
    /// <returns>A cookie for <see cref="StopWatching" />.</returns>
    /// <remarks>
    /// The new value is deliberately not passed to the handler, even though the game offers it. It
    /// arrives as a bare MessagePack blob with no type attached, and turning one of those into a C#
    /// object needs a resolver the runtime does not set up. Reading the value back with
    /// <see cref="Get{T}" />, where the caller knows what type it wants, costs one native call on an
    /// event that fires rarely and cannot guess wrong.
    /// </remarks>
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

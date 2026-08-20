using System.Globalization;

using CitizenFX.FiveM.Server;

using MessagePack;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization.Server;

/// <summary>
/// The server half of vMenu's state bag handling: writing values every client then sees.
/// </summary>
/// <remarks>
/// The client half lives in <c>vMenu.Enhanced.Events.StateBags</c>. They are separate rather than
/// shared because the natives come from a different <c>Native</c> class on each side, and the two
/// sides want different things: the server almost only writes, the client almost only reads.
///
/// <para>
/// Written by hand because the CitizenFX packages offer no state bag support of any kind. There is
/// no <c>StateBag</c> type and no <c>State</c> property on a player, only the raw natives.
/// </para>
///
/// <para>
/// It lives with the server's serialization rather than with its actions because a state bag value
/// is a MessagePack blob on the wire, and because both the actions layer and the permissions layer
/// write these. Permissions sits underneath actions, so a shared home has to sit underneath both.
/// </para>
/// </remarks>
public static class ServerStateBags
{
    /// <summary>What the game calls a player's bag. The number is a server id.</summary>
    private const string PlayerBagPrefix = "player:";

    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard;

    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    /// <summary>Writes a value onto a player's bag and tells every client about it.</summary>
    public static bool SetPlayer<T>(int serverId, string key, T value) =>
        Set(PlayerBag(serverId), key, value);

    /// <summary>Writes a value, replacing whatever was there.</summary>
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
}

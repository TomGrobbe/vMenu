using System.Globalization;

using CitizenFX.FiveM.Server;

using MessagePack;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization.Server;

// The client half lives in vMenu.Enhanced.Events.StateBags. They are separate rather than shared
// because the natives come from a different Native class on each side, and the two sides want
// different things: the server almost only writes, the client almost only reads. Written by hand
// because the CitizenFX packages offer no state bag support of any kind. It lives with the server's
// serialization because a bag value is a MessagePack blob on the wire, and because both the actions
// layer and the permissions layer write these.
public static class ServerStateBags
{
    // The number is a server id.
    private const string PlayerBagPrefix = "player:";

    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard;

    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    public static bool SetPlayer<T>(int serverId, string key, T value) =>
        Set(PlayerBag(serverId), key, value);

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

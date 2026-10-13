using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Serialization.Server;

public static class ServerStateBags
{
    private const string PlayerBagPrefix = "player:";

    public static string PlayerBag(int serverId) =>
        PlayerBagPrefix + serverId.ToString(CultureInfo.InvariantCulture);

    public static bool SetPlayer<T>(int serverId, string key, T value) =>
        Set(PlayerBag(serverId), key, value);

    public static T? Get<T>(string bagName, string key)
    {
        if (!API.StateBagHasKey(bagName, key))
        {
            return default;
        }

        try
        {
            var value = (object)API.GetStateBagValue(bagName, key);

            return value is null ? default : JToken.FromObject(value).ToObject<T>();
        }
        catch (Exception exception)
        {
            Log.Error($"[StateBags] Could not read '{key}' from '{bagName}': {exception}");

            return default;
        }
    }

    public static bool Set<T>(string bagName, string key, T value, bool replicated = true)
    {
        try
        {
            var bag = Resolve(bagName);

            if (bag is null)
            {
                Log.Error($"[StateBags] '{bagName}' is not a bag the server can write.");

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

    private static StateBag? Resolve(string bagName)
    {
        if (bagName == "global")
        {
            return Script.GlobalState;
        }

        if (bagName.StartsWith(PlayerBagPrefix, StringComparison.Ordinal)
            && int.TryParse(bagName.Substring(PlayerBagPrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId))
        {
            return Script.Players[serverId]?.State;
        }

        return null;
    }
}

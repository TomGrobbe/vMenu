using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Actions.Server.Events;

// Under OneSync a mine set off on one machine leaves the other copies armed, so the spot is cleared for everyone.
public static class VehicleMineBroadcast
{
    private const string ExplosionEvent = "explosionEvent";

    private const string DroppedEvent = "playerDropped";

    private const int BurstAllowance = 20;

    private const int BurstWindowMs = 2000;

    private static readonly PlayerWindows Bursts = new();

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnEvent(ExplosionEvent, new Action<MessagePackBuffer?, MessagePackBuffer?>(OnExplosion), false);
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);
    }

    private static void OnExplosion(MessagePackBuffer? sender, MessagePackBuffer? data)
    {
        if (data is null || Sender(sender) is not int source)
        {
            return;
        }

        var fields = Read<Dictionary<string, MessagePackBuffer>>(data);

        if (fields is null
            || !Number(fields, "explosionType", out var type)
            || !VehicleMineProfiles.IsMineExplosion((int)type)
            || !Number(fields, "posX", out var x)
            || !Number(fields, "posY", out var y)
            || !Number(fields, "posZ", out var z)
            || !TakeAllowance(source))
        {
            return;
        }

        var bucket = Native.GetPlayerRoutingBucket(Targets.Id(source));

        foreach (var player in ConnectedPlayers.All())
        {
            if (Native.GetPlayerRoutingBucket(Targets.Id(player.ServerId)) != bucket)
            {
                continue;
            }

            API.EmitClient(player.ServerId, VehicleMineProfiles.DetonatedEvent, x, y, z);
        }
    }

    private static int? Sender(MessagePackBuffer? sender)
    {
        if (sender is null)
        {
            return null;
        }

        if (Read<string>(sender) is string text)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
        }

        return Number(sender, out var number) && number > 0 ? (int)number : null;
    }

    private static bool Number(Dictionary<string, MessagePackBuffer> fields, string name, out float value)
    {
        value = 0f;

        return fields.TryGetValue(name, out var field) && Number(field, out value);
    }

    private static bool Number(MessagePackBuffer field, out float value)
    {
        try
        {
            value = field.DeserializeTo<float>(true);

            return float.IsFinite(value);
        }
        catch (Exception)
        {
            value = 0f;

            return false;
        }
    }

    private static T? Read<T>(MessagePackBuffer value)
        where T : class
    {
        try
        {
            return value.DeserializeTo<T>(true);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool TakeAllowance(int player)
    {
        var now = Environment.TickCount;
        var burst = Bursts.For(player, now, BurstWindowMs);

        if (burst.Count >= BurstAllowance)
        {
            return false;
        }

        burst.Add(now);

        return true;
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            Bursts.Forget(source);
        }
    }
}

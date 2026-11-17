using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Webhooks.Server;

using vMenu.Legacy.Hosting;

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

        Script.EventHandlers[ExplosionEvent] += new Action<string, object, object>(OnExplosion);
        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);
    }

    private static void OnExplosion([FromSource] string eventSource, object sender, object data)
    {
        if (Script.IsFromPlayer(eventSource)
            || Sender(sender) is not int source
            || data is not IDictionary<string, object> fields
            || !Number(fields, "explosionType", out var type)
            || !VehicleMineProfiles.IsMineExplosion((int)type)
            || !Number(fields, "posX", out var x)
            || !Number(fields, "posY", out var y)
            || !Number(fields, "posZ", out var z)
            || !TakeAllowance(source))
        {
            return;
        }

        var bucket = API.GetPlayerRoutingBucket(Targets.Id(source));

        foreach (var player in ConnectedPlayers.All())
        {
            if (API.GetPlayerRoutingBucket(Targets.Id(player.ServerId)) != bucket)
            {
                continue;
            }

            Script.TriggerClientEvent(player.ServerId, VehicleMineProfiles.DetonatedEvent, x, y, z);
        }
    }

    private static int? Sender(object? sender)
    {
        var text = Convert.ToString(sender, CultureInfo.InvariantCulture);

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
    }

    private static bool Number(IDictionary<string, object> fields, string name, out float value)
    {
        value = 0f;

        if (!fields.TryGetValue(name, out var field) || field is null)
        {
            return false;
        }

        try
        {
            value = Convert.ToSingle(field, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return false;
        }

        return !float.IsNaN(value) && !float.IsInfinity(value);
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

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source > 0)
        {
            Bursts.Forget(source);
        }
    }
}

using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Deaths;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Webhooks.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class PedDeathBroadcast
{
    private const string PedDeathEvent = "onPedDeath";

    private const uint NoEntity = uint.MaxValue;

    private static bool _registered;

    private static bool _reported;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[PedDeathEvent] += new Action<string, uint, uint, uint>(([FromSource] string source, uint arg1, uint arg2, uint arg3) =>
            {
                if (!Script.IsFromPlayer(source))
                {
                    OnPedDeath(arg1, arg2, arg3);
                }
            });
    }

    private static void OnPedDeath(uint entity, uint attacker, uint cause)
    {
        var victimPed = unchecked((int)entity);
        var attackerPed = attacker == NoEntity ? 0 : unchecked((int)attacker);

        if (!_reported)
        {
            _reported = true;

            Log.Debug(
                $"[Deaths] onPedDeath is firing. First one: entity {entity}, attacker {attacker}, "
                + $"cause {cause}, entity is a player: {API.IsPedAPlayer(victimPed)}.");
        }

        if (!API.IsPedAPlayer(victimPed))
        {
            return;
        }

        var players = ConnectedPlayers.All();

        var victim = ConnectedPlayers.Owning(players, victimPed);

        if (victim is null)
        {
            return;
        }

        var killer = ConnectedPlayers.Owning(players, attackerPed);

        if (killer is not null && killer.ServerId == victim.ServerId)
        {
            killer = null;
        }

        Log.Debug(
            $"[Deaths] {victim.Name} ({victim.ServerId}) died. "
            + $"Attacker entity {attackerPed}, killer {killer?.Name ?? "none"}, cause {cause}.");

        if (WebhookLog.Wants(LogCategory.Event))
        {
            WebhookLog.Event(
                killer is null ? "died." : "was killed by",
                WebhookActor.For(victim.ServerId, victim.Name),
                killer is null ? null : WebhookActor.For(killer.ServerId, killer.Name));
        }

        var victimId = victim.ServerId.ToString(CultureInfo.InvariantCulture);
        var killerId = killer is null ? string.Empty : killer.ServerId.ToString(CultureInfo.InvariantCulture);
        var killerName = killer?.Name ?? string.Empty;
        var causeHash = cause.ToString(CultureInfo.InvariantCulture);

        foreach (var player in players)
        {
            Script.TriggerClientEvent(
                player.ServerId,
                DeathEvents.Announce,
                victimId,
                victim.Name,
                killerId,
                killerName,
                causeHash);
        }
    }
}

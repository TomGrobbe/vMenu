using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Mugger
{
    private const float Sprint = 3f;

    private const float ReachDistance = 1.8f;

    private const int ApproachTimeoutMs = 45000;

    private const int FleeMs = 20000;

    private const int CorpseMs = 10000;

    private const float PushStrength = 3f;

    private const int ImpulseForce = 1;

    private const int TeeterEdge = 4;

    private const int ShovedRagdollMs = 6000;

    private static readonly uint Knife = Streaming.Hash("WEAPON_KNIFE");

    private static bool _active;

    public static void Initialize() =>
        API.OnNetEvent(BullyEvents.Shoved, new Action<string, string>(OnShoved), false);

    public static async Task Start(BullyRun run)
    {
        if (_active)
        {
            run.Busy();

            return;
        }

        _active = true;

        var mugger = 0;
        var blip = 0;

        try
        {
            var model = Streaming.Hash(BullyModels.Muggers[Dice.Next(BullyModels.Muggers.Count)]);

            if (!await Streaming.ModelAsync(model))
            {
                run.Failed();

                return;
            }

            var generation = BullyState.Generation;
            var position = Native.GetEntityCoords(Native.PlayerPedId(), false);

            if (NpcSupport.SafeSpotAround(position, 20f, 30f, hidden: true) is not { } spot)
            {
                Native.SetModelAsNoLongerNeeded(model);

                run.Failed();

                return;
            }

            mugger = Native.CreatePed(NpcSupport.CivilianPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, position), true, false);

            Native.SetModelAsNoLongerNeeded(model);

            if (mugger == 0)
            {
                run.Failed();

                return;
            }

            Native.SetEntityAsMissionEntity(mugger, true, true);
            Native.SetBlockingOfNonTemporaryEvents(mugger, true);
            Native.GiveWeaponToPed(mugger, Knife, 1, true, true);
            Native.SetPedDropsWeaponsWhenDead(mugger, false);
            Native.SetPedMoney(mugger, 0);

            NpcSupport.Own(mugger);

            blip = NpcSupport.HostileBlip(mugger);

            run.Started();

            var left = new List<int>(run.Victims);
            var lastVictim = 0;
            var killed = false;

            while (left.Count > 0 && generation == BullyState.Generation && !killed)
            {
                var victim = Nearest(mugger, left);

                left.Remove(victim);

                var chase = await ChaseAsync(mugger, victim, generation);

                killed = chase.Killed;
                lastVictim = chase.Ped == 0 ? lastVictim : chase.Ped;
            }

            if (!killed && lastVictim != 0)
            {
                NpcSupport.MakeFlee(mugger, lastVictim);
            }

            var fleeUntil = Native.GetGameTimer() + FleeMs;

            while (!killed && generation == BullyState.Generation && Native.GetGameTimer() < fleeUntil)
            {
                killed = Dead(mugger);

                await API.Delay(500);
            }

            if (killed)
            {
                NpcSupport.RemoveBlip(blip);

                blip = 0;

                await API.Delay(CorpseMs);
            }
        }
        finally
        {
            NpcSupport.RemoveBlip(blip);

            if (mugger != 0)
            {
                await NpcSupport.DeleteAsync(mugger);
            }

            _active = false;
        }
    }

    private static int Nearest(int mugger, List<int> victims)
    {
        var from = Native.GetEntityCoords(mugger, false);

        return victims
            .OrderBy(victim => BullyRun.PedOf(victim) is var ped and not 0
                ? Vector3.Distance(from, Native.GetEntityCoords(ped, false))
                : float.MaxValue)
            .First();
    }

    private static bool Dead(int mugger) => !Native.DoesEntityExist(mugger) || Native.IsPedDeadOrDying(mugger, true);

    private static async Task<(bool Killed, int Ped)> ChaseAsync(int mugger, int victim, int generation)
    {
        var chasing = 0;
        var giveUpAt = Native.GetGameTimer() + ApproachTimeoutMs;

        while (generation == BullyState.Generation && Native.GetGameTimer() < giveUpAt)
        {
            if (Dead(mugger))
            {
                return (true, chasing);
            }

            var ped = BullyRun.PedOf(victim);

            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                return (false, chasing);
            }

            if (ped != chasing)
            {
                chasing = ped;

                Native.TaskGoToEntity(mugger, ped, -1, 1f, Sprint, 0f, 0);
            }

            if (Vector3.Distance(Native.GetEntityCoords(mugger, false), Native.GetEntityCoords(ped, false)) <= ReachDistance)
            {
                Rob(mugger, victim, ped);

                return (false, ped);
            }

            await API.Delay(100);
        }

        return (false, chasing);
    }

    private static void Rob(int mugger, int victim, int ped)
    {
        var away = Native.GetEntityCoords(ped, false) - Native.GetEntityCoords(mugger, false);
        var push = new Vector2(away.X, away.Y);

        push = push.LengthSquared() > 0.001f ? Vector2.Normalize(push) * PushStrength : new Vector2(0f, PushStrength);

        if (victim == Native.GetPlayerServerId(Native.PlayerId()))
        {
            Shove(push);

            return;
        }

        API.EmitServer(
            BullyEvents.Shove,
            victim.ToString(CultureInfo.InvariantCulture),
            push.X.ToString(CultureInfo.InvariantCulture),
            push.Y.ToString(CultureInfo.InvariantCulture));
    }

    private static void OnShoved(string x, string y)
    {
        if (float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var pushX)
            && float.TryParse(y, NumberStyles.Float, CultureInfo.InvariantCulture, out var pushY))
        {
            Shove(new Vector2(pushX, pushY));
        }
    }

    private static void Shove(Vector2 push)
    {
        var ped = Native.PlayerPedId();

        PedPranks.AllowRagdoll(ShovedRagdollMs);

        Native.SetHighFallTask(ped, 5000, 10000, TeeterEdge);
        Native.ApplyForceToEntity(ped, ImpulseForce, push.X, push.Y, 0f, 0f, 0f, 0.5f, 0, false, true, true, false, true);
    }
}

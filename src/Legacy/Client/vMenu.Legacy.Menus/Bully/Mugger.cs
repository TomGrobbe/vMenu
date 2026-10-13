using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;

using vMenu.Legacy.Hosting;

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
        Script.EventHandlers[BullyEvents.Shoved] += new Action<string, string>(OnShoved);

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

            var stopAllCount = BullyState.StopAllCount;
            var position = API.GetEntityCoords(API.PlayerPedId(), false);

            if (SpawnSpots.FindSafeSpotAround(position, 20f, 30f, hidden: true) is not { } spot)
            {
                API.SetModelAsNoLongerNeeded(model);

                run.Failed();

                return;
            }

            mugger = API.CreatePed(HostilePeds.CivilianPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, position), true, false);

            API.SetModelAsNoLongerNeeded(model);

            if (mugger == 0)
            {
                run.Failed();

                return;
            }

            API.SetEntityAsMissionEntity(mugger, true, true);
            API.SetBlockingOfNonTemporaryEvents(mugger, true);
            API.GiveWeaponToPed(mugger, Knife, 1, true, true);
            API.SetPedDropsWeaponsWhenDead(mugger, false);
            API.SetPedMoney(mugger, 0);

            BullySpawnCleanup.TrackForCleanup(mugger);

            blip = HostilePeds.AddHostileBlip(mugger);

            run.Started();

            var left = new List<int>(run.Victims);
            var lastVictim = 0;
            var killed = false;

            while (left.Count > 0 && stopAllCount == BullyState.StopAllCount && !killed)
            {
                var victim = Nearest(mugger, left);

                left.Remove(victim);

                var chase = await ChaseAsync(mugger, victim, stopAllCount);

                killed = chase.Killed;
                lastVictim = chase.Ped == 0 ? lastVictim : chase.Ped;
            }

            if (!killed && lastVictim != 0)
            {
                HostilePeds.MakeFleeFrom(mugger, lastVictim);
            }

            var fleeUntil = API.GetGameTimer() + FleeMs;

            while (!killed && stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < fleeUntil)
            {
                killed = Dead(mugger);

                await BaseScript.Delay(500);
            }

            if (killed)
            {
                HostilePeds.RemoveBlip(blip);

                blip = 0;

                await BaseScript.Delay(CorpseMs);
            }
        }
        finally
        {
            HostilePeds.RemoveBlip(blip);

            if (mugger != 0)
            {
                await BullySpawnCleanup.DeleteAsync(mugger);
            }

            _active = false;
        }
    }

    private static int Nearest(int mugger, List<int> victims)
    {
        var from = API.GetEntityCoords(mugger, false);

        return victims
            .OrderBy(victim => BullyRun.PedOf(victim) is var ped and not 0
                ? Vector3.Distance(from, API.GetEntityCoords(ped, false))
                : float.MaxValue)
            .First();
    }

    private static bool Dead(int mugger) => !API.DoesEntityExist(mugger) || API.IsPedDeadOrDying(mugger, true);

    private static async Task<(bool Killed, int Ped)> ChaseAsync(int mugger, int victim, int stopAllCount)
    {
        var chasing = 0;
        var giveUpAt = API.GetGameTimer() + ApproachTimeoutMs;

        while (stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < giveUpAt)
        {
            if (Dead(mugger))
            {
                return (true, chasing);
            }

            var ped = BullyRun.PedOf(victim);

            if (ped == 0 || !API.DoesEntityExist(ped))
            {
                return (false, chasing);
            }

            if (ped != chasing)
            {
                chasing = ped;

                API.TaskGoToEntity(mugger, ped, -1, 1f, Sprint, 0f, 0);
            }

            if (Vector3.Distance(API.GetEntityCoords(mugger, false), API.GetEntityCoords(ped, false)) <= ReachDistance)
            {
                Rob(mugger, victim, ped);

                return (false, ped);
            }

            await BaseScript.Delay(100);
        }

        return (false, chasing);
    }

    private static void Rob(int mugger, int victim, int ped)
    {
        var away = API.GetEntityCoords(ped, false) - API.GetEntityCoords(mugger, false);
        var push = new Vector2(away.X, away.Y);

        push = push.LengthSquared() > 0.001f ? Vector2.Normalize(push) * PushStrength : new Vector2(0f, PushStrength);

        if (victim == API.GetPlayerServerId(API.PlayerId()))
        {
            Shove(push);

            return;
        }

        BaseScript.TriggerServerEvent(
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
        var ped = API.PlayerPedId();

        PedPranks.AllowRagdoll(ShovedRagdollMs);

        API.SetHighFallTask(ped, 5000, 10000, TeeterEdge);
        API.ApplyForceToEntity(ped, ImpulseForce, push.X, push.Y, 0f, 0f, 0f, 0.5f, 0, false, true, true, false, true);
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Bully;

internal static class CougarAttack
{
    private const float MinDistance = 25f;

    private const float MaxDistance = 40f;

    private const int AttackMs = 60000;

    private const int FleeMs = 20000;

    private const int CorpseMs = 10000;

    private static bool _active;

    public static async Task Start(BullyRun run)
    {
        if (_active)
        {
            run.Busy();

            return;
        }

        _active = true;

        var lion = 0;

        try
        {
            var model = Streaming.Hash(BullyModels.MountainLion);

            if (!await Streaming.ModelAsync(model))
            {
                run.Failed();

                return;
            }

            var stopAllCount = BullyState.StopAllCount;
            var victim = API.PlayerPedId();
            var position = API.GetEntityCoords(victim, false);

            if (SpawnSpots.FindSafeSpotAround(position, MinDistance, MaxDistance, hidden: true) is not { } spot)
            {
                API.SetModelAsNoLongerNeeded(model);

                run.Failed();

                return;
            }

            lion = API.CreatePed(MountainLion.AnimalPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, position), true, false);

            API.SetModelAsNoLongerNeeded(model);

            if (lion == 0)
            {
                run.Failed();

                return;
            }

            API.SetEntityAsMissionEntity(lion, true, true);
            API.SetBlockingOfNonTemporaryEvents(lion, true);

            BullySpawnCleanup.TrackForCleanup(lion);
            HostilePeds.MakeHostile(lion);
            HostilePeds.Attack(lion, victim);

            run.Started();

            var killed = false;
            var giveUpAt = API.GetGameTimer() + AttackMs;

            while (stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < giveUpAt)
            {
                if (Dead(lion))
                {
                    killed = true;

                    break;
                }

                if (API.IsPedDeadOrDying(API.PlayerPedId(), true))
                {
                    break;
                }

                await BaseScript.Delay(250);
            }

            if (!killed)
            {
                HostilePeds.MakeFleeFrom(lion, API.PlayerPedId());
            }

            var fleeUntil = API.GetGameTimer() + FleeMs;

            while (!killed && stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < fleeUntil)
            {
                killed = Dead(lion);

                await BaseScript.Delay(500);
            }

            if (killed)
            {
                await BaseScript.Delay(CorpseMs);
            }
        }
        finally
        {
            if (lion != 0)
            {
                await BullySpawnCleanup.DeleteAsync(lion);
            }

            _active = false;
        }
    }

    private static bool Dead(int lion) => !API.DoesEntityExist(lion) || API.IsPedDeadOrDying(lion, true);
}

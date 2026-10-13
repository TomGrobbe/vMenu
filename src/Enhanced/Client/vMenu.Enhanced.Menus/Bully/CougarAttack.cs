using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

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
            var victim = Native.PlayerPedId();
            var position = Native.GetEntityCoords(victim, false);

            if (SpawnSpots.FindSafeSpotAround(position, MinDistance, MaxDistance, hidden: true) is not { } spot)
            {
                Native.SetModelAsNoLongerNeeded(model);

                run.Failed();

                return;
            }

            lion = Native.CreatePed(MountainLion.AnimalPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, position), true, false);

            Native.SetModelAsNoLongerNeeded(model);

            if (lion == 0)
            {
                run.Failed();

                return;
            }

            Native.SetEntityAsMissionEntity(lion, true, true);
            Native.SetBlockingOfNonTemporaryEvents(lion, true);

            BullySpawnCleanup.TrackForCleanup(lion);
            HostilePeds.MakeHostile(lion);
            HostilePeds.Attack(lion, victim);

            run.Started();

            var killed = false;
            var giveUpAt = Native.GetGameTimer() + AttackMs;

            while (stopAllCount == BullyState.StopAllCount && Native.GetGameTimer() < giveUpAt)
            {
                if (Dead(lion))
                {
                    killed = true;

                    break;
                }

                if (Native.IsPedDeadOrDying(Native.PlayerPedId(), true))
                {
                    break;
                }

                await API.Delay(250);
            }

            if (!killed)
            {
                HostilePeds.MakeFleeFrom(lion, Native.PlayerPedId());
            }

            var fleeUntil = Native.GetGameTimer() + FleeMs;

            while (!killed && stopAllCount == BullyState.StopAllCount && Native.GetGameTimer() < fleeUntil)
            {
                killed = Dead(lion);

                await API.Delay(500);
            }

            if (killed)
            {
                await API.Delay(CorpseMs);
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

    private static bool Dead(int lion) => !Native.DoesEntityExist(lion) || Native.IsPedDeadOrDying(lion, true);
}

using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Vehicles;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Carjackers
{
    private const float SearchRadius = 60f;

    private const int MaxJackers = 3;

    private const int MaxAnimals = 2;

    private const int MissionPopulation = 7;

    private const int DriverSeat = -1;

    private const int EnterFlags = ResumeIfInterrupted | JackAnyone | DontWaitForVehicleToStop;

    private const int ResumeIfInterrupted = 1;

    private const int JackAnyone = 8;

    private const int DontWaitForVehicleToStop = 64;

    private const int WillJackAnyPlayer = 141;

    private const float Run = 2f;

    private const int EnterTimeoutMs = 20000;

    private const int ForgetAfterMs = 60000;

    private const float GetawaySpeed = 40f;

    private const int GetawayDriving = 786468;

    public static async Task Start(BullyRun run)
    {
        var player = Native.PlayerPedId();
        var vehicle = Native.GetVehiclePedIsIn(player, true);

        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            run.Skip(BullyEvents.RefusedNoVehicle);

            return;
        }

        var target = Native.GetEntityCoords(vehicle, false);

        var nearby = NativeFixer.GetGamePool("CPed")
            .Where(ped => ped != player
                && Native.DoesEntityExist(ped)
                && !Native.IsPedAPlayer(ped)
                && !Native.IsPedDeadOrDying(ped, true)
                && Native.GetEntityPopulationType(ped) != MissionPopulation
                && Vector3.Distance(Native.GetEntityCoords(ped, false), target) <= SearchRadius)
            .OrderBy(ped => Vector3.Distance(Native.GetEntityCoords(ped, false), target))
            .ToLookup(Native.IsPedHuman);

        var jackers = nearby[true].Take(MaxJackers).ToList();
        var spawned = jackers.Count == 0 ? await SpawnJackerAsync(target) : 0;

        if (spawned != 0)
        {
            jackers.Add(spawned);
        }

        var tasked = new List<int>();

        foreach (var jacker in jackers)
        {
            if (await JackAsync(jacker, vehicle))
            {
                tasked.Add(jacker);

                run.Started();
            }
        }

        foreach (var animal in nearby[false].Take(MaxAnimals))
        {
            if (await NetworkEntity.TakeControlAsync(animal))
            {
                HostilePeds.Attack(animal, player);

                run.Started();
            }
        }

        run.Failed();

        // Overrides the target's own Stay In Vehicle for as long as the jack can take, like ragdolling does.
        Native.SetPedCanBeDraggedOut(player, true);

        try
        {
            await DriveOffAsync(tasked, vehicle);
        }
        finally
        {
            PedProtection.Reapply();
        }

        if (spawned == 0)
        {
            return;
        }

        await API.Delay(ForgetAfterMs);

        BullySpawnCleanup.StopTrackingForCleanup(spawned);

        if (Native.DoesEntityExist(spawned))
        {
            Native.SetPedAsNoLongerNeeded(ref spawned);
        }
    }

    private static async Task<int> SpawnJackerAsync(Vector3 near)
    {
        var model = Streaming.Hash(BullyModels.Carjacker);

        if (!await Streaming.ModelAsync(model))
        {
            return 0;
        }

        if (SpawnSpots.FindSafeSpotAround(near, 15f, 30f, hidden: true) is not { } spot)
        {
            Native.SetModelAsNoLongerNeeded(model);

            return 0;
        }

        var ped = Native.CreatePed(HostilePeds.CivilianPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, near), true, false);

        Native.SetModelAsNoLongerNeeded(model);

        if (ped == 0)
        {
            return 0;
        }

        Native.SetEntityAsMissionEntity(ped, true, true);

        BullySpawnCleanup.TrackForCleanup(ped);

        return ped;
    }

    private static async Task<bool> JackAsync(int ped, int vehicle)
    {
        if (!await NetworkEntity.TakeControlAsync(ped))
        {
            return false;
        }

        Native.SetBlockingOfNonTemporaryEvents(ped, true);
        Native.SetPedConfigFlag(ped, WillJackAnyPlayer, true);
        Native.TaskEnterVehicle(ped, vehicle, EnterTimeoutMs, DriverSeat, Run, EnterFlags, string.Empty);
        Native.SetPedKeepTask(ped, true);

        return true;
    }

    // Whoever wins the driver seat takes off with it. Done by watching rather than with a task sequence,
    // which does not survive on ambient peds this client does not keep owning.
    private static async Task DriveOffAsync(List<int> jackers, int vehicle)
    {
        var giveUpAt = Native.GetGameTimer() + EnterTimeoutMs;

        while (jackers.Count > 0 && Native.GetGameTimer() < giveUpAt && Native.DoesEntityExist(vehicle))
        {
            if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0 && jackers.Contains(driver))
            {
                Native.TaskVehicleDriveWander(driver, vehicle, GetawaySpeed, GetawayDriving);
                Native.SetPedKeepTask(driver, true);

                return;
            }

            await API.Delay(250);
        }
    }
}

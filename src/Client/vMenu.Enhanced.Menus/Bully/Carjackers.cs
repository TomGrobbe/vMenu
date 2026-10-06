using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Vehicles;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Carjackers
{
    private const float SearchRadius = 250f;

    private const float AnimalReach = 3.5f;

    private const float AnimalSprint = 3f;

    private const int MissionPopulation = 7;

    private const int DriverSeat = -1;

    private const int EnterFlags = ResumeIfInterrupted | JackAnyone | DontWaitForVehicleToStop;

    private const int ResumeIfInterrupted = 1;

    private const int JackAnyone = 8;

    private const int DontWaitForVehicleToStop = 64;

    private const int DontCloseDoor = 256;

    private const int BlockSeatShuffling = 1048576;

    private const int PlayerExitMs = 5000;

    private const int AnimalEnterMs = 10000;

    private const int WillJackAnyPlayer = 141;

    private const float Run = 2f;

    private const int EnterTimeoutMs = 60000;

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

        var thief = NativeFixer.GetGamePool("CPed")
            .Where(ped => ped != player
                && Native.DoesEntityExist(ped)
                && !Native.IsPedAPlayer(ped)
                && !Native.IsPedDeadOrDying(ped, true)
                && Native.GetEntityPopulationType(ped) != MissionPopulation
                && Distance(ped, target) <= SearchRadius)
            .OrderBy(ped => Distance(ped, target))
            .FirstOrDefault();


        if (thief != 0 && Native.GetPedType(thief) == MountainLion.AnimalPedType)
        {
            Native.SetPedCanBeDraggedOut(player, true);

            try
            {
                await AnimalJackAsync(run, thief, vehicle);
            }
            finally
            {
                PedProtection.Reapply();
            }

            return;
        }

        var spawned = thief == 0 ? await SpawnJackerAsync(target) : 0;
        var jacker = thief != 0 ? thief : spawned;
        var tasked = new List<int>();

        if (jacker != 0 && await JackAsync(jacker, vehicle))
        {
            tasked.Add(jacker);

            run.Started();
        }
        else
        {
            run.Failed();
        }

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

    // Animals walk to the driver door and get in, but have no clips for pulling someone out, so the target gets out first.
    private static async Task AnimalJackAsync(BullyRun run, int animal, int vehicle)
    {
        if (!await NetworkEntity.TakeControlAsync(animal))
        {
            run.Failed();

            return;
        }

        Native.SetBlockingOfNonTemporaryEvents(animal, true);
        Native.TaskEnterVehicle(animal, vehicle, -1, DriverSeat, AnimalSprint, ResumeIfInterrupted | JackAnyone | BlockSeatShuffling, string.Empty);
        Native.SetPedKeepTask(animal, true);

        run.Started();

        var stopAllCount = BullyState.StopAllCount;
        var giveUpAt = Native.GetGameTimer() + EnterTimeoutMs;

        while (Vector3.Distance(Native.GetEntityCoords(animal, false), Native.GetEntityCoords(vehicle, false)) > AnimalReach
            && Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            if (Native.GetGameTimer() > giveUpAt
                || stopAllCount != BullyState.StopAllCount
                || !Native.DoesEntityExist(animal)
                || Native.IsPedDeadOrDying(animal, true)
                || !Native.DoesEntityExist(vehicle))
            {
                return;
            }

            await API.Delay(100);
        }

        var player = Native.PlayerPedId();

        if (Native.IsPedInVehicle(player, vehicle, false))
        {
            Native.TaskLeaveVehicle(player, vehicle, DontCloseDoor);

            var outBy = Native.GetGameTimer() + PlayerExitMs;

            while (Native.IsPedInVehicle(player, vehicle, false) && Native.GetGameTimer() < outBy)
            {
                await API.Delay(0);
            }
        }

        if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            Native.TaskEnterVehicle(animal, vehicle, AnimalEnterMs, DriverSeat, Run, ResumeIfInterrupted | BlockSeatShuffling, string.Empty);
        }

        var seatedBy = Native.GetGameTimer() + AnimalEnterMs + PlayerExitMs;

        while (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal && Native.GetGameTimer() < seatedBy)
        {
            if (stopAllCount != BullyState.StopAllCount || !Native.DoesEntityExist(animal) || !Native.DoesEntityExist(vehicle))
            {
                return;
            }

            await API.Delay(100);
        }

        if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            if (!Native.IsVehicleSeatFree(vehicle, DriverSeat, false))
            {
                return;
            }

            Native.SetPedIntoVehicle(animal, vehicle, DriverSeat);
        }

        Native.SetVehicleEngineOn(vehicle, true, true, false);
        Native.TaskVehicleDriveWander(animal, vehicle, GetawaySpeed, GetawayDriving);
        Native.SetPedKeepTask(animal, true);
    }

    private static float Distance(int ped, Vector3 target) => Vector3.Distance(Native.GetEntityCoords(ped, false), target);

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


using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Vehicles;

using vMenu.Legacy.Hosting;

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
        var player = API.PlayerPedId();
        var vehicle = API.GetVehiclePedIsIn(player, true);

        if (vehicle == 0 || !API.DoesEntityExist(vehicle))
        {
            run.Skip(BullyEvents.RefusedNoVehicle);

            return;
        }

        var target = API.GetEntityCoords(vehicle, false);

        var thief = NativeFixer.GetGamePool("CPed")
            .Where(ped => ped != player
                && API.DoesEntityExist(ped)
                && !API.IsPedAPlayer(ped)
                && !API.IsPedDeadOrDying(ped, true)
                && API.GetEntityPopulationType(ped) != MissionPopulation
                && Distance(ped, target) <= SearchRadius)
            .OrderBy(ped => Distance(ped, target))
            .FirstOrDefault();


        if (thief != 0 && API.GetPedType(thief) == MountainLion.AnimalPedType)
        {
            API.SetPedCanBeDraggedOut(player, true);

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
        API.SetPedCanBeDraggedOut(player, true);

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

        await BaseScript.Delay(ForgetAfterMs);

        BullySpawnCleanup.StopTrackingForCleanup(spawned);

        if (API.DoesEntityExist(spawned))
        {
            NativeFixer.SetPedAsNoLongerNeeded(ref spawned);
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

        API.SetBlockingOfNonTemporaryEvents(animal, true);
        NativeFixer.TaskEnterVehicle(animal, vehicle, -1, DriverSeat, AnimalSprint, ResumeIfInterrupted | JackAnyone | BlockSeatShuffling, string.Empty);
        API.SetPedKeepTask(animal, true);

        run.Started();

        var stopAllCount = BullyState.StopAllCount;
        var giveUpAt = API.GetGameTimer() + EnterTimeoutMs;

        while (Vector3.Distance(API.GetEntityCoords(animal, false), API.GetEntityCoords(vehicle, false)) > AnimalReach
            && NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            if (API.GetGameTimer() > giveUpAt
                || stopAllCount != BullyState.StopAllCount
                || !API.DoesEntityExist(animal)
                || API.IsPedDeadOrDying(animal, true)
                || !API.DoesEntityExist(vehicle))
            {
                return;
            }

            await BaseScript.Delay(100);
        }

        var player = API.PlayerPedId();

        if (API.IsPedInVehicle(player, vehicle, false))
        {
            API.TaskLeaveVehicle(player, vehicle, DontCloseDoor);

            var outBy = API.GetGameTimer() + PlayerExitMs;

            while (API.IsPedInVehicle(player, vehicle, false) && API.GetGameTimer() < outBy)
            {
                await BaseScript.Delay(0);
            }
        }

        if (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            NativeFixer.TaskEnterVehicle(animal, vehicle, AnimalEnterMs, DriverSeat, Run, ResumeIfInterrupted | BlockSeatShuffling, string.Empty);
        }

        var seatedBy = API.GetGameTimer() + AnimalEnterMs + PlayerExitMs;

        while (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal && API.GetGameTimer() < seatedBy)
        {
            if (stopAllCount != BullyState.StopAllCount || !API.DoesEntityExist(animal) || !API.DoesEntityExist(vehicle))
            {
                return;
            }

            await BaseScript.Delay(100);
        }

        if (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) != animal)
        {
            if (!NativeFixer.IsVehicleSeatFree(vehicle, DriverSeat, false))
            {
                return;
            }

            API.SetPedIntoVehicle(animal, vehicle, DriverSeat);
        }

        API.SetVehicleEngineOn(vehicle, true, true, false);
        API.TaskVehicleDriveWander(animal, vehicle, GetawaySpeed, GetawayDriving);
        API.SetPedKeepTask(animal, true);
    }

    private static float Distance(int ped, Vector3 target) => Vector3.Distance(API.GetEntityCoords(ped, false), target);

    private static async Task<int> SpawnJackerAsync(Vector3 near)
    {
        var model = Streaming.Hash(BullyModels.Carjacker);

        if (!await Streaming.ModelAsync(model))
        {
            return 0;
        }

        if (SpawnSpots.FindSafeSpotAround(near, 15f, 30f, hidden: true) is not { } spot)
        {
            API.SetModelAsNoLongerNeeded(model);

            return 0;
        }

        var ped = API.CreatePed(HostilePeds.CivilianPedType, model, spot.X, spot.Y, spot.Z, Fx.HeadingTowards(spot, near), true, false);

        API.SetModelAsNoLongerNeeded(model);

        if (ped == 0)
        {
            return 0;
        }

        API.SetEntityAsMissionEntity(ped, true, true);

        BullySpawnCleanup.TrackForCleanup(ped);

        return ped;
    }

    private static async Task<bool> JackAsync(int ped, int vehicle)
    {
        if (!await NetworkEntity.TakeControlAsync(ped))
        {
            return false;
        }

        API.SetBlockingOfNonTemporaryEvents(ped, true);
        API.SetPedConfigFlag(ped, WillJackAnyPlayer, true);
        NativeFixer.TaskEnterVehicle(ped, vehicle, EnterTimeoutMs, DriverSeat, Run, EnterFlags, string.Empty);
        API.SetPedKeepTask(ped, true);

        return true;
    }

    // Whoever wins the driver seat takes off with it. Done by watching rather than with a task sequence,
    // which does not survive on ambient peds this client does not keep owning.
    private static async Task DriveOffAsync(List<int> jackers, int vehicle)
    {
        var giveUpAt = API.GetGameTimer() + EnterTimeoutMs;

        while (jackers.Count > 0 && API.GetGameTimer() < giveUpAt && API.DoesEntityExist(vehicle))
        {
            if (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0 && jackers.Contains(driver))
            {
                API.TaskVehicleDriveWander(driver, vehicle, GetawaySpeed, GetawayDriving);
                API.SetPedKeepTask(driver, true);

                return;
            }

            await BaseScript.Delay(250);
        }
    }
}

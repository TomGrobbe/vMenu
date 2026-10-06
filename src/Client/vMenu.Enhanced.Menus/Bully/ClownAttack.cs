using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

namespace vMenu.Enhanced.Menus.Bully;

internal static class ClownAttack
{
    private const int MaxClowns = 10;

    private const int MaxVans = 5;

    private const int DriverSeat = -1;

    private const int DurationMs = 5 * 60 * 1000;

    private const float UnloadDistance = 30f;

    private const int StuckUnloadMs = 25000;

    private const int CheckMs = 500;

    private const int RoadNodeFlags = 1;

    private const int ReorderMs = 4000;

    private const int BodiesLingerMs = 10000;

    private const int LaughOdds = 12;

    private const int RedriveMs = 3000;

    private const float DriveSpeed = 30f;

    private const int RushedDriving = 786468;

    private const string HonkLoop = "Honk_Loop";

    private const string CreepLoop = "Character_Loop";

    private const string MusicStart = "HALLOWEEN_START_MUSIC";

    private const string MusicStop = "HALLOWEEN_FAST_STOP_MUSIC";

    private static bool _active;

    public static async Task Start(BullyRun run)
    {
        if (_active)
        {
            run.Busy();

            return;
        }

        _active = true;

        var vans = new List<int>();
        var clowns = new List<int>();
        var blips = new List<int>();
        var unloaded = new HashSet<int>();
        var leaving = new HashSet<int>();
        var assigned = new Dictionary<int, int>();
        var attacking = new Dictionary<int, int>();
        var orderedAt = new Dictionary<int, int>();
        var loops = new Dictionary<int, int>();
        var bank = false;
        var music = false;

        try
        {
            var van = Streaming.Hash(BullyModels.ClownVan);
            var clown = Streaming.Hash(BullyModels.Clown);

            if (!await Streaming.ModelAsync(van) || !await Streaming.ModelAsync(clown))
            {
                run.Failed();

                return;
            }

            var stopAllCount = BullyState.StopAllCount;
            var victims = run.Victims;
            var position = Native.GetEntityCoords(Native.PlayerPedId(), false);
            var seats = Math.Max(1, Native.GetVehicleModelNumberOfSeats(van));
            var remaining = MaxClowns;

            for (var index = 0; index < MaxVans && remaining > 0; index++)
            {
                if (!Native.GetNthClosestVehicleNodeWithHeading(
                        position.X, position.Y, position.Z, 30 + index * 8, out var node, out var heading, out _, RoadNodeFlags, 3f, 0f))
                {
                    continue;
                }

                var vehicle = Native.CreateVehicle(van, node.X, node.Y, node.Z, heading, true, false, false);

                if (vehicle == 0)
                {
                    continue;
                }

                vans.Add(vehicle);

                Native.SetEntityAsMissionEntity(vehicle, true, true);
                Native.SetVehicleDoorsLockedForAllPlayers(vehicle, true);
                Native.SetVehicleEngineOn(vehicle, true, true, false);

                BullySpawnCleanup.TrackForCleanup(vehicle);

                for (var seat = DriverSeat; seat < seats - 1 && remaining > 0; seat++)
                {
                    if (SpawnClown(vehicle, clown, seat) is var ped and not 0)
                    {
                        assigned[ped] = victims[clowns.Count % victims.Count];

                        remaining--;

                        clowns.Add(ped);
                        blips.Add(HostilePeds.AddHostileBlip(ped));
                    }
                }

                if (Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0)
                {
                    Native.SetDriverAbility(driver, 1f);
                    Native.SetDriverAggressiveness(driver, 1f);

                    DriveTo(driver, vehicle, position);
                }
            }

            Native.SetModelAsNoLongerNeeded(van);
            Native.SetModelAsNoLongerNeeded(clown);

            if (clowns.Count == 0)
            {
                run.Failed();

                return;
            }

            run.Started();

            BullyTask.Run(ScreenPranks.ClownScareAsync, "ClownAttack.Scare");

            bank = await Streaming.AudioBankAsync(ScreenPranks.ClownBank);

            if (bank)
            {
                vans.ForEach(vehicle => StartLoop(loops, vehicle, HonkLoop));
            }

            music = Native.TriggerMusicEvent(MusicStart);

            var started = Native.GetGameTimer();
            var redriveAt = started + RedriveMs;
            var endsAt = started + DurationMs;

            while (stopAllCount == BullyState.StopAllCount && Native.GetGameTimer() < endsAt)
            {
                if (clowns.All(Dead))
                {
                    await API.Delay(BodiesLingerMs);

                    break;
                }

                var alive = new Dictionary<int, int>();

                foreach (var victim in victims)
                {
                    if (BullyRun.PedOf(victim) is var ped and not 0 && Native.DoesEntityExist(ped) && !Native.IsPedDeadOrDying(ped, true))
                    {
                        alive[victim] = ped;
                    }
                }

                var redrive = Native.GetGameTimer() >= redriveAt;

                if (redrive)
                {
                    redriveAt = Native.GetGameTimer() + RedriveMs;
                }

                foreach (var vehicle in vans)
                {
                    if (unloaded.Contains(vehicle) || !Native.DoesEntityExist(vehicle) || alive.Count == 0)
                    {
                        continue;
                    }

                    var at = Native.GetEntityCoords(vehicle, false);
                    var nearest = alive.Values
                        .Select(ped => Native.GetEntityCoords(ped, false))
                        .OrderBy(spot => Vector3.Distance(spot, at))
                        .First();

                    var close = Vector3.Distance(at, nearest) <= UnloadDistance;
                    var stuck = Native.GetGameTimer() - started > StuckUnloadMs && Native.GetEntitySpeed(vehicle) < 1f;

                    if (close || stuck)
                    {
                        unloaded.Add(vehicle);

                        StopLoop(loops, vehicle);

                        Native.SetVehicleEngineOn(vehicle, false, true, false);
                    }
                    else if (redrive && Native.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0)
                    {
                        DriveTo(driver, vehicle, nearest);
                    }
                }

                var spare = 0;

                foreach (var ped in clowns)
                {
                    if (Dead(ped))
                    {
                        StopLoop(loops, ped);

                        continue;
                    }

                    var vehicle = Native.GetVehiclePedIsIn(ped, false);

                    if (vehicle != 0)
                    {
                        if (unloaded.Contains(vehicle) && leaving.Add(ped))
                        {
                            Native.SetBlockingOfNonTemporaryEvents(ped, false);
                            Native.TaskLeaveVehicle(ped, vehicle, 256);
                        }

                        continue;
                    }

                    if (bank && !loops.ContainsKey(ped))
                    {
                        StartLoop(loops, ped, CreepLoop);
                    }

                    if (alive.Count == 0)
                    {
                        continue;
                    }

                    if (!alive.TryGetValue(assigned[ped], out var victim))
                    {
                        victim = alive.Values.ElementAt(spare++ % alive.Count);
                    }

                    var now = Native.GetGameTimer();

                    if (Dice.Next(LaughOdds) == 0)
                    {
                        Native.PlayPedAmbientSpeechWithVoiceNative(ped, ScreenPranks.ClownLaugh, ScreenPranks.ClownVoice, "SPEECH_PARAMS_FORCE", true);
                    }

                    if (!attacking.TryGetValue(ped, out var target)
                        || target != victim
                        || !orderedAt.TryGetValue(ped, out var ordered)
                        || (now - ordered > ReorderMs && !Native.IsPedInCombat(ped, victim)))
                    {
                        attacking[ped] = victim;
                        orderedAt[ped] = now;

                        HostilePeds.Attack(ped, victim);
                    }
                }

                await API.Delay(CheckMs);
            }
        }
        finally
        {
            foreach (var entity in loops.Keys.ToList())
            {
                StopLoop(loops, entity);
            }

            if (music)
            {
                Native.TriggerMusicEvent(MusicStop);
            }

            if (bank)
            {
                Streaming.ReleaseAudioBank(ScreenPranks.ClownBank);
            }

            blips.ForEach(HostilePeds.RemoveBlip);

            foreach (var entity in clowns.Concat(vans))
            {
                await BullySpawnCleanup.DeleteAsync(entity);
            }

            _active = false;
        }
    }

    private static void StartLoop(Dictionary<int, int> loops, int entity, string sound)
    {
        var id = Native.GetSoundId();

        Native.PlaySoundFromEntity(id, sound, entity, ScreenPranks.ClownSoundSet, true, 0);

        loops[entity] = id;
    }

    private static void StopLoop(Dictionary<int, int> loops, int entity)
    {
        if (!loops.Remove(entity, out var id))
        {
            return;
        }

        Native.StopSound(id);
        Native.ReleaseSoundId(id);
    }

    private static bool Dead(int ped) => !Native.DoesEntityExist(ped) || Native.IsPedDeadOrDying(ped, true);

    private static int SpawnClown(int vehicle, uint model, int seat)
    {
        var ped = Native.CreatePedInsideVehicle(vehicle, HostilePeds.CivilianPedType, model, seat, true, false);

        if (ped != 0)
        {
            Native.SetEntityAsMissionEntity(ped, true, true);
            Native.RemoveAllPedWeapons(ped, true);
            Native.SetPedRandomComponentVariation(ped, 0);

            Native.StopPedSpeaking(ped, true);
            Native.DisablePedPainAudio(ped, true);
            Native.SetBlockingOfNonTemporaryEvents(ped, true);
            Native.SetAmbientVoiceName(ped, ScreenPranks.ClownVoice);

            HostilePeds.MakeHostile(ped);
            BullySpawnCleanup.TrackForCleanup(ped);
        }

        return ped;
    }

    private static void DriveTo(int driver, int vehicle, Vector3 target) =>
        Native.TaskVehicleDriveToCoordLongrange(driver, vehicle, target.X, target.Y, target.Z, DriveSpeed, RushedDriving, UnloadDistance / 2f);
}

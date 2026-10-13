
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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
                API.SetModelAsNoLongerNeeded(van);
                API.SetModelAsNoLongerNeeded(clown);

                run.Failed();

                return;
            }

            var stopAllCount = BullyState.StopAllCount;
            var victims = run.Victims;
            var position = API.GetEntityCoords(API.PlayerPedId(), false);
            var seats = Math.Max(1, API.GetVehicleModelNumberOfSeats(van));
            var remaining = MaxClowns;

            for (var index = 0; index < MaxVans && remaining > 0; index++)
            {
                if (!NativeFixer.GetNthClosestVehicleNodeWithHeading(
                        position.X, position.Y, position.Z, 30 + index * 8, out var node, out var heading, out _, RoadNodeFlags, 3f, 0f))
                {
                    continue;
                }

                var vehicle = NativeFixer.CreateVehicle(van, node.X, node.Y, node.Z, heading, true, false, false);

                if (vehicle == 0)
                {
                    continue;
                }

                vans.Add(vehicle);

                API.SetEntityAsMissionEntity(vehicle, true, true);
                API.SetVehicleDoorsLockedForAllPlayers(vehicle, true);
                API.SetVehicleEngineOn(vehicle, true, true, false);

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

                if (NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0)
                {
                    API.SetDriverAbility(driver, 1f);
                    API.SetDriverAggressiveness(driver, 1f);

                    DriveTo(driver, vehicle, position);
                }
            }

            API.SetModelAsNoLongerNeeded(van);
            API.SetModelAsNoLongerNeeded(clown);

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

            music = API.TriggerMusicEvent(MusicStart);

            var started = API.GetGameTimer();
            var redriveAt = started + RedriveMs;
            var endsAt = started + DurationMs;

            while (stopAllCount == BullyState.StopAllCount && API.GetGameTimer() < endsAt)
            {
                if (clowns.All(Dead))
                {
                    await BaseScript.Delay(BodiesLingerMs);

                    break;
                }

                var alive = new Dictionary<int, int>();

                foreach (var victim in victims)
                {
                    if (BullyRun.PedOf(victim) is var ped and not 0 && API.DoesEntityExist(ped) && !API.IsPedDeadOrDying(ped, true))
                    {
                        alive[victim] = ped;
                    }
                }

                var redrive = API.GetGameTimer() >= redriveAt;

                if (redrive)
                {
                    redriveAt = API.GetGameTimer() + RedriveMs;
                }

                foreach (var vehicle in vans)
                {
                    if (unloaded.Contains(vehicle) || !API.DoesEntityExist(vehicle) || alive.Count == 0)
                    {
                        continue;
                    }

                    var at = API.GetEntityCoords(vehicle, false);
                    var nearest = alive.Values
                        .Select(ped => API.GetEntityCoords(ped, false))
                        .OrderBy(spot => Vector3.Distance(spot, at))
                        .First();

                    var close = Vector3.Distance(at, nearest) <= UnloadDistance;
                    var stuck = API.GetGameTimer() - started > StuckUnloadMs && API.GetEntitySpeed(vehicle) < 1f;

                    if (close || stuck)
                    {
                        unloaded.Add(vehicle);

                        StopLoop(loops, vehicle);

                        API.SetVehicleEngineOn(vehicle, false, true, false);
                    }
                    else if (redrive && NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false) is var driver and not 0)
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

                    var vehicle = API.GetVehiclePedIsIn(ped, false);

                    if (vehicle != 0)
                    {
                        if (unloaded.Contains(vehicle) && leaving.Add(ped))
                        {
                            API.SetBlockingOfNonTemporaryEvents(ped, false);
                            API.TaskLeaveVehicle(ped, vehicle, 256);
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

                    var now = API.GetGameTimer();

                    if (Dice.Next(LaughOdds) == 0)
                    {
                        API.PlayPedAmbientSpeechWithVoiceNative(ped, ScreenPranks.ClownLaugh, ScreenPranks.ClownVoice, "SPEECH_PARAMS_FORCE", true);
                    }

                    if (!attacking.TryGetValue(ped, out var target)
                        || target != victim
                        || !orderedAt.TryGetValue(ped, out var ordered)
                        || (now - ordered > ReorderMs && !API.IsPedInCombat(ped, victim)))
                    {
                        attacking[ped] = victim;
                        orderedAt[ped] = now;

                        HostilePeds.Attack(ped, victim);
                    }
                }

                await BaseScript.Delay(CheckMs);
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
                API.TriggerMusicEvent(MusicStop);
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
        var id = API.GetSoundId();

        API.PlaySoundFromEntity(id, sound, entity, ScreenPranks.ClownSoundSet, true, 0);

        loops[entity] = id;
    }

    private static void StopLoop(Dictionary<int, int> loops, int entity)
    {
        if (!loops.Remove(entity, out var id))
        {
            return;
        }

        API.StopSound(id);
        API.ReleaseSoundId(id);
    }

    private static bool Dead(int ped) => !API.DoesEntityExist(ped) || API.IsPedDeadOrDying(ped, true);

    private static int SpawnClown(int vehicle, uint model, int seat)
    {
        var ped = API.CreatePedInsideVehicle(vehicle, HostilePeds.CivilianPedType, model, seat, true, false);

        if (ped != 0)
        {
            API.SetEntityAsMissionEntity(ped, true, true);
            API.RemoveAllPedWeapons(ped, true);
            NativeFixer.SetPedRandomComponentVariation(ped, 0);

            API.StopPedSpeaking(ped, true);
            API.DisablePedPainAudio(ped, true);
            API.SetBlockingOfNonTemporaryEvents(ped, true);
            API.SetAmbientVoiceName(ped, ScreenPranks.ClownVoice);

            HostilePeds.MakeHostile(ped);
            BullySpawnCleanup.TrackForCleanup(ped);
        }

        return ped;
    }

    private static void DriveTo(int driver, int vehicle, Vector3 target) =>
        API.TaskVehicleDriveToCoordLongrange(driver, vehicle, target.X, target.Y, target.Z, DriveSpeed, RushedDriving, UnloadDistance / 2f);
}

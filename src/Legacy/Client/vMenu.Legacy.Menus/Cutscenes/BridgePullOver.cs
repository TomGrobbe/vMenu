using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Bully;

using NoClipState = vMenu.Enhanced.NoClip.NoClip;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Cutscenes;

internal static class BridgePullOver
{
    private const string Cutscene = "Car_steal_3_mcs_3";

    private const string RacerHandle = "Racer_that_runsaway";

    private const string RacerModel = "IG_CAR3GUY2";

    private const int AnimateExistingEntity = 0;

    private const int IgnoreModelName = 64;

    private const int DontApplyVariations = 1;

    private const int RequestedInMission = 8;

    private const int NoWidescreenBorders = 8;

    private const int FadeMs = 500;

    private const int CutsceneLoadTimeoutMs = 20000;

    private const int CutsceneStartTimeoutMs = 5000;

    private const int WorldMoveTimeoutMs = 5000;

    private const int LoadSceneTimeoutMs = 8000;

    private const int NoClipReleaseTimeoutMs = 2000;

    private const float LoadSceneRadius = 60f;

    private const int CivilianPedType = 4;

    private const int DriverSeat = -1;

    private const int HeadProp = 0;

    private const int MichaelTakesHelmetOffMs = 46687;

    private const string Michael = "Michael";

    private const string Trevor = "Trevor";

    private const string Franklin = "Franklin";

    private const string OtherRacer = "Racer_that_dies";

    private const string FranklinsCar = "Franklins_car";

    private const string EntityXf = "Car_Racer_runsaway";

    private const string Cheetah = "Car_Racer_dies";

    private const string TrevorsBike = "Trevors_police_bike";

    private const string MichaelsBike = "Michaels_Police_bike";

    private static (Vector3 Position, float Heading) Bridge => (new(-1921.26f, 4590.63f, 56.03f), 141.45f);

    private static readonly (string Handle, string Model, Vector3 Position, float Heading, (int Slot, int Drawable)[] Clothes, (int Slot, int Drawable)[] Props)[] Cast =
    [
        (Michael, "player_zero", new(-1917.87f, 4608.78f, 56.05f), 135.06f,
            [(3, 10), (4, 9), (5, 0), (6, 1), (7, 0), (8, 0), (9, 6), (10, 3), (11, 0)], [(HeadProp, 4)]),
        (Trevor, "player_two", new(-1921.37f, 4604.54f, 56.06f), 135.03f,
            [(3, 7), (4, 7), (5, 0), (6, 1), (7, 0), (8, 14), (9, 3), (10, 3), (11, 0)], [(HeadProp, 5)]),
        (Franklin, "player_one", new(-1910.99f, 4615.92f, 56.05f), 23.97f,
            [(3, 8), (4, 8), (5, 0), (6, 6), (7, 0), (8, 14), (9, 0), (10, 0), (11, 0)], []),
        (OtherRacer, "IG_CAR3GUY1", new(-1915.63f, 4600.51f, 56.03f), 141.45f, [], []),
    ];

    private static readonly (string Handle, string Model, Vector3 Position, float Heading)[] Vehicles =
    [
        (FranklinsCar, "F620", new(-1912.55f, 4616.36f, 56.04f), 134.67f),
        (EntityXf, "ENTITYXF", new(-1923.02f, 4605.49f, 56.04f), 135.03f),
        (Cheetah, "CHEETAH", new(-1919.33f, 4609.29f, 56.05f), 135.08f),
        (TrevorsBike, "POLICEB", new(-1909.11f, 4619.60f, 56.04f), 135.71f),
        (MichaelsBike, "POLICEB", new(-1907.50f, 4618.12f, 56.05f), 137.06f),
    ];

    private static readonly Dictionary<string, int> Spawned = new(StringComparer.Ordinal);

    private static readonly HashSet<string> Exited = new(StringComparer.Ordinal);

    private static TaskCompletionSource<bool>? _worldMove;

    private static (Vector3 Position, float Heading)? _origin;

    private static bool _active;

    private static bool _noClipReleased;

    public static void Initialize()
    {
        Script.EventHandlers[BullyEvents.PrivateWorldMoved] += new Action<string>(OnWorldMoved);

        ResourceShutdown.Stopping += OnShutdown;
    }

    private static void OnWorldMoved(string state) => _worldMove?.TrySetResult(state == BullyEvents.On);

    private static void OnShutdown()
    {
        if (!_active)
        {
            return;
        }

        API.StopCutsceneImmediately();
        API.RemoveCutscene();

        DeleteSpawned();

        if (_origin is { } origin)
        {
            API.SetEntityCoords(API.PlayerPedId(), origin.Position.X, origin.Position.Y, origin.Position.Z, false, false, false, true);
        }

        API.DoScreenFadeIn(0);
    }

    public static async Task PlayAsync(int ped, Func<bool> stopped, Action started, Action failed)
    {
        _active = true;

        SceneLock.Take();
        BullyState.SetPaused(true);

        var askedForPrivateWorld = false;
        var playing = false;

        try
        {
            await FadeOutAsync();

            await LeaveNoClipAsync();

            _origin = (API.GetEntityCoords(ped, false), API.GetEntityHeading(ped));

            askedForPrivateWorld = true;

            if (!await MoveWorldAsync(toPrivate: true) || stopped())
            {
                return;
            }

            await PlaceAsync(ped, Bridge);

            if (!await LoadAsync())
            {
                return;
            }

            if (!await SpawnAsync())
            {
                return;
            }

            foreach (var (handle, entity) in Spawned)
            {
                API.RegisterEntityForCutscene(entity, handle, AnimateExistingEntity, 0, 0);
            }

            API.RegisterEntityForCutscene(ped, RacerHandle, AnimateExistingEntity, Streaming.Hash(RacerModel), IgnoreModelName);

            API.SetCurrentPedWeapon(ped, Streaming.Hash("WEAPON_UNARMED"), true);

            API.StartCutscene(NoWidescreenBorders);

            if (!await WaitAsync(API.IsCutscenePlaying, CutsceneStartTimeoutMs))
            {
                return;
            }

            playing = true;

            started();

            API.DoScreenFadeIn(FadeMs);

            await WatchAsync(ped, stopped);
        }
        finally
        {
            if (!playing)
            {
                failed();
            }

            await FinishAsync(ped, askedForPrivateWorld);
        }
    }

    private static async Task<bool> LoadAsync()
    {
        API.RequestCutscene(Cutscene, RequestedInMission);

        var started = API.GetGameTimer();
        var clothesLocked = false;

        while (!API.HasCutsceneLoaded())
        {
            if (!clothesLocked && API.CanRequestAssetsForCutsceneEntity())
            {
                NativeFixer.SetCutsceneEntityStreamingFlags(RacerHandle, 0, DontApplyVariations);

                foreach (var member in Cast)
                {
                    NativeFixer.SetCutsceneEntityStreamingFlags(member.Handle, 0, DontApplyVariations);
                }

                clothesLocked = true;
            }

            if (API.GetGameTimer() - started > CutsceneLoadTimeoutMs)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        return true;
    }

    private static async Task<bool> SpawnAsync()
    {
        var models = Cast.Select(member => member.Model).Concat(Vehicles.Select(vehicle => vehicle.Model)).Distinct(StringComparer.Ordinal).ToList();

        try
        {
            foreach (var model in models)
            {
                if (!await Streaming.ModelAsync(model))
                {
                    return false;
                }
            }

            foreach (var (handle, model, position, heading) in Vehicles)
            {
                var vehicle = NativeFixer.CreateVehicle(Streaming.Hash(model), position.X, position.Y, position.Z, heading, false, false, false);

                Spawned[handle] = vehicle;

                API.SetVehicleEngineOn(vehicle, false, true, false);
            }

            API.SetVehicleSiren(Spawned[TrevorsBike], true);
            API.SetVehicleSiren(Spawned[MichaelsBike], true);

            foreach (var (handle, model, position, heading, clothes, props) in Cast)
            {
                var member = API.CreatePed(CivilianPedType, Streaming.Hash(model), position.X, position.Y, position.Z, heading, false, false);

                Spawned[handle] = member;

                API.SetBlockingOfNonTemporaryEvents(member, true);
                API.SetPedDefaultComponentVariation(member);

                foreach (var (slot, drawable) in clothes)
                {
                    API.SetPedComponentVariation(member, slot, drawable, 0, 0);
                }

                foreach (var (slot, drawable) in props)
                {
                    NativeFixer.SetPedPropIndex(member, slot, drawable, 0, true, false);
                }
            }

            return true;
        }
        finally
        {
            foreach (var model in models)
            {
                API.SetModelAsNoLongerNeeded(Streaming.Hash(model));
            }
        }
    }

    private static void SetExitStates(int ped)
    {
        if (API.GetCutsceneTime() > MichaelTakesHelmetOffMs)
        {
            NativeFixer.ClearPedProp(Spawned[Michael], HeadProp, false);
        }

        foreach (var (handle, entity) in Spawned)
        {
            if (Exited.Contains(handle) || !API.CanSetExitStateForRegisteredEntity(handle, 0))
            {
                continue;
            }

            Exited.Add(handle);

            switch (handle)
            {
                case Michael:
                    NativeFixer.ClearPedProp(entity, HeadProp, false);
                    API.SetPedIntoVehicle(entity, Spawned[EntityXf], DriverSeat);
                    break;
                case Trevor:
                    NativeFixer.ClearPedProp(entity, HeadProp, false);
                    API.SetPedIntoVehicle(entity, Spawned[Cheetah], DriverSeat);
                    break;
                case Franklin:
                    API.SetPedIntoVehicle(entity, Spawned[FranklinsCar], DriverSeat);
                    break;
                case OtherRacer:
                    API.SetEntityCoords(entity, -1915.6257f, 4600.5103f, 56.0301f, false, false, false, true);
                    API.SetEntityHeading(entity, 141.45f);
                    API.TaskSmartFleePed(entity, ped, 500f, -1, false, false);
                    API.SetPedKeepTask(entity, true);
                    break;
                case FranklinsCar or EntityXf or Cheetah:
                    API.SetVehicleDoorsShut(entity, false);
                    break;
                case TrevorsBike or MichaelsBike:
                    var (_, _, position, heading) = Vehicles.First(vehicle => vehicle.Handle == handle);
                    API.SetEntityCoords(entity, position.X, position.Y, position.Z, false, false, false, true);
                    API.SetEntityHeading(entity, heading);
                    break;
            }
        }
    }

    private static void DeleteSpawned()
    {
        foreach (var entity in Spawned.Values)
        {
            var doomed = entity;

            if (API.DoesEntityExist(doomed))
            {
                NativeFixer.DeleteEntity(ref doomed);
            }
        }

        Spawned.Clear();
        Exited.Clear();
    }

    private static async Task WatchAsync(int ped, Func<bool> stopped)
    {
        while (API.IsCutsceneActive() && !stopped())
        {
            SetExitStates(ped);

            if (API.CanSetExitStateForCamera(false))
            {
                API.SetGameplayCamRelativeHeading(0f);
                API.SetGameplayCamRelativePitch(0f, 1f);
            }

            await BaseScript.Delay(0);
        }
    }

    private static async Task FinishAsync(int ped, bool leavePrivateWorld)
    {
        try
        {
            await FadeOutAsync();

            if (API.IsCutsceneActive())
            {
                API.StopCutsceneImmediately();
            }

            API.RemoveCutscene();

            DeleteSpawned();

            if (_origin is { } origin)
            {
                await PlaceAsync(ped, origin);
            }

            if (leavePrivateWorld)
            {
                await MoveWorldAsync(toPrivate: false);
            }
        }
        finally
        {
            _origin = null;

            API.DoScreenFadeIn(FadeMs);

            SceneLock.Release();
            BullyState.SetPaused(false);

            _active = false;
        }
    }

    private static async Task LeaveNoClipAsync()
    {
        if (!NoClipState.IsActive)
        {
            return;
        }

        _noClipReleased = false;

        NoClipState.EntityReleased += OnNoClipReleased;

        try
        {
            NoClipState.Disable();

            await WaitAsync(() => _noClipReleased, NoClipReleaseTimeoutMs);
        }
        finally
        {
            NoClipState.EntityReleased -= OnNoClipReleased;
        }
    }

    private static void OnNoClipReleased(int entity) => _noClipReleased = true;

    private static async Task<bool> MoveWorldAsync(bool toPrivate)
    {
        var move = new TaskCompletionSource<bool>();

        _worldMove = move;

        BaseScript.TriggerServerEvent(BullyEvents.PrivateWorld, toPrivate ? BullyEvents.On : BullyEvents.Off);

        await WaitAsync(() => move.Task.IsCompleted, WorldMoveTimeoutMs);

        _worldMove = null;

        return move.Task.IsCompleted && move.Task.Result == toPrivate;
    }

    private static async Task PlaceAsync(int ped, (Vector3 Position, float Heading) spot)
    {
        var (position, heading) = spot;

        API.SetEntityCoords(ped, position.X, position.Y, position.Z, false, false, false, true);
        API.SetEntityHeading(ped, heading);

        API.NewLoadSceneStartSphere(position.X, position.Y, position.Z, LoadSceneRadius, 0);

        await WaitAsync(() => !API.IsNewLoadSceneActive() || API.IsNewLoadSceneLoaded(), LoadSceneTimeoutMs);

        API.NewLoadSceneStop();
    }

    private static async Task FadeOutAsync()
    {
        if (API.IsScreenFadedOut())
        {
            return;
        }

        API.DoScreenFadeOut(FadeMs);

        await WaitAsync(API.IsScreenFadedOut, FadeMs * 4);
    }

    private static async Task<bool> WaitAsync(Func<bool> done, int timeoutMs)
    {
        var started = API.GetGameTimer();

        while (!done())
        {
            if (API.GetGameTimer() - started > timeoutMs)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        return true;
    }
}

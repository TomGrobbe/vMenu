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

internal static class HaircutRoast
{
    private const string Cutscene = "arm_1_mcs_2_concat";

    private const string FranklinHandle = "Franklin";

    private const string FranklinModel = "player_one";

    private const string TextBlock = "ARM1";

    private const string BarberHelp = "AR1_BARBERS";

    private const int BarberHelpAtMs = 21500;

    private const int BarberHelpMs = 7000;

    private const int MissionTextSlot = 0;

    private const int AnimateExistingEntity = 0;

    private const int IgnoreModelName = 64;

    private const int DontApplyVariations = 1;

    private const int RequestedInMission = 8;

    private const int NoWidescreenBorders = 8;

    private const int DoorUnlocked = 0;

    private const int DoorLocked = 1;

    private const int FadeMs = 500;

    private const int CutsceneLoadTimeoutMs = 20000;

    private const int CutsceneStartTimeoutMs = 5000;

    private const int WorldMoveTimeoutMs = 5000;

    private const int LoadSceneTimeoutMs = 8000;

    private const int NoClipReleaseTimeoutMs = 2000;

    private const float LoadSceneRadius = 40f;

    private const float ModelHideRadius = 3f;

    private const string InteriorType = "v_franklins";

    private const int InteriorReadyTimeoutMs = 5000;

    private static (Vector3 Position, float Heading) Driveway => (new(-25.4559f, -1426.9977f, 29.6560f), 0f);

    private static readonly (string Model, string Handle, Vector3 Position)[] SceneProps =
    [
        ("p_cs_shirt_01_s", "Franklins_shirt", new(-11.700f, -1439.255f, 30.099f)),
        ("v_ilev_fa_warddoorl", "Closet_Door_L", new(-12.700f, -1439.255f, 20.099f)),
        ("v_ilev_fa_warddoorr", "Closet_Door_R", new(-10.700f, -1439.255f, 20.099f)),
    ];

    private static readonly (string Model, Vector3 Position)[] ModelHides =
    [
        ("v_ilev_frnkwarddr1", new(-18.3539f, -1438.7838f, 31.3050f)),
        ("v_ilev_frnkwarddr2", new(-18.3594f, -1438.1329f, 31.3050f)),
    ];

    private static readonly (string Name, string Model, Vector3 Position)[] Doors =
    [
        ("vmenu_haircut_front_door", "v_ilev_fa_frontdoor", new(-14f, -1441f, 31f)),
        ("vmenu_haircut_back_door", "v_ilev_fh_frntdoor", new(-15f, -1427f, 31f)),
    ];

    private static readonly (string Name, bool Active)[] StoryEntitySets =
    [
        ("V_57_FranklinStuff", true),
        ("V_57_Franklin_LEFT", false),
        ("V_57_GangBandana", false),
        ("V_57_Safari", false),
    ];

    private static readonly List<(string Handle, int Entity)> Spawned = [];

    private static readonly List<(string Name, bool Active)> PreviousEntitySets = [];

    private static int _interior;

    private static readonly List<(uint Door, bool Ours, int Previous)> OpenedDoors = [];

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

        var ped = API.PlayerPedId();

        API.StopCutsceneImmediately();
        API.RemoveCutscene();

        CleanUp();

        if (_origin is { } origin)
        {
            API.SetEntityCoords(ped, origin.Position.X, origin.Position.Y, origin.Position.Z, false, false, false, true);
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

            await PlaceAsync(ped, Driveway);

            await DressInteriorAsync();

            if (!await LoadAsync())
            {
                return;
            }

            RegisterAndStart(ped);

            if (!await WaitAsync(API.IsCutscenePlaying, CutsceneStartTimeoutMs))
            {
                return;
            }

            playing = true;

            started();

            API.DoScreenFadeIn(FadeMs);

            await WatchAsync(stopped);
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
        API.RequestAdditionalText(TextBlock, MissionTextSlot);
        API.RequestCutscene(Cutscene, RequestedInMission);

        var started = API.GetGameTimer();
        var clothesLocked = false;

        while (!API.HasCutsceneLoaded())
        {
            if (!clothesLocked && API.CanRequestAssetsForCutsceneEntity())
            {
                NativeFixer.SetCutsceneEntityStreamingFlags(FranklinHandle, 0, DontApplyVariations);

                clothesLocked = true;
            }

            if (API.GetGameTimer() - started > CutsceneLoadTimeoutMs)
            {
                return false;
            }

            await BaseScript.Delay(0);
        }

        foreach (var (model, _, _) in SceneProps)
        {
            if (!await Streaming.ModelAsync(model))
            {
                foreach (var (loaded, _, _) in SceneProps)
                {
                    API.SetModelAsNoLongerNeeded(Streaming.Hash(loaded));
                }

                return false;
            }
        }

        return true;
    }

    private static void RegisterAndStart(int ped)
    {
        foreach (var (model, handle, position) in SceneProps)
        {
            var hash = Streaming.Hash(model);
            var entity = NativeFixer.CreateObject(hash, position.X, position.Y, position.Z, false, false, false);

            API.SetModelAsNoLongerNeeded(hash);

            Spawned.Add((handle, entity));

            API.RegisterEntityForCutscene(entity, handle, AnimateExistingEntity, 0, 0);
        }

        API.RegisterEntityForCutscene(ped, FranklinHandle, AnimateExistingEntity, Streaming.Hash(FranklinModel), IgnoreModelName);

        foreach (var (model, position) in ModelHides)
        {
            API.CreateModelHide(position.X, position.Y, position.Z, ModelHideRadius, Streaming.Hash(model), false);
        }

        UnlockDoors();

        API.SetCurrentPedWeapon(ped, Streaming.Hash("WEAPON_UNARMED"), true);

        API.StartCutscene(NoWidescreenBorders);
    }

    private static async Task WatchAsync(Func<bool> stopped)
    {
        var barberHelpShown = false;

        while (API.IsCutsceneActive() && !stopped())
        {
            if (!barberHelpShown
                && API.GetCutsceneTime() > BarberHelpAtMs
                && API.HasAdditionalTextLoaded(MissionTextSlot))
            {
                API.BeginTextCommandDisplayHelp(BarberHelp);
                API.EndTextCommandDisplayHelp(0, false, true, BarberHelpMs);

                barberHelpShown = true;
            }

            foreach (var (handle, entity) in Spawned)
            {
                if (API.CanSetExitStateForRegisteredEntity(handle, 0))
                {
                    API.FreezeEntityPosition(entity, true);
                }
            }

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

            CleanUp();

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

    private static void CleanUp()
    {
        foreach (var (_, entity) in Spawned)
        {
            if (API.DoesEntityExist(entity))
            {
                NativeFixer.DeleteObject(entity);
            }
        }

        Spawned.Clear();

        foreach (var (model, position) in ModelHides)
        {
            NativeFixer.RemoveModelHide(position.X, position.Y, position.Z, ModelHideRadius, Streaming.Hash(model), false);
        }

        API.ClearAdditionalText(MissionTextSlot, false);

        RestoreDoors();

        RestoreInterior();
    }

    private static async Task DressInteriorAsync()
    {
        var interior = API.GetInteriorAtCoordsWithType(-13.9623f, -1440.6136f, 30.1015f, InteriorType);

        if (interior == 0)
        {
            return;
        }

        _interior = interior;

        API.PinInteriorInMemory(interior);

        await WaitAsync(() => API.IsInteriorReady(interior), InteriorReadyTimeoutMs);

        foreach (var (name, active) in StoryEntitySets)
        {
            PreviousEntitySets.Add((name, API.IsInteriorEntitySetActive(interior, name)));

            SetEntitySet(interior, name, active);
        }

        API.RefreshInterior(interior);
    }

    private static void RestoreInterior()
    {
        if (_interior == 0)
        {
            return;
        }

        foreach (var (name, active) in PreviousEntitySets)
        {
            SetEntitySet(_interior, name, active);
        }

        PreviousEntitySets.Clear();

        API.RefreshInterior(_interior);
        API.UnpinInterior(_interior);

        _interior = 0;
    }

    private static void SetEntitySet(int interior, string name, bool active)
    {
        if (active)
        {
            API.ActivateInteriorEntitySet(interior, name);
        }
        else
        {
            API.DeactivateInteriorEntitySet(interior, name);
        }
    }

    private static void UnlockDoors()
    {
        foreach (var (name, model, position) in Doors)
        {
            var modelHash = Streaming.Hash(model);
            if (NativeFixer.DoorSystemFindExistingDoor(position.X, position.Y, position.Z, modelHash, out var existing))
            {
                var door = (uint)existing;

                OpenedDoors.Add((door, false, API.DoorSystemGetDoorState(door)));

                API.DoorSystemSetDoorState(door, DoorUnlocked, false, true);

                continue;
            }

            var locked = NativeFixer.IsClosestDoorOfTypeLocked(modelHash, position.X, position.Y, position.Z);

            var ours = Streaming.Hash(name);

            NativeFixer.AddDoorToSystem(ours, modelHash, position.X, position.Y, position.Z, false, false, false, false);
            API.DoorSystemSetDoorState(ours, DoorUnlocked, false, true);

            OpenedDoors.Add((ours, true, locked ? DoorLocked : DoorUnlocked));
        }
    }

    private static void RestoreDoors()
    {
        foreach (var (door, ours, previous) in OpenedDoors)
        {
            API.DoorSystemSetDoorState(door, previous, false, true);

            if (ours)
            {
                NativeFixer.RemoveDoorFromSystem(door, previous == DoorLocked);
            }
        }

        OpenedDoors.Clear();
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

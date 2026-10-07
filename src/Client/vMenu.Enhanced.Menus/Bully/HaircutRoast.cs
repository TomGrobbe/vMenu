using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;

using NoClipState = vMenu.Enhanced.NoClip.NoClip;

namespace vMenu.Enhanced.Menus.Bully;

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

    public static bool IsActive => _active;

    public static void Initialize()
    {
        API.OnNetEvent(BullyEvents.PrivateWorldMoved, new Action<string>(OnWorldMoved), false);

        ResourceShutdown.Stopping += OnShutdown;
    }

    private static void OnWorldMoved(string state) => _worldMove?.TrySetResult(state == BullyEvents.On);

    private static void OnShutdown()
    {
        if (!_active)
        {
            return;
        }

        var ped = Native.PlayerPedId();

        Native.StopCutsceneImmediately();
        Native.RemoveCutscene();

        CleanUp();

        if (_origin is { } origin)
        {
            Native.SetEntityCoords(ped, origin.Position.X, origin.Position.Y, origin.Position.Z, false, false, false, true);
        }

        Native.DoScreenFadeIn(0);
    }

    public static async Task Start(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        if (_active || Relocation.IsAway || AlienAbduction.IsActive)
        {
            run.Busy();

            return;
        }

        if (!Native.IsPedOnFoot(ped))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _active = true;

        SceneLock.Take();
        BullyState.SetPaused(true);

        var stopAllCount = BullyState.StopAllCount;
        var askedForPrivateWorld = false;

        try
        {
            await FadeOutAsync();

            await LeaveNoClipAsync();

            _origin = (Native.GetEntityCoords(ped, false), Native.GetEntityHeading(ped));

            askedForPrivateWorld = true;

            if (!await MoveWorldAsync(toPrivate: true) || stopAllCount != BullyState.StopAllCount)
            {
                run.Failed();

                return;
            }

            await PlaceAsync(ped, Driveway);

            await DressInteriorAsync();

            if (!await LoadAsync())
            {
                run.Failed();

                return;
            }

            RegisterAndStart(ped);

            if (!await WaitAsync(Native.IsCutscenePlaying, CutsceneStartTimeoutMs))
            {
                run.Failed();

                return;
            }

            run.Started();

            Native.DoScreenFadeIn(FadeMs);

            await PlayAsync(stopAllCount);
        }
        finally
        {
            await FinishAsync(ped, askedForPrivateWorld);
        }
    }

    private static async Task<bool> LoadAsync()
    {
        Native.RequestAdditionalText(TextBlock, MissionTextSlot);
        Native.RequestCutscene(Cutscene, RequestedInMission);

        var started = Native.GetGameTimer();
        var clothesLocked = false;

        while (!Native.HasCutsceneLoaded())
        {
            if (!clothesLocked && Native.CanRequestAssetsForCutsceneEntity())
            {
                Native.SetCutsceneEntityStreamingFlags(FranklinHandle, 0, DontApplyVariations);

                clothesLocked = true;
            }

            if (Native.GetGameTimer() - started > CutsceneLoadTimeoutMs)
            {
                return false;
            }

            await API.Delay(0);
        }

        foreach (var (model, _, _) in SceneProps)
        {
            if (!await Streaming.ModelAsync(model))
            {
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
            var entity = Native.CreateObject(hash, position.X, position.Y, position.Z, false, false, false);

            Native.SetModelAsNoLongerNeeded(hash);

            Spawned.Add((handle, entity));

            Native.RegisterEntityForCutscene(entity, handle, AnimateExistingEntity, 0, 0);
        }

        Native.RegisterEntityForCutscene(ped, FranklinHandle, AnimateExistingEntity, Streaming.Hash(FranklinModel), IgnoreModelName);

        foreach (var (model, position) in ModelHides)
        {
            Native.CreateModelHide(position.X, position.Y, position.Z, ModelHideRadius, Streaming.Hash(model), false);
        }

        UnlockDoors();

        Native.SetCurrentPedWeapon(ped, Streaming.Hash("WEAPON_UNARMED"), true);

        Native.StartCutscene(NoWidescreenBorders);
    }

    private static async Task PlayAsync(int stopAllCount)
    {
        var barberHelpShown = false;

        while (Native.IsCutsceneActive() && stopAllCount == BullyState.StopAllCount)
        {
            if (!barberHelpShown
                && Native.GetCutsceneTime() > BarberHelpAtMs
                && Native.HasAdditionalTextLoaded(MissionTextSlot))
            {
                Native.BeginTextCommandDisplayHelp(BarberHelp);
                Native.EndTextCommandDisplayHelp(0, false, true, BarberHelpMs);

                barberHelpShown = true;
            }

            foreach (var (handle, entity) in Spawned)
            {
                if (Native.CanSetExitStateForRegisteredEntity(handle, 0))
                {
                    Native.FreezeEntityPosition(entity, true);
                }
            }

            if (Native.CanSetExitStateForCamera(false))
            {
                Native.SetGameplayCamRelativeHeading(0f);
                Native.SetGameplayCamRelativePitch(0f, 1f);
            }

            await API.Delay(0);
        }
    }

    private static async Task FinishAsync(int ped, bool leavePrivateWorld)
    {
        try
        {
            await FadeOutAsync();

            if (Native.IsCutsceneActive())
            {
                Native.StopCutsceneImmediately();
            }

            Native.RemoveCutscene();

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

            Native.DoScreenFadeIn(FadeMs);

            SceneLock.Release();
            BullyState.SetPaused(false);

            _active = false;
        }
    }

    private static void CleanUp()
    {
        foreach (var (_, entity) in Spawned)
        {
            if (Native.DoesEntityExist(entity))
            {
                Native.DeleteObject(entity);
            }
        }

        Spawned.Clear();

        foreach (var (model, position) in ModelHides)
        {
            Native.RemoveModelHide(position.X, position.Y, position.Z, ModelHideRadius, Streaming.Hash(model), false);
        }

        Native.ClearAdditionalText(MissionTextSlot, false);

        RestoreDoors();

        RestoreInterior();
    }

    private static async Task DressInteriorAsync()
    {
        var interior = Native.GetInteriorAtCoordsWithType(-13.9623f, -1440.6136f, 30.1015f, InteriorType);

        if (interior == 0)
        {
            return;
        }

        _interior = interior;

        Native.PinInteriorInMemory(interior);

        await WaitAsync(() => Native.IsInteriorReady(interior), InteriorReadyTimeoutMs);

        foreach (var (name, active) in StoryEntitySets)
        {
            PreviousEntitySets.Add((name, Native.IsInteriorEntitySetActive(interior, name)));

            SetEntitySet(interior, name, active);
        }

        Native.RefreshInterior(interior);
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

        Native.RefreshInterior(_interior);
        Native.UnpinInterior(_interior);

        _interior = 0;
    }

    private static void SetEntitySet(int interior, string name, bool active)
    {
        if (active)
        {
            Native.ActivateInteriorEntitySet(interior, name);
        }
        else
        {
            Native.DeactivateInteriorEntitySet(interior, name);
        }
    }

    private static void UnlockDoors()
    {
        foreach (var (name, model, position) in Doors)
        {
            var modelHash = Streaming.Hash(model);
            if (Native.DoorSystemFindExistingDoor(position.X, position.Y, position.Z, modelHash, out var existing))
            {
                var door = (uint)existing;

                OpenedDoors.Add((door, false, Native.DoorSystemGetDoorState(door)));

                Native.DoorSystemSetDoorState(door, DoorUnlocked, false, true);

                continue;
            }

            var locked = NativeFixer.IsClosestDoorOfTypeLocked(modelHash, position.X, position.Y, position.Z);

            var ours = Streaming.Hash(name);

            Native.AddDoorToSystem(ours, modelHash, position.X, position.Y, position.Z, false, false, false, false);
            Native.DoorSystemSetDoorState(ours, DoorUnlocked, false, true);

            OpenedDoors.Add((ours, true, locked ? DoorLocked : DoorUnlocked));
        }
    }

    private static void RestoreDoors()
    {
        foreach (var (door, ours, previous) in OpenedDoors)
        {
            Native.DoorSystemSetDoorState(door, previous, false, true);

            if (ours)
            {
                Native.RemoveDoorFromSystem(door, previous == DoorLocked);
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

        API.EmitServer(BullyEvents.PrivateWorld, toPrivate ? BullyEvents.On : BullyEvents.Off);

        await WaitAsync(() => move.Task.IsCompleted, WorldMoveTimeoutMs);

        _worldMove = null;

        return move.Task.IsCompleted && move.Task.Result == toPrivate;
    }

    private static async Task PlaceAsync(int ped, (Vector3 Position, float Heading) spot)
    {
        var (position, heading) = spot;

        Native.SetEntityCoords(ped, position.X, position.Y, position.Z, false, false, false, true);
        Native.SetEntityHeading(ped, heading);

        Native.NewLoadSceneStartSphere(position.X, position.Y, position.Z, LoadSceneRadius, 0);

        await WaitAsync(() => !Native.IsNewLoadSceneActive() || Native.IsNewLoadSceneLoaded(), LoadSceneTimeoutMs);

        Native.NewLoadSceneStop();
    }

    private static async Task FadeOutAsync()
    {
        if (Native.IsScreenFadedOut())
        {
            return;
        }

        Native.DoScreenFadeOut(FadeMs);

        await WaitAsync(Native.IsScreenFadedOut, FadeMs * 4);
    }

    private static async Task<bool> WaitAsync(Func<bool> done, int timeoutMs)
    {
        var started = Native.GetGameTimer();

        while (!done())
        {
            if (Native.GetGameTimer() - started > timeoutMs)
            {
                return false;
            }

            await API.Delay(0);
        }

        return true;
    }
}

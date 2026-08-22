using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players.Appearance;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Players.Character;

public enum CameraFocus
{
    FullBody,
    Head,
    UpperBody,
    LowerBody,
    Shoes,
    LowerArms,
    FullArms,
}

public static class CharacterCamera
{
    private const int ControlGroup = 0;

    private const string ScriptedCamera = "DEFAULT_SCRIPTED_CAMERA";

    private const string WatchDictionary = "anim@random@shop_clothes@watches";

    private const float FieldOfView = 45f;

    private const int InterpMs = 1000;

    private const int TurnMs = 1600;

    private const int LookMs = 10000;

    private const int LookRenewMs = 8000;

    private static readonly (Vector3 From, Vector3 At)[] Framings =
    [
        (new Vector3(0f, 2.8f, 0.3f), new Vector3(0f, 0f, 0f)),
        (new Vector3(0f, 0.9f, 0.65f), new Vector3(0f, 0f, 0.6f)),
        (new Vector3(0f, 1.4f, 0.5f), new Vector3(0f, 0f, 0.3f)),
        (new Vector3(0f, 1.6f, -0.3f), new Vector3(0f, 0f, -0.45f)),
        (new Vector3(0f, 0.98f, -0.7f), new Vector3(0f, 0f, -0.90f)),
        (new Vector3(0f, 0.98f, 0.1f), new Vector3(0f, 0f, 0f)),
        (new Vector3(0f, 1.3f, 0.35f), new Vector3(0f, 0f, 0.15f)),
    ];

    private static readonly (float X, float Y)[] SideSwings =
    [
        (2.2f, -1f),
        (0.7f, -0.45f),
        (1.35f, -0.4f),
        (1f, -0.4f),
        (0.9f, -0.4f),
        (0.8f, -0.7f),
        (1.5f, -1f),
    ];

    private static readonly int[] Suppressed =
    [
        24, 25, 30, 31, 32, 33, 34, 35, 36, 37, 44, 45, 47, 55, 56, 57, 58,
        71, 72, 75, 76, 81, 82, 83, 84, 85, 99, 100, 114, 115, 140, 141, 142, 143, 257, 263, 264,
    ];

    private static TickHandle? _tick;

    private static int _camera;

    private static Vector3 _from;

    private static Vector3 _at;

    private static bool _interpolating;

    private static int _previous;

    private static bool _reversed;

    private static bool _watchesLoaded;

    private static bool _watchPlaying;

    private static int _looking = int.MinValue;

    private static int _lookedAt;

    public static void Initialize() =>
        _tick = TickRegistry.Register(
            "CharacterCreator.Camera",
            Frame,
            TickRate.PerFrame,
            condition: () => MpCharacterState.IsEditing,
            onStarted: Begin,
            onStopped: End);

    public static void Reevaluate() => _tick?.Reevaluate();

    public static void AddButtons(MenuBuilder menu)
    {
        menu.InstructionalButtons.Add((Control.MoveLeftRight, MenuText.Key(Loc.CharacterCreator.TurnHead)));
        menu.InstructionalButtons.Add((Control.Jump, MenuText.Key(Loc.CharacterCreator.TurnCharacter)));
        menu.InstructionalButtons.Add((Control.ParachuteBrakeLeft, MenuText.Key(Loc.CharacterCreator.TurnCameraLeft)));
        menu.InstructionalButtons.Add((Control.ParachuteBrakeRight, MenuText.Key(Loc.CharacterCreator.TurnCameraRight)));
    }

    public static CameraFocus FocusFor(Menu? menu, CameraFocus page)
    {
        if (menu?.GetCurrentMenuItem() is not { } item)
        {
            return page;
        }

        return item.ItemData switch
        {
            CameraFocus focus => focus,
            ICameraFraming framing => framing.Framing,
            PedCustomizationRows.SlotReference slot => FocusForSlot(slot),
            _ => page,
        };
    }

    public static CameraFocus Page { get; set; } = CameraFocus.FullBody;

    private static CameraFocus FocusForSlot(PedCustomizationRows.SlotReference slot)
    {
        if (slot.IsComponent)
        {
            return slot.Slot switch
            {
                PedComponentSlots.Head or PedComponentSlots.Hair or PedComponentSlots.Mask => CameraFocus.Head,
                PedComponentSlots.Legs => CameraFocus.LowerBody,
                PedComponentSlots.Shoes => CameraFocus.Shoes,
                PedComponentSlots.Decals => CameraFocus.FullBody,
                _ => CameraFocus.UpperBody,
            };
        }

        return slot.Slot switch
        {
            PedPropSlots.Hats or PedPropSlots.Glasses or PedPropSlots.Ears => CameraFocus.Head,

            PedPropSlots.Watches => _reversed ? CameraFocus.LowerArms : CameraFocus.FullArms,
            _ => CameraFocus.LowerArms,
        };
    }

    private static void Begin() => SharedAPI.RunOnMainThread(() =>
    {
        _reversed = false;
        _interpolating = false;
        _watchPlaying = false;
        _looking = int.MinValue;

        var ped = Native.PlayerPedId();

        Native.ClearPedTasksImmediately(ped);

        Native.SetPedDesiredHeading(ped, Native.GetEntityHeading(ped));

        Native.RequestAnimDict(WatchDictionary);

        Native.DisplayHud(false);
        Native.DisplayRadar(false);
    });

    private static void End() => SharedAPI.RunOnMainThread(() =>
    {
        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);
        Native.SetEntityCollision(ped, true, true);
        Native.FreezeEntityPosition(ped, false);

        Native.SetPedCanPlayGestureAnims(ped, true);
        Native.SetPedCanPlayAmbientIdles(ped, false, false);

        _watchPlaying = false;
        _looking = int.MinValue;

        Native.DisplayHud(true);
        Native.DisplayRadar(true);

        if (_watchesLoaded)
        {
            Native.RemoveAnimDict(WatchDictionary);
            _watchesLoaded = false;
        }

        Destroy();

        MenuController.DisableBackButton = false;

        _reversed = false;
    });

    private static async Task Frame()
    {
        var ped = Native.PlayerPedId();

        Suppress();
        StandStill(ped);

        CharacterEdit.KeepExpression();

        Native.SetEntityCollision(ped, false, false);
        Native.FreezeEntityPosition(ped, true);

        _watchesLoaded = Native.HasAnimDictLoaded(WatchDictionary);

        var focus = FocusFor(MenuController.GetCurrentMenu(), Page);

        PlayWatchIdle(ped, focus);
        LookWherePlayerAsks(ped);

        if (Native.IsControlJustReleased(ControlGroup, (int)Control.Jump))
        {
            await TurnAroundAsync(ped);

            return;
        }

        Aim(ped, focus);
    }

    private static void Aim(int ped, CameraFocus focus)
    {
        var framing = Framings[(int)focus];
        var swing = Swing(focus);

        var x = framing.From.X + swing.X;
        var y = framing.From.Y + swing.Y;

        var from = Native.GetOffsetFromEntityInWorldCoords(
            ped, _reversed ? -x : x, _reversed ? -y : y, framing.From.Z);

        var at = Native.GetOffsetFromEntityInWorldCoords(ped, framing.At.X, framing.At.Y, framing.At.Z);

        if (!Native.DoesCamExist(_camera))
        {
            _camera = Create(from, at);
            _from = from;
            _at = at;

            Native.SetCamActive(_camera, true);
            Native.RenderScriptCams(true, false, 0, false, false, 0);

            return;
        }

        if (_interpolating)
        {
            if (Native.IsCamInterpolating(_camera))
            {
                return;
            }

            Retire();

            _interpolating = false;
        }

        if (Same(_from, from) && Same(_at, at))
        {
            return;
        }

        var replacement = Create(from, at);

        Native.SetCamActiveWithInterp(replacement, _camera, InterpMs, 1, 1);

        _previous = _camera;
        _camera = replacement;
        _from = from;
        _at = at;
        _interpolating = true;
    }

    private static int Create(Vector3 from, Vector3 at)
    {
        var camera = Native.CreateCam(ScriptedCamera, false);

        Native.SetCamCoord(camera, from.X, from.Y, from.Z);
        Native.SetCamFov(camera, FieldOfView);
        Native.PointCamAtCoord(camera, at.X, at.Y, at.Z);

        return camera;
    }

    private static void Retire()
    {
        if (_previous != 0 && Native.DoesCamExist(_previous))
        {
            Native.DestroyCam(_previous, false);
        }

        _previous = 0;
    }

    private static void Destroy()
    {
        Retire();

        Native.RenderScriptCams(false, false, 0, true, true, 0);

        if (Native.DoesCamExist(_camera))
        {
            Native.SetCamActive(_camera, false);
            Native.DestroyCam(_camera, false);
        }

        _camera = 0;
        _interpolating = false;
    }

    private static (float X, float Y) Swing(CameraFocus focus)
    {
        var left = Native.IsDisabledControlPressed(ControlGroup, (int)Control.ParachuteBrakeLeft);
        var right = Native.IsDisabledControlPressed(ControlGroup, (int)Control.ParachuteBrakeRight);

        if (left == right)
        {
            return (0f, 0f);
        }

        var swing = SideSwings[(int)focus];

        return right ? (-swing.X, swing.Y) : swing;
    }

    private static void StandStill(int ped)
    {
        Native.SetPedCanPlayGestureAnims(ped, false);

        Native.SetPedCanPlayAmbientIdles(ped, true, true);
    }

    private static void LookWherePlayerAsks(int ped)
    {
        var left = Native.IsDisabledControlPressed(ControlGroup, (int)Control.MoveLeftOnly);
        var right = Native.IsDisabledControlPressed(ControlGroup, (int)Control.MoveRightOnly);

        var side = left && !right ? 1 : right && !left ? -1 : 0;

        if (side == _looking && Native.GetGameTimer() - _lookedAt < LookRenewMs)
        {
            return;
        }

        _looking = side;
        _lookedAt = Native.GetGameTimer();

        var at = Native.GetOffsetFromEntityInWorldCoords(ped, side * 1.2f, 0.5f, 0.7f);

        Native.TaskLookAtCoord(ped, at.X, at.Y, at.Z, LookMs, 0, 2);
    }

    private static void PlayWatchIdle(int ped, CameraFocus focus)
    {
        var wanted = focus == CameraFocus.FullArms && !_reversed && _watchesLoaded;

        if (wanted)
        {
            if (!Native.IsEntityPlayingAnim(ped, WatchDictionary, "BASE", 3))
            {
                Native.TaskPlayAnim(ped, WatchDictionary, "BASE", 8f, -8f, -1, 1, 0f, false, 0, false);
            }

            _watchPlaying = true;

            return;
        }

        if (!_watchPlaying)
        {
            return;
        }

        Native.StopAnimTask(ped, WatchDictionary, "BASE", -4f);

        _watchPlaying = false;
    }

    private static async Task TurnAroundAsync(int ped)
    {
        var position = Native.GetEntityCoords(ped, true);
        var heading = Native.GetEntityHeading(ped);

        Native.SetEntityCollision(ped, true, true);
        Native.FreezeEntityPosition(ped, false);
        Native.TaskGoStraightToCoord(ped, position.X, position.Y, position.Z, 8f, TurnMs, heading + 180f, 0.1f);

        var started = Native.GetGameTimer();

        while (Native.GetGameTimer() - started < TurnMs)
        {
            Native.DisableAllControlActions(ControlGroup);

            await API.Delay(0);
        }

        Native.ClearPedTasks(ped);
        Native.SetEntityCoordsNoOffset(ped, position.X, position.Y, position.Z, false, false, false);
        Native.FreezeEntityPosition(ped, true);
        Native.SetEntityCollision(ped, false, false);

        _reversed = !_reversed;
    }

    private static void Suppress()
    {
        foreach (var control in Suppressed)
        {
            Native.DisableControlAction(ControlGroup, control, true);
        }
    }

    private static bool Same(Vector3 left, Vector3 right) =>
        Math.Abs(left.X - right.X) < 0.001f
        && Math.Abs(left.Y - right.Y) < 0.001f
        && Math.Abs(left.Z - right.Z) < 0.001f;
}

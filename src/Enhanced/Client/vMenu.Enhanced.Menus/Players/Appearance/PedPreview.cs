using System.Numerics;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Stands the ped a menu row is about beside the menu, so a saved ped or character can be looked at
// before it is spawned. A menu registers how to find its ped, and one tick asks whichever menu is on
// screen. Asking rather than being told is what keeps the same ped standing while the player walks
// from a list into that ped's own page, instead of deleting it on one menu's close and making it
// again on the next one's open.
public static class PedPreview
{
    private const float Distance = 4.2f;

    // How far the ped stands from the edge of the menu, as a share of the screen width.
    private const float MenuGap = 0.02f;

    // Roughly how wide a ped is in the world, used to keep the whole ped clear of the menu.
    private const float PedWidth = 0.4f;

    // How far below the centre of the screen the ped stands, as a share of the screen height.
    private const float Lower = 0.12f;

    private const float TurnDegreesPerSecond = 120f;

    private const int ModelTimeoutMs = 5000;

    private const int StreamingTimeoutMs = 3000;

    private const int IdleMs = 250;

    private const int CivilianPedType = 4;

    private const int ControlGroup = 0;

    private const int TurnLeftControl = 44;

    private const int TurnRightControl = 38;

    private static readonly Dictionary<Menu, Source> Sources = [];

    private static TickHandle? _tick;

    private static int _ped;

    private static object? _showing;

    private static int _generation;

    private static float _turn;

    public static bool Enabled => UserDefaults.MiscPedPreview.Value;

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "Players.PedPreview",
            Frame,
            TickRate.Varying(static () => _showing is null ? TickRate.Every(IdleMs) : TickRate.PerFrame),
            static () => Enabled,
            onStopped: Hide);

        ResourceShutdown.Stopping += Hide;
    }

    public static void SetEnabled(bool enabled)
    {
        UserDefaults.MiscPedPreview.Value = enabled;

        _tick?.Reevaluate();
    }

    // pick returns what the highlighted row stands for, or null for a row with no ped. The same object
    // coming back keeps the ped that is already standing there, a different one replaces it.
    public static void Register<T>(Menu menu, Func<Menu, T?> pick, Func<T, uint> model, Func<int, T, Task> dress)
        where T : class
    {
        Sources[menu] = new Source(
            current => pick(current),
            picked => model((T)picked),
            (ped, picked) => dress(ped, (T)picked));
    }

    public static void Hide()
    {
        _generation++;
        _showing = null;

        Destroy(_ped);

        _ped = 0;
    }

    private static void Frame()
    {
        var picked = Pick(out var source);

        if (picked is null || source is null)
        {
            if (_showing is not null)
            {
                Hide();
            }

            return;
        }

        if (!ReferenceEquals(picked, _showing))
        {
            Show(picked, source);
        }

        Follow();
    }

    private static object? Pick(out Source? source)
    {
        source = null;

        // The pause menu hides every menu but leaves them open, and a ped floating over the map is wrong.
        if (Native.IsPauseMenuActive() || MenuController.GetCurrentMenu() is not { } menu)
        {
            return null;
        }

        if (!Sources.TryGetValue(menu, out source))
        {
            return null;
        }

        try
        {
            return source.Pick(menu);
        }
        catch (Exception exception)
        {
            Log.Debug($"[PedPreview] Working out the ped for this menu failed: {exception.Message}");

            return null;
        }
    }

    private static async void Show(object picked, Source source)
    {
        Hide();

        // After Hide, which bumps the generation itself.
        var mine = _generation;

        // Set before anything can fail, so a model that will not load is tried once rather than every frame.
        _showing = picked;
        _turn = 0f;

        var model = source.Model(picked);

        if (!Native.IsModelInCdimage(model) || !Native.IsModelAPed(model))
        {
            return;
        }

        Native.RequestModel(model);

        var deadline = Native.GetGameTimer() + ModelTimeoutMs;

        while (!Native.HasModelLoaded(model))
        {
            if (Native.GetGameTimer() > deadline || mine != _generation)
            {
                Native.SetModelAsNoLongerNeeded(model);

                return;
            }

            await API.Delay(0);
        }

        if (mine != _generation)
        {
            Native.SetModelAsNoLongerNeeded(model);

            return;
        }

        var (position, heading) = Placement();

        // Local only. Nobody else needs to see a ped that exists for one player's menu.
        var ped = Native.CreatePed(CivilianPedType, model, position.X, position.Y, position.Z, heading, false, false);

        Native.SetModelAsNoLongerNeeded(model);

        if (ped == 0)
        {
            return;
        }

        // Hidden until it is dressed, so the player never sees it pop from the model's default clothes.
        Native.SetEntityVisible(ped, false, false);
        Native.SetEntityCollision(ped, false, false);
        Native.FreezeEntityPosition(ped, true);
        Native.SetEntityInvincible(ped, true, false);
        Native.SetPedCanRagdoll(ped, false);
        Native.SetBlockingOfNonTemporaryEvents(ped, true);

        _ped = ped;

        try
        {
            await source.Dress(ped, picked);
        }
        catch (Exception exception)
        {
            Log.Debug($"[PedPreview] Dressing the preview failed: {exception.Message}");
        }

        var ready = Native.GetGameTimer() + StreamingTimeoutMs;

        while (mine == _generation
            && Native.DoesEntityExist(ped)
            && !Native.HaveAllStreamingRequestsCompleted(ped)
            && Native.GetGameTimer() < ready)
        {
            await API.Delay(0);
        }

        // Hide already deleted it when the generation moved on.
        if (mine != _generation || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.TaskStandStill(ped, -1);
        Native.SetEntityVisible(ped, true, false);
    }

    private static void Follow()
    {
        if (_ped == 0 || !Native.DoesEntityExist(_ped))
        {
            return;
        }

        Native.DisableControlAction(ControlGroup, TurnLeftControl, true);
        Native.DisableControlAction(ControlGroup, TurnRightControl, true);

        var step = TurnDegreesPerSecond * Native.GetFrameTime();

        if (Native.IsDisabledControlPressed(ControlGroup, TurnLeftControl))
        {
            _turn -= step;
        }

        if (Native.IsDisabledControlPressed(ControlGroup, TurnRightControl))
        {
            _turn += step;
        }

        var (position, heading) = Placement();

        Native.SetEntityCoordsNoOffset(_ped, position.X, position.Y, position.Z, false, false, false);
        Native.SetEntityHeading(_ped, heading + _turn);

        // Lights the ped from the camera's side, so it reads at night and indoors too.
        var camera = Native.GetFinalRenderedCamCoord();
        var light = position + ((camera - position) * 0.4f) + new Vector3(0f, 0f, 0.6f);

        Native.DrawLightWithRange(light.X, light.Y, light.Z, 255, 255, 255, 3.5f, 1.2f);
    }

    // In front of the camera, on whichever side of the menu has room, facing the player. Worked out again
    // every frame, so the ped stays put on screen while the camera moves.
    private static (Vector3 Position, float Heading) Placement()
    {
        var camera = Native.GetFinalRenderedCamCoord();
        var rotation = Native.GetFinalRenderedCamRot(2);

        var pitch = Radians(rotation.X);
        var yaw = Radians(rotation.Z);

        var forward = new Vector3(
            (float)(-Math.Sin(yaw) * Math.Cos(pitch)),
            (float)(Math.Cos(yaw) * Math.Cos(pitch)),
            (float)Math.Sin(pitch));

        var right = new Vector3((float)Math.Cos(yaw), (float)Math.Sin(yaw), 0f);

        var up = new Vector3(
            (right.Y * forward.Z) - (right.Z * forward.Y),
            (right.Z * forward.X) - (right.X * forward.Z),
            (right.X * forward.Y) - (right.Y * forward.X));

        var aspectRatio = Native.GetScreenAspectRatio(false);
        var tanHalfVertical = (float)Math.Tan(Radians(Native.GetFinalRenderedCamFov()) / 2f);
        var tanHalfHorizontal = tanHalfVertical * aspectRatio;

        // The same numbers MenuAPI places the menu with.
        var inset = (1f - Native.GetSafeZoneSize()) / 2f;
        var menuWidth = Menu.Width / (1080f * aspectRatio);

        var halfWidth = PedWidth / (2f * Distance * tanHalfHorizontal);

        var screenX = MenuController.MenuAlignment == MenuController.MenuAlignmentOption.Left
            ? inset + menuWidth + MenuGap + halfWidth
            : 1f - inset - menuWidth - MenuGap - halfWidth;

        // No room on that side, so the other one.
        var across = menuWidth + (2f * (MenuGap + halfWidth));

        if (screenX - halfWidth < 0f)
        {
            screenX += across;
        }
        else if (screenX + halfWidth > 1f)
        {
            screenX -= across;
        }

        var side = ((screenX * 2f) - 1f) * Distance * tanHalfHorizontal;

        var position = camera
            + (forward * Distance)
            + (right * side)
            - (up * (Lower * Distance * tanHalfVertical));

        return (position, rotation.Z + 180f);
    }

    private static double Radians(float degrees) => degrees * Math.PI / 180.0;

    private static void Destroy(int entity)
    {
        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        Native.SetEntityAsMissionEntity(entity, true, true);

        // Ref<T> cannot cross an await, and the out overload pushes a literal 0.
        var doomed = entity;

        Native.DeleteEntity(ref doomed);
    }

    private sealed record Source(Func<Menu, object?> Pick, Func<object, uint> Model, Func<int, object, Task> Dress);
}

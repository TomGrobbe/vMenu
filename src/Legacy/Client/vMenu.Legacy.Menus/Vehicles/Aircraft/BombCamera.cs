using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Aircraft;

internal static class BombCamera
{
    private const uint ScriptedCamera = 26379945;

    private const string AudioScene = "DLC_SM_Bomb_Bay_View_Scene";

    private const float FieldOfView = 65f;

    private const float LookDown = -90f;

    private const float FollowPitch = -105f;

    private const int EaseInMs = 3000;

    private const int FollowMs = 3000;

    private const int ReturnMs = 2000;

    private const int FindBombMs = 1000;

    private const int RotationOrder = 2;

    private const float AutopilotDistance = 1000f;

    private const float RetargetDistance = 25f;

    private const float FollowSearchRadius = 25f;

    private const int PlaneMissionGoTo = 4;

    private static int _camera;

    private static int _followCamera;

    private static int _followedBomb;

    private static bool _following;

    private static bool _returning;

    private static int _findBombUntil;

    private static bool _autopilot;

    private static Vector3 _autopilotTarget;

    private static bool _cinematicBlocked;

    public static void FollowNextBomb() => _findBombUntil = _camera != 0 ? API.GetGameTimer() + FindBombMs : 0;

    public static void Frame(AircraftSession session, bool doorsOpen)
    {
        if (!doorsOpen)
        {
            Stop();
            Unblock();

            return;
        }

        API.DisableControlAction(AircraftControls.Group, AircraftControls.CinematicCamera, true);

        if (!_cinematicBlocked)
        {
            API.SetCinematicButtonActive(false);

            _cinematicBlocked = true;
        }

        if (API.IsCinematicCamRendering())
        {
            API.SetCinematicModeActive(false);
        }

        var toggled = !AircraftControls.Muted && API.IsDisabledControlJustPressed(AircraftControls.Group, AircraftControls.CinematicCamera);

        if (_camera == 0)
        {
            if (toggled)
            {
                Start(session);
            }

            return;
        }

        if (toggled)
        {
            Stop();

            return;
        }

        Track(session);
    }

    public static void Stop()
    {
        if (_camera == 0 && _followCamera == 0)
        {
            return;
        }

        if (_autopilot && !API.IsPedInjured(API.PlayerPedId()))
        {
            API.ClearPedTasks(API.PlayerPedId());
        }

        NativeFixer.RenderScriptCams(false, false, EaseInMs, true, false, 0);

        Destroy(ref _camera);
        Destroy(ref _followCamera);

        if (API.IsAudioSceneActive(AudioScene))
        {
            API.StopAudioScene(AudioScene);
        }

        _autopilot = false;
        _following = false;
        _returning = false;
        _findBombUntil = 0;
        _followedBomb = 0;
    }

    public static void Reset()
    {
        Stop();
        Unblock();
    }

    private static void Unblock()
    {
        if (!_cinematicBlocked)
        {
            return;
        }

        API.SetCinematicButtonActive(true);

        _cinematicBlocked = false;
    }

    private static void Start(AircraftSession session)
    {
        var heading = API.GetEntityHeading(session.Vehicle);

        Autopilot(session, API.GetEntityCoords(session.Vehicle, true));

        API.StartAudioScene(AudioScene);

        _camera = API.CreateCameraWithParams(ScriptedCamera, 0f, 0f, 0f, LookDown, 0f, heading, FieldOfView, true, RotationOrder);

        API.AttachCamToEntity(_camera, session.Vehicle, 0f, session.Profile.CameraY, session.Profile.CameraZ, true);
        NativeFixer.RenderScriptCams(true, false, EaseInMs, true, false, 0);
    }

    private static void Track(AircraftSession session)
    {
        var position = API.GetEntityCoords(session.Vehicle, true);
        var heading = API.GetEntityHeading(session.Vehicle);

        if (_autopilot && Vector3.Distance(position, _autopilotTarget) < RetargetDistance)
        {
            Autopilot(session, position);
        }

        API.DisableControlAction(AircraftControls.Group, AircraftControls.LookLeftRight, true);
        API.DisableControlAction(AircraftControls.Group, AircraftControls.LookUpDown, true);

        if (_findBombUntil != 0)
        {
            if (_following || API.GetGameTimer() >= _findBombUntil)
            {
                _findBombUntil = 0;
            }
            else if (NativeFixer.GetProjectileOfProjectileTypeWithinDistance(
                    API.PlayerPedId(), BombBay.WeaponFor(session), FollowSearchRadius, out _, out var bomb, false)
                && API.DoesEntityExist(bomb))
            {
                _findBombUntil = 0;

                Follow(session, bomb, heading);
            }
        }

        if (_following && !API.DoesEntityExist(_followedBomb))
        {
            Return(session, heading);
        }

        if (_returning && !API.IsCamInterpolating(_camera))
        {
            Destroy(ref _followCamera);

            _returning = false;
        }

        if (_followCamera != 0)
        {
            API.SetCamRot(_followCamera, FollowPitch, 0f, heading, RotationOrder);
        }

        API.SetCamRot(_camera, LookDown, 0f, heading, RotationOrder);
    }

    private static void Follow(AircraftSession session, int bomb, float heading)
    {
        Destroy(ref _followCamera);

        _followCamera = Attached(session, FollowPitch, heading);

        API.SetCamActiveWithInterp(_followCamera, _camera, FollowMs, 1, 1);

        _followedBomb = bomb;
        _following = true;
        _returning = false;
    }

    private static void Return(AircraftSession session, float heading)
    {
        Destroy(ref _camera);

        _camera = Attached(session, LookDown, heading);

        API.SetCamActiveWithInterp(_camera, _followCamera, ReturnMs, 1, 1);

        _following = false;
        _returning = true;
        _followedBomb = 0;
    }

    private static int Attached(AircraftSession session, float pitch, float heading)
    {
        var camera = API.CreateCam("DEFAULT_SCRIPTED_CAMERA", false);

        API.SetCamFov(camera, FieldOfView);
        API.AttachCamToEntity(camera, session.Vehicle, 0f, session.Profile.CameraY, session.Profile.CameraZ, true);
        API.SetCamRot(camera, pitch, 0f, heading, RotationOrder);

        return camera;
    }

    // Rockstar flies the plane straight on while the pilot looks down, and leaves helicopters and
    // hovering VTOLs alone.
    private static void Autopilot(AircraftSession session, Vector3 position)
    {
        if (!session.IsPilot || !session.IsPlane)
        {
            return;
        }

        var vtol = session.Has(AircraftWeaponRules.Vtol);

        if (vtol && API.GetVehicleFlightNozzlePosition(session.Vehicle) != 0f)
        {
            return;
        }

        var forward = API.GetEntityForwardVector(session.Vehicle);
        var flat = new Vector2(forward.X, forward.Y);

        if (flat.LengthSquared() > 0f)
        {
            flat = Vector2.Normalize(flat);
        }

        _autopilotTarget = new Vector3(position.X + (flat.X * AutopilotDistance), position.Y + (flat.Y * AutopilotDistance), position.Z);

        API.TaskPlaneMission(
            API.PlayerPedId(), session.Vehicle, 0, 0,
            _autopilotTarget.X, _autopilotTarget.Y, position.Z,
            PlaneMissionGoTo, 50f, 0.1f, -1f, 30, 20, !vtol);

        _autopilot = true;
    }

    private static void Destroy(ref int camera)
    {
        if (camera != 0 && API.DoesCamExist(camera))
        {
            API.DestroyCam(camera, false);
        }

        camera = 0;
    }
}

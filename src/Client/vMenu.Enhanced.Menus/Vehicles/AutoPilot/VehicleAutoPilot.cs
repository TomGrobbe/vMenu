using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public enum AutoPilotMode
{
    None,
    Waypoint,
    Point,
    Wander,
    Path,
}

public static class VehicleAutoPilot
{
    private const float GroundArrival = 12f;

    private const float AirArrival = 40f;

    private const float GameDefault = -1f;

    private const int MissionGoto = 4;

    private const int ParkPerpendicular = 1;

    private const float WanderRadius = 600f;

    private const float WanderHeight = 120f;

    private const int StopActionPark = 0;

    private const int StopActionBrake = 1;

    private const int BoatWanderRoute = 256;

    private const int WaypointBlipType = 4;

    private static readonly Random Rng = new();

    private static TickHandle? _tick;

    private static AutoPilotMode _mode;

    private static bool _paused;

    private static Vector3 _target;

    private static string _targetName = string.Empty;

    private static List<AutoPilotPathPoint> _path = [];

    private static int _pathIndex;

    public static event Action? Changed;

    public static AutoPilotMode Mode => _mode;

    public static bool HasTask => _mode != AutoPilotMode.None;

    public static bool IsPaused => _paused;

    public static string TargetName => _targetName;

    public static int PathIndex => _pathIndex;

    public static int PathCount => _path.Count;

    public static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.AutoPilot);

    public static void Initialize() =>
        _tick = TickRegistry.Register(
            "Vehicle.AutoPilot", Monitor, TickRate.Every(250), () => HasTask && !_paused);

    public static bool DriveToWaypoint()
    {
        if (Where() is not { } vehicle)
        {
            return false;
        }

        if (WaypointPosition() is not { } target)
        {
            return false;
        }

        _mode = AutoPilotMode.Waypoint;
        _targetName = string.Empty;
        _target = target;

        Begin(vehicle);

        return true;
    }

    public static bool DriveToPoint(SavedAutoPilotPoint point)
    {
        if (Where() is not { } vehicle)
        {
            return false;
        }

        _mode = AutoPilotMode.Point;
        _targetName = point.Name;
        _target = new Vector3(point.X, point.Y, point.Z);

        Begin(vehicle);

        return true;
    }

    public static bool Wander()
    {
        if (Where() is not { } vehicle)
        {
            return false;
        }

        _mode = AutoPilotMode.Wander;
        _targetName = string.Empty;

        Begin(vehicle);

        return true;
    }

    public static bool ReplayPath(SavedAutoPilotPath path)
    {
        if (path.Points.Count == 0 || Where() is not { } vehicle)
        {
            return false;
        }

        _mode = AutoPilotMode.Path;
        _targetName = path.Name;

        _path = [.. path.Points];
        _pathIndex = 0;
        _target = Point(_path[0]);

        Begin(vehicle);

        return true;
    }

    public static void Pause()
    {
        if (!HasTask || _paused)
        {
            return;
        }

        _paused = true;

        Halt();

        Announce();
    }

    public static void Resume()
    {
        if (!HasTask || !_paused)
        {
            return;
        }

        if (Where() is not { } vehicle)
        {
            return;
        }

        _paused = false;

        Begin(vehicle);
    }

    public static void Stop()
    {
        if (!HasTask)
        {
            return;
        }

        Halt();

        Clear();
    }

    private static void Clear()
    {
        _mode = AutoPilotMode.None;
        _paused = false;
        _targetName = string.Empty;
        _path = [];
        _pathIndex = 0;

        Announce();
    }

    private static void Begin(int vehicle)
    {
        Issue(vehicle);

        Announce();
    }

    private static void Announce()
    {
        _tick?.Reevaluate();

        Changed?.Invoke();
    }

    private static int? Where()
    {
        if (!IsAllowed)
        {
            return null;
        }

        if (OwnVehicle.RequireDriven(Loc.AutoPilot.NoVehicle, Loc.AutoPilot.NotDriver) is not { } vehicle)
        {
            return null;
        }

        return vehicle.Handle;
    }

    private static void Monitor()
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0 || !IsAllowed || Native.IsEntityDead(vehicle, false) || !Native.IsVehicleDriveable(vehicle, false))
        {
            Clear();

            return;
        }

        switch (_mode)
        {
            case AutoPilotMode.Waypoint:
                if (!Native.IsWaypointActive())
                {
                    Stop();
                }

                break;

            case AutoPilotMode.Point:
                if (Arrived(vehicle))
                {
                    Stop();
                }

                break;

            case AutoPilotMode.Path:
                Advance(vehicle);

                break;

            case AutoPilotMode.Wander:
                if (Airborne(Kind(vehicle)) && Arrived(vehicle))
                {
                    Issue(vehicle);
                }

                break;
        }
    }

    private static void Advance(int vehicle)
    {
        if (!Arrived(vehicle))
        {
            return;
        }

        _pathIndex++;

        if (_pathIndex >= _path.Count)
        {
            Stop();

            return;
        }

        _target = Point(_path[_pathIndex]);

        Issue(vehicle);

        Changed?.Invoke();
    }

    private static bool Arrived(int vehicle)
    {
        var here = Native.GetEntityCoords(vehicle, false);
        var radius = Airborne(Kind(vehicle)) ? AirArrival : GroundArrival;

        return Vector3.Distance(here, _target) <= radius;
    }

    private static void Issue(int vehicle)
    {
        var ped = Native.PlayerPedId();
        var kind = Kind(vehicle);
        var profile = AutoPilotDefaults.Resolve(kind);
        var speed = Speed(vehicle, profile);

        Native.SetDriverAbility(ped, 1f);
        Native.SetDriverAggressiveness(ped, 0f);

        if (_mode is AutoPilotMode.Wander && !Airborne(kind))
        {
            Roam(ped, vehicle, kind, profile, speed);

            return;
        }

        if (_mode is AutoPilotMode.Wander)
        {
            _target = Somewhere(vehicle);
        }

        Goto(ped, vehicle, kind, profile, speed);
    }

    private static void Goto(int ped, int vehicle, AutoPilotCategory kind, SavedDrivingProfile profile, float speed)
    {
        switch (kind)
        {
            case AutoPilotCategory.Helicopter:
                Native.TaskHeliMission(
                    ped, vehicle, 0, 0,
                    _target.X, _target.Y, _target.Z,
                    MissionGoto, speed, AirArrival,
                    GameDefault,
                    profile.FlightHeight, profile.MinHeightAboveTerrain,
                    GameDefault,
                    profile.Flags);

                break;

            case AutoPilotCategory.Plane:
                Native.TaskPlaneMission(
                    ped, vehicle, 0, 0,
                    _target.X, _target.Y, _target.Z,
                    MissionGoto, speed, AirArrival,
                    GameDefault,
                    profile.FlightHeight, profile.MinHeightAboveTerrain,
                    profile.Precise);

                break;

            case AutoPilotCategory.Boat:
                Native.TaskBoatMission(
                    ped, vehicle, 0, 0,
                    _target.X, _target.Y, _target.Z,
                    MissionGoto, speed,
                    AutoPilotDefaults.Resolve(AutoPilotCategory.Vehicle).Flags,
                    GroundArrival,
                    profile.Flags);

                break;

            default:
                Native.TaskVehicleDriveToCoordLongrange(
                    ped, vehicle,
                    _target.X, _target.Y, _target.Z,
                    speed, profile.Flags, GroundArrival);

                break;
        }
    }

    private static void Roam(int ped, int vehicle, AutoPilotCategory kind, SavedDrivingProfile profile, float speed)
    {
        if (kind is AutoPilotCategory.Boat)
        {
            _target = Somewhere(vehicle);

            Native.TaskBoatMission(
                ped, vehicle, 0, 0,
                _target.X, _target.Y, _target.Z,
                MissionGoto, speed,
                AutoPilotDefaults.Resolve(AutoPilotCategory.Vehicle).Flags,
                GroundArrival,
                profile.Flags | BoatWanderRoute);

            return;
        }

        Native.TaskVehicleDriveWander(ped, vehicle, speed, profile.Flags);
    }

    private static Vector3 Somewhere(int vehicle)
    {
        var here = Native.GetEntityCoords(vehicle, false);

        var angle = Rng.NextDouble() * Math.PI * 2;
        var distance = (float)((Rng.NextDouble() * 0.7 + 0.3) * WanderRadius);

        return new Vector3(
            here.X + (float)Math.Cos(angle) * distance,
            here.Y + (float)Math.Sin(angle) * distance,
            Airborne(Kind(vehicle)) ? here.Z + WanderHeight : here.Z);
    }

    private static float Speed(int vehicle, SavedDrivingProfile profile)
    {
        if (profile.CruiseSpeed > 0f)
        {
            return profile.CruiseSpeed;
        }

        var chosen = UserDefaults.AutoPilotCruiseSpeed.Value;

        return chosen > 0
            ? chosen
            : Native.GetVehicleModelMaxSpeed((uint)Native.GetEntityModel(vehicle));
    }

    private static void Halt()
    {
        var vehicle = OwnVehicle.Driven();
        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);

        if (vehicle == 0)
        {
            return;
        }

        var action = UserDefaults.AutoPilotStopAction.Value;

        if (action == StopActionPark && Kind(vehicle) is AutoPilotCategory.Vehicle && Park(ped, vehicle))
        {
            return;
        }

        if (action == StopActionPark || action == StopActionBrake)
        {
            Native.BringVehicleToHalt(vehicle, 5f, 1, false);
        }
    }

    private static bool Park(int ped, int vehicle)
    {
        var here = Native.GetEntityCoords(vehicle, false);

        if (!Native.GetClosestVehicleNodeWithHeading(here.X, here.Y, here.Z, out var node, out var heading, 1, 3f, 0))
        {
            return false;
        }

        Native.TaskVehiclePark(ped, vehicle, node.X, node.Y, node.Z, heading, ParkPerpendicular, 20f, false);

        return true;
    }

    public static Vector3? WaypointPosition()
    {
        if (!Native.IsWaypointActive())
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.NoWaypoint));

            return null;
        }

        if (WaypointBlip() is not { } blip)
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.NoWaypoint));

            return null;
        }

        var position = Native.GetBlipCoords(blip);

        var height = Native.GetClosestVehicleNodeWithHeading(position.X, position.Y, position.Z, out var node, out _, 1, 3f, 0)
            ? node.Z
            : position.Z;

        return new Vector3(position.X, position.Y, height);
    }

    private static int? WaypointBlip()
    {
        for (int it = Native.GetBlipInfoIdIterator(), blip = Native.GetFirstBlipInfoId(it);
             Native.DoesBlipExist(blip);
             blip = Native.GetNextBlipInfoId(it))
        {
            if (Native.GetBlipInfoIdType(blip) == WaypointBlipType)
            {
                return blip;
            }
        }

        return null;
    }

    private static Vector3 Point(AutoPilotPathPoint point) => new(point.X, point.Y, point.Z);

    private static bool Airborne(AutoPilotCategory kind) =>
        kind is AutoPilotCategory.Helicopter or AutoPilotCategory.Plane;

    public static AutoPilotCategory Kind(int vehicle)
    {
        var model = (uint)Native.GetEntityModel(vehicle);

        if (Native.IsThisModelAHeli(model))
        {
            return AutoPilotCategory.Helicopter;
        }

        if (Native.IsThisModelAPlane(model))
        {
            return AutoPilotCategory.Plane;
        }

        if (Native.IsThisModelABoat(model))
        {
            return AutoPilotCategory.Boat;
        }

        return AutoPilotCategory.Vehicle;
    }
}

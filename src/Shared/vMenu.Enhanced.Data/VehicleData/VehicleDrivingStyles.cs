namespace vMenu.Enhanced.Data.VehicleData;

public static class VehicleDrivingStyles
{
    public record Styles(string Name, string Description, uint Value);

    public static readonly IReadOnlyList<Styles> DrivingModes =
    [
        new("StopForCars", "Stop for cars", 1),
        new("StopForPeds", "Stop for pedestrians", 2),
        new("SwerveAroundAllCars", "Swerve around all cars", 4),
        new("SteerAroundStationaryCars", "Steer around stationary cars", 8),
        new("SteerAroundPeds", "Steer around pedestrians", 16),
        new("SteerAroundObjects", "Steer around objects", 32),
        new("DontSteerAroundPlayerPed", "Don't steer around the player ped", 64),
        new("StopAtLights", "Stop at traffic lights", 128),
        new("GoOffRoadWhenAvoiding", "Go off-road when avoiding obstacles", 256),
        new("DriveIntoOncomingTraffic", "Drive into oncoming traffic", 512),
        new("DriveInReverse", "Drive in reverse", 1024),
        new("UseWanderFallbackInsteadOfStraightLine", "Wander randomly instead of driving in a straight line when pathfinding fails", 2048),
        new("AvoidRestrictedAreas", "Avoid restricted areas", 4096),
        new("PreventBackgroundPathfinding", "Prevent background pathfinding", 8192),
        new("AdjustCruiseSpeedBasedOnRoadSpeed", "Adjust cruise speed based on road speed", 16384),
        new("UseShortCutLinks", "Use shortcut links", 262144),
        new("ChangeLanesAroundObstructions", "Change lanes around obstructions", 524288),
        new("UseSwitchedOffNodes", "Use switched-off navigation nodes", 2097152),
        new("PreferNavmeshRoute", "Prefer a navmesh route, primarily for off-road driving", 4194304),
        new("PlaneTaxiMode", "Make planes drive along the ground instead of flying", 8388608),
        new("ForceStraightLine", "Force a straight-line route", 16777216),
        new("UseStringPullingAtJunctions", "Use string pulling at junctions", 33554432),
        new("AvoidHighways", "Avoid highways", 536870912),
        new("ForceJoinInRoadDirection", "Force joining the road in its direction", 1073741824)
    ];

    public static readonly IReadOnlyList<Styles> BoatModes =
    [
        new("StopAtEnd", "Stop at the end of the route", 1),
        new("StopAtShore", "Stop at the shore", 2),
        new("AvoidShore", "Avoid the shore", 4),
        new("PreferForward", "Prefer moving forward", 8),
        new("NeverStop", "Never stop", 16),
        new("NeverNavMesh", "Never use the navmesh", 32),
        new("NeverRoute", "Never use a route", 64),
        new("ForceBeached", "Force the boat to be beached", 128),
        new("UseWanderRoute", "Use a wandering route", 256),
        new("UseFleeRoute", "Use a flee route", 512),
        new("NeverPause", "Never pause", 1024)
    ];

    public static readonly IReadOnlyList<Styles> HeliModes =
    [
        new("AttainRequestedOrientation", "Attain the requested orientation", 1),
        new("DontModifyOrientation", "Don't modify orientation", 2),
        new("DontModifyPitch", "Don't modify pitch", 4),
        new("DontModifyThrottle", "Don't modify throttle", 8),
        new("DontModifyRoll", "Don't modify roll", 16),
        new("LandOnArrival", "Land on arrival", 32),
        new("DontDoAvoidance", "Don't perform avoidance", 64),
        new("StartEngineImmediately", "Start the engine immediately", 128),
        new("ForceHeightMapAvoidance", "Force height-map avoidance", 256),
        new("DontClampProbesToDestination", "Don't clamp probes to the destination", 512),
        new("EnableTimeslicingWhenPossible", "Enable timeslicing when possible", 1024),
        new("CircleOppositeDirection", "Circle in the opposite direction", 2048),
        new("MaintainHeightAboveTerrain", "Maintain height above terrain", 4096),
        new("IgnoreHiddenEntitiesDuringLand", "Ignore hidden entities during landing", 8192),
        new("DisableAllHeightMapAvoidance", "Disable all height-map avoidance", 16384),
         new("None", "No helicopter mode flags", 0),
        new("HeightMapOnlyAvoidance", "Use height-map-only avoidance", 320)
    ];

    public static readonly IReadOnlyList<Styles> DrivingStyles =
    [
        new("Normal", "Normal driving style", 0),
        new("Racing", "Racing driving style", 1),
        new("Reversing", "Reversing driving style", 2)
    ];

}

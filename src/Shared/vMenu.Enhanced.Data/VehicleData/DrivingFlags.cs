namespace vMenu.Enhanced.Data.VehicleData;

// One flag bit, or one ready made combination of them. Name is the identifier Rockstar's own script
// headers use, which is what gets shown as the row description, and Label is the short human title.
//
// A class, not a record: the sandbox refuses the generated EqualityComparer<T>.Default.
public sealed class DrivingFlag(string name, string label, int value)
{
    public string Name { get; } = name;

    public string Label { get; } = label;

    public int Value { get; } = value;
}

// The three flag bitfields the vehicle task natives take, transcribed from commands_vehicle.sch in
// the leaked script source. Values are int rather than uint because every native takes an int, and
// the top bit does not fit in a positive one.
public static class DrivingFlags
{
    // DRIVINGMODE. Taken by nearly every driving task, and by the boat and plane tasks for the part
    // of the job that happens on the ground. Bits 15, 16, 17, 20, 26, 27, 28 and 31 are not named in
    // the script headers, so those names come from the old vMenu instead.
    public static readonly IReadOnlyList<DrivingFlag> Driving =
    [
        /* 01 */ new("DF_StopForCars", "Stop for cars", 1),
        /* 02 */ new("DF_StopForPeds", "Stop for pedestrians", 2),
        /* 03 */ new("DF_SwerveAroundAllCars", "Swerve around all cars", 4),
        /* 04 */ new("DF_SteerAroundStationaryCars", "Steer around parked cars", 8),
        /* 05 */ new("DF_SteerAroundPeds", "Steer around pedestrians", 16),
        /* 06 */ new("DF_SteerAroundObjects", "Steer around objects", 32),
        /* 07 */ new("DF_DontSteerAroundPlayerPed", "Do not steer around the player", 64),
        /* 08 */ new("DF_StopAtLights", "Stop at traffic lights", 128),
        /* 09 */ new("DF_GoOffRoadWhenAvoiding", "Go off road when avoiding", 256),
        /* 10 */ new("DF_DriveIntoOncomingTraffic", "Drive into oncoming traffic", 512),
        /* 11 */ new("DF_DriveInReverse", "Drive in reverse", 1024),
        /* 12 */ new("DF_UseWanderFallbackInsteadOfStraightLine", "Wander when no route is found", 2048),
        /* 13 */ new("DF_AvoidRestrictedAreas", "Avoid restricted areas", 4096),
        /* 14 */ new("DF_PreventBackgroundPathfinding", "Prevent background pathfinding", 8192),
        /* 15 */ new("DF_AdjustCruiseSpeedBasedOnRoadSpeed", "Match speed to the road", 16384),
        /* 16 */ new("DF_PreventJoinInRoadDirectionWhenMoving", "Do not join road direction while moving", 32768),
        /* 17 */ new("DF_DontAvoidTarget", "Do not avoid the target", 65536),
        /* 18 */ new("DF_TargetPositionOverridesEntity", "Target position beats target entity", 131072),
        /* 19 */ new("DF_UseShortCutLinks", "Use shortcut links", 262144),
        /* 20 */ new("DF_ChangeLanesAroundObstructions", "Change lanes around obstructions", 524288),
        /* 21 */ new("DF_AvoidTargetCoors", "Path away from the target", 1048576),
        /* 22 */ new("DF_UseSwitchedOffNodes", "Use switched off nodes", 2097152),
        /* 23 */ new("DF_PreferNavmeshRoute", "Prefer a navmesh route", 4194304),
        /* 24 */ new("DF_PlaneTaxiMode", "Plane taxi mode", 8388608),
        /* 25 */ new("DF_ForceStraightLine", "Force a straight line", 16777216),
        /* 26 */ new("DF_UseStringPullingAtJunctions", "Smooth turns at junctions", 33554432),
        /* 27 */ new("DF_AvoidAdverseConditions", "Avoid adverse conditions", 67108864),
        /* 28 */ new("DF_AvoidTurns", "Avoid turns", 134217728),
        /* 29 */ new("DF_ExtendRouteWithWanderResults", "Extend the route by wandering", 268435456),
        /* 30 */ new("DF_AvoidHighways", "Avoid highways", 536870912),
        /* 31 */ new("DF_ForceJoinInRoadDirection", "Force joining road direction", 1073741824),
        /* 32 */ new("DF_DontTerminateTaskWhenAchieved", "Do not end the task on arrival", int.MinValue),
    ];

    // BOATMODE, taken only by TASK_BOAT_MISSION.
    public static readonly IReadOnlyList<DrivingFlag> Boat =
    [
        new("BCF_StopAtEnd", "Stop at the end", 1),
        new("BCF_StopAtShore", "Stop at the shore", 2),
        new("BCF_AvoidShore", "Avoid the shore", 4),
        new("BCF_PreferForward", "Prefer moving forward", 8),
        new("BCF_NeverStop", "Never stop", 16),
        new("BCF_NeverNavMesh", "Never use the navmesh", 32),
        new("BCF_NeverRoute", "Never use a route", 64),
        new("BCF_ForceBeached", "Force beached", 128),
        new("BCF_UseWanderRoute", "Use a wander route", 256),
        new("BCF_UseFleeRoute", "Use a flee route", 512),
        new("BCF_NeverPause", "Never pause", 1024),
    ];

    // HELIMODE, taken by TASK_HELI_MISSION and TASK_VEHICLE_HELI_PROTECT.
    public static readonly IReadOnlyList<DrivingFlag> Heli =
    [
        new("HF_AttainRequestedOrientation", "Turn to the requested heading", 1),
        new("HF_DontModifyOrientation", "Do not change orientation", 2),
        new("HF_DontModifyPitch", "Do not change pitch", 4),
        new("HF_DontModifyThrottle", "Do not change throttle", 8),
        new("HF_DontModifyRoll", "Do not change roll", 16),
        new("HF_LandOnArrival", "Land on arrival", 32),
        new("HF_DontDoAvoidance", "Do not avoid anything", 64),
        new("HF_StartEngineImmediately", "Start the engine instantly", 128),
        new("HF_ForceHeightMapAvoidance", "Force height map avoidance", 256),
        new("HF_DontClampProbesToDestination", "Do not clamp probes to the target", 512),
        new("HF_EnableTimeslicingWhenPossible", "Allow timeslicing", 1024),
        new("HF_CircleOppositeDirection", "Circle the other way", 2048),
        new("HF_MaintainHeightAboveTerrain", "Hold height above terrain", 4096),
        new("HF_IgnoreHiddenEntitiesDuringLand", "Ignore hidden entities when landing", 8192),
        new("HF_DisableAllHeightMapAvoidance", "Disable all height map avoidance", 16384),
    ];

    // The eight combinations the game's own missions reuse, with the values straight out of the
    // script headers rather than recomputed, so they match what Rockstar actually shipped.
    public static readonly IReadOnlyList<DrivingFlag> DrivingPresets =
    [
        new("DRIVINGMODE_STOPFORCARS", "Normal", 786603),
        new("DRIVINGMODE_STOPFORCARS_STRICT", "Normal, never deviates", 262275),
        new("DRIVINGMODE_STOPFORCARS_IGNORELIGHTS", "Normal, ignores lights", 786475),
        new("DRIVINGMODE_AVOIDCARS", "Alerted", 786469),
        new("DRIVINGMODE_AVOIDCARS_RECKLESS", "Reckless", 786468),
        new("DRIVINGMODE_AVOIDCARS_OBEYLIGHTS", "Alerted, obeys lights", 786597),
        new("DRIVINGMODE_AVOIDCARS_STOPFORPEDS_OBEYLIGHTS", "Alerted, careful with peds", 786599),
        new("DRIVINGMODE_PLOUGHTHROUGH", "Plough through everything", 262144),
    ];

    public static readonly IReadOnlyList<DrivingFlag> BoatPresets =
    [
        new("BCF_DEFAULTSETTINGS", "Default", 7),
        new("BCF_OPENOCEANSETTINGS", "Open ocean", 111),
        new("BCF_BOATTAXISETTINGS", "Boat taxi", 1071),
    ];

    // Combinations rather than bits of their own, which is why they sit here and not in the Heli list.
    // Only the first two are the game's own: HF_HEIGHTMAPONLYAVOIDANCE is HF_DontDoAvoidance |
    // HF_ForceHeightMapAvoidance. The third is ours, because a helicopter that starts its engine and
    // keeps its distance from the ground is what almost everybody actually wants.
    public static readonly IReadOnlyList<DrivingFlag> HeliPresets =
    [
        new("HF_NONE", "No flags", 0),
        new("HF_HEIGHTMAPONLYAVOIDANCE", "Height map avoidance only", 320),

        // HF_StartEngineImmediately | HF_MaintainHeightAboveTerrain | HF_AttainRequestedOrientation.
        new("HF_StartEngineImmediately | HF_MaintainHeightAboveTerrain | HF_AttainRequestedOrientation", "Sensible flight", 4225),
    ];
}

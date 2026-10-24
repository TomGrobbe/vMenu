namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class AutoPilot
    {
        #region Menu chrome

        [English("Auto Pilot")]
        public const string Title = "autopilot.title";

        [English("Vehicle Auto Pilot")]
        public const string Subtitle = "autopilot.subtitle";

        [English(
            "Let your ped drive, sail or fly for you. Pick where to go, save the places you go often, and record routes to replay.")]
        public const string LinkDescription = "autopilot.link.desc";

        [English("Right Now")]
        public const string GroupStatus = "autopilot.group.status";

        [English("What your vehicle is currently doing.")]
        public const string GroupStatusDescription = "autopilot.group.status.desc";

        [English("Where To", "Give your ped somewhere to take you.")]
        public const string GroupStart = "autopilot.group.start";

        [English("Control", "Hold the current job, pick it back up, or call it off.")]
        public const string GroupControl = "autopilot.group.control";

        [English(
            "Driving Profiles In Use",
            "Which saved profile applies to each kind of vehicle. Which one applies is decided by what you are sitting in.")]
        public const string GroupProfiles = "autopilot.group.profiles";

        [English("Manage", "Build your own profiles, and keep your saved places and routes.")]
        public const string GroupManage = "autopilot.group.manage";

        #endregion

        #region Status

        [English("Idle")]
        public const string StatusIdle = "autopilot.status.idle";

        [English("Driving to your ~b~waypoint~s~")]
        public const string StatusWaypoint = "autopilot.status.waypoint";

        [English("Driving to ~b~{name}~s~")]
        public const string StatusPoint = "autopilot.status.point";

        [English("Driving around ~b~at random~s~")]
        public const string StatusWander = "autopilot.status.wander";

        [English("Replaying ~b~{name}~s~, point {index} of {count}")]
        public const string StatusPath = "autopilot.status.path";

        [English("~y~Paused~s~ · {what}")]
        public const string StatusPaused = "autopilot.status.paused";

        #endregion

        #region Starting a task

        [English(
            "Drive To Waypoint",
            "Take me to the waypoint I set on the map. Set one first, or there is nowhere to go.")]
        public const string DriveToWaypoint = "autopilot.drivetowaypoint";

        [English(
            "Drive Around Randomly",
            "Just drive, with no particular destination. Helicopters and planes pick a new spot every time they arrive.")]
        public const string DriveWander = "autopilot.drivewander";

        [English("Drive To Saved Point", "Pick one of the places you saved and go there.")]
        public const string DriveToPoint = "autopilot.drivetopoint";

        [English("Saved Points")]
        public const string DriveToPointSubtitle = "autopilot.drivetopoint.subtitle";

        [English("Replay Recorded Path", "Pick a route you recorded and drive it point by point.")]
        public const string ReplayPath = "autopilot.replaypath";

        [English("Recorded Paths")]
        public const string ReplayPathSubtitle = "autopilot.replaypath.subtitle";

        [English("Driving to ~g~{name}~s~. Use Stop whenever you want it to end.")]
        public const string Started = "autopilot.started";

        [English("Driving to your ~g~waypoint~s~. Use Stop whenever you want it to end.")]
        public const string StartedWaypoint = "autopilot.started.waypoint";

        [English("Driving around at random. Use Stop whenever you want it to end.")]
        public const string StartedWander = "autopilot.started.wander";

        [English("Replaying ~g~{name}~s~, {count} point(s) to go.")]
        public const string StartedPath = "autopilot.started.path";

        #endregion

        #region Control

        [English("Pause")]
        public const string Pause = "autopilot.pause";

        [English("Hold the current job where it is. Resume picks it back up from the same spot.")]
        public const string PauseDescription = "autopilot.pause.desc";

        [English("Resume")]
        public const string Resume = "autopilot.resume";

        [English("Carry on from where you paused.")]
        public const string ResumeDescription = "autopilot.resume.desc";

        [English("Stop", "End the job entirely. There is nothing left to resume afterwards.")]
        public const string Stop = "autopilot.stop";

        [English("Auto pilot paused.")]
        public const string Paused = "autopilot.paused";

        [English("Auto pilot resumed.")]
        public const string Resumed = "autopilot.resumed";

        [English("Auto pilot stopped.")]
        public const string Stopped = "autopilot.stopped";

        [English(
            "When Stopping",
            "What the vehicle does when you pause or stop. Parking only applies to road vehicles, everything else brakes instead.")]
        public const string StopAction = "autopilot.stopaction";

        [English("Park properly")]
        public const string StopActionPark = "autopilot.stopaction.park";

        [English("Brake and hold")]
        public const string StopActionBrake = "autopilot.stopaction.brake";

        [English("Coast to a stop")]
        public const string StopActionCoast = "autopilot.stopaction.coast";

        [English(
            "Cruise Speed",
            "How fast to travel, in metres per second. A profile with its own speed set overrides this.")]
        public const string CruiseSpeed = "autopilot.cruisespeed";

        [English("Vehicle maximum")]
        public const string CruiseSpeedAuto = "autopilot.cruisespeed.auto";

        #endregion

        #region Refusals

        [English("~r~You need to be in a vehicle first.")]
        public const string NoVehicle = "autopilot.novehicle";

        [English("~r~You have to be the one driving.")]
        public const string NotDriver = "autopilot.notdriver";

        [English("~r~Set a waypoint on the map first.")]
        public const string NoWaypoint = "autopilot.nowaypoint";

        #endregion

        #region Active profiles

        [English("Cars And Bikes", "The profile used for anything that drives on roads.")]
        public const string ProfileVehicle = "autopilot.profile.vehicle";

        [English("Planes", "The profile used for planes.")]
        public const string ProfilePlane = "autopilot.profile.plane";

        [English("Boats", "The profile used for boats.")]
        public const string ProfileBoat = "autopilot.profile.boat";

        [English("Helicopters", "The profile used for helicopters.")]
        public const string ProfileHeli = "autopilot.profile.heli";

        #endregion

        #region Profiles menu

        [English("Driving Profiles")]
        public const string ProfilesTitle = "autopilot.profiles.title";

        [English("Driving Profiles")]
        public const string ProfilesSubtitle = "autopilot.profiles.subtitle";

        [English("Build and keep your own sets of driving behaviour, one list per kind of vehicle.")]
        public const string ProfilesLinkDescription = "autopilot.profiles.link.desc";

        [English("Cars And Bikes")]
        public const string CategoryVehicle = "autopilot.category.vehicle";

        [English("Planes")]
        public const string CategoryPlane = "autopilot.category.plane";

        [English("Boats")]
        public const string CategoryBoat = "autopilot.category.boat";

        [English("Helicopters")]
        public const string CategoryHelicopter = "autopilot.category.helicopter";

        [English("{count} profile(s) here, {presets} of them built in.")]
        public const string CategoryDescription = "autopilot.category.desc";

        [English("Create Profile", "Start a new empty profile in this category, then tick the behaviour you want.")]
        public const string CreateProfile = "autopilot.profile.create";

        [English("Name this profile")]
        public const string ProfileNamePrompt = "autopilot.profile.name.prompt";

        [English("Description (optional)")]
        public const string ProfileDescriptionPrompt = "autopilot.profile.desc.prompt";

        [English("Saved ~g~{name}~s~.")]
        public const string ProfileSaved = "autopilot.profile.saved";

        [English("~r~You already have a profile called {name}.")]
        public const string ProfileNameTaken = "autopilot.profile.nametaken";

        [English("~r~That could not be saved. A newer version of vMenu may have written it.")]
        public const string ProfileRefused = "autopilot.profile.refused";

        [English("~r~{name} is a built in profile, so that name is taken. Pick another.")]
        public const string ProfileNameReserved = "autopilot.profile.namereserved";

        [English("{description}\nValue: ~b~{value}~s~")]
        public const string ProfileRowDescription = "autopilot.profile.row.desc";

        [English("Built in. ~b~{description}~s~\nValue: ~b~{value}~s~\nDuplicate it to get a copy you can change.")]
        public const string ProfilePresetDescription = "autopilot.profile.preset.desc";

        [English("Value: ~b~{value}~s~   ({hex})")]
        public const string ProfileValue = "autopilot.profile.value";

        [English("The number your ticks add up to. This is exactly what gets handed to the game.")]
        public const string ProfileValueDescription = "autopilot.profile.value.desc";

        [English("{flag}")]
        public const string ProfileFlagDescription = "autopilot.profile.flag.desc";

        [English("This one is built in and cannot be changed. Duplicate it to get a copy you can edit.")]
        public const string ProfilePresetLocked = "autopilot.profile.preset.locked";

        [English("~r~This was saved by a newer version of vMenu, so it cannot be changed here.")]
        public const string ProfileNewerBuild = "autopilot.profile.newerbuild";

        [English("Rename", "Change the name and description of this profile.")]
        public const string ProfileRename = "autopilot.profile.rename";

        [English("New name")]
        public const string ProfileRenamePrompt = "autopilot.profile.rename.prompt";

        [English("Renamed to ~g~{name}~s~.")]
        public const string ProfileRenamed = "autopilot.profile.renamed";

        [English("Duplicate", "Make a copy of this profile under a new name.")]
        public const string ProfileDuplicate = "autopilot.profile.duplicate";

        [English("Name for the copy")]
        public const string ProfileDuplicatePrompt = "autopilot.profile.duplicate.prompt";

        [English("Use This Profile", "Make this the profile every vehicle in this category uses.")]
        public const string ProfileUse = "autopilot.profile.use";

        [English("~g~{name}~s~ is now in use.")]
        public const string ProfileInUse = "autopilot.profile.inuse";

        [English("~r~Delete Profile", "Remove this profile for good.")]
        public const string ProfileDelete = "autopilot.profile.delete";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string ProfileDeleteConfirm = "autopilot.profile.delete.confirm";

        [English("~g~{name}~s~ was deleted.")]
        public const string ProfileDeleted = "autopilot.profile.deleted";

        [English("Flight Height", "How high to cruise, in metres.")]
        public const string ProfileFlightHeight = "autopilot.profile.flightheight";

        [English("Minimum Height Above Ground", "How much clearance to keep over the terrain, in metres.")]
        public const string ProfileMinHeight = "autopilot.profile.minheight";

        [English(
            "Cruise Speed",
            "Speed for this profile, in metres per second. Zero means use the vehicle maximum instead.")]
        public const string ProfileCruiseSpeed = "autopilot.profile.cruisespeed";

        [English(
            "Line Up Precisely",
            "Take more care lining up with the destination. Slower, but it arrives where you meant.")]
        public const string ProfilePrecise = "autopilot.profile.precise";

        [English(
            "No profiles of your own yet",
            "Only the built in ones are here. Create one, or duplicate a built in profile to get started.")]
        public const string ProfilesEmpty = "autopilot.profiles.empty";

        #endregion

        #region Points and paths menu

        [English("Points And Paths")]
        public const string RoutesTitle = "autopilot.routes.title";

        [English("Points And Paths")]
        public const string RoutesSubtitle = "autopilot.routes.subtitle";

        [English("The places you saved, and the routes you recorded.")]
        public const string RoutesLinkDescription = "autopilot.routes.link.desc";

        [English("Saved Points", "Single places you can be driven to.")]
        public const string GroupPoints = "autopilot.group.points";

        [English("Recorded Paths", "Routes made of many points, replayed in order.")]
        public const string GroupPaths = "autopilot.group.paths";

        [English("Save Where I Am", "Remember the spot you are standing or parked on right now.")]
        public const string SavePosition = "autopilot.point.savehere";

        [English("Save My Waypoint", "Remember the place your map waypoint is pointing at.")]
        public const string SaveWaypoint = "autopilot.point.savewaypoint";

        [English("Name this place")]
        public const string PointNamePrompt = "autopilot.point.name.prompt";

        [English("Description (optional)")]
        public const string PointDescriptionPrompt = "autopilot.point.desc.prompt";

        [English("Saved ~g~{name}~s~.")]
        public const string PointSaved = "autopilot.point.saved";

        [English("~r~You already have a place called {name}.")]
        public const string PointNameTaken = "autopilot.point.nametaken";

        [English("~r~That could not be saved. A newer version of vMenu may have written it.")]
        public const string PointRefused = "autopilot.point.refused";

        [English("No saved points yet", "Save where you are, or save your waypoint, and it will show up here.")]
        public const string PointsEmpty = "autopilot.points.empty";

        [English("{description}")]
        public const string PointRowDescription = "autopilot.point.row.desc";

        [English("Drive Here", "Set off for this place now.")]
        public const string PointDriveHere = "autopilot.point.drivehere";

        [English("Rename", "Change the name and description of this place.")]
        public const string PointRename = "autopilot.point.rename";

        [English("Renamed to ~g~{name}~s~.")]
        public const string PointRenamed = "autopilot.point.renamed";

        [English("~r~Delete Point", "Forget this place for good.")]
        public const string PointDelete = "autopilot.point.delete";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string PointDeleteConfirm = "autopilot.point.delete.confirm";

        [English("~g~{name}~s~ was deleted.")]
        public const string PointDeleted = "autopilot.point.deleted";

        [English("No recorded paths yet", "Record one and it will show up here, ready to replay.")]
        public const string PathsEmpty = "autopilot.paths.empty";

        [English("{description}\n~b~{count}~s~ point(s), about ~b~{length}~s~ m long.")]
        public const string PathRowDescription = "autopilot.path.row.desc";

        [English("Replay This Path", "Drive the route from its first point to its last.")]
        public const string PathReplay = "autopilot.path.replay";

        [English("Rename", "Change the name and description of this path.")]
        public const string PathRename = "autopilot.path.rename";

        [English("Renamed to ~g~{name}~s~.")]
        public const string PathRenamed = "autopilot.path.renamed";

        [English("~r~Delete Path", "Remove this route and every point in it.")]
        public const string PathDelete = "autopilot.path.delete";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string PathDeleteConfirm = "autopilot.path.delete.confirm";

        [English("~g~{name}~s~ was deleted.")]
        public const string PathDeleted = "autopilot.path.deleted";

        [English("Points", "Look through the points in this route and take any of them out.")]
        public const string PathPointsList = "autopilot.path.points";

        [English("Points In This Path")]
        public const string PathPointsListSubtitle = "autopilot.path.points.subtitle";

        [English("Point {index}")]
        public const string PathPointRow = "autopilot.path.point.row";

        [English("At {x}, {y}, {z}.")]
        public const string PathPointRowDescription = "autopilot.path.point.row.desc";

        [English("~r~Remove point {index}? Press enter again to confirm.")]
        public const string PathPointDeleteConfirm = "autopilot.path.point.delete.confirm";

        [English("Point removed.")]
        public const string PathPointRemoved = "autopilot.path.point.removed";

        [English(
            "~r~A path needs at least one point, so the last one cannot be removed. Delete the whole path instead.")]
        public const string PathPointLast = "autopilot.path.point.last";

        #endregion

        #region Recorder

        [English("Record New Path")]
        public const string RecordPath = "autopilot.record";

        [English("Drive a route and have it remembered, so you can replay it later.")]
        public const string RecordPathDescription = "autopilot.record.desc";

        [English("Path Recorder")]
        public const string RecordPathSubtitle = "autopilot.record.subtitle";

        [English("Not recording")]
        public const string RecorderIdle = "autopilot.record.idle";

        [English("~g~Recording~s~ · {count} point(s) · about {length} m")]
        public const string RecorderRunning = "autopilot.record.running";

        [English("~y~Stopped~s~ · {count} point(s) · about {length} m")]
        public const string RecorderHeld = "autopilot.record.held";

        [English("Start Recording")]
        public const string RecorderStart = "autopilot.record.start";

        [English("Begin dropping points as you drive.")]
        public const string RecorderStartDescription = "autopilot.record.start.desc";

        [English("Stop Recording")]
        public const string RecorderStop = "autopilot.record.stop";

        [English("Stop dropping points. What you have so far is kept.")]
        public const string RecorderStopDescription = "autopilot.record.stop.desc";

        [English(
            "Drop Point Here",
            "Add a point at exactly where you are right now, whether or not automatic points are on.")]
        public const string RecorderDrop = "autopilot.record.drop";

        [English("Point added, {count} so far.")]
        public const string RecorderDropped = "autopilot.record.dropped";

        [English("~r~This path is full at {count} points. Save it and start another.")]
        public const string RecorderFull = "autopilot.record.full";

        [English("Undo Last Point", "Take the most recent point back off again.")]
        public const string RecorderUndo = "autopilot.record.undo";

        [English("Point removed, {count} left.")]
        public const string RecorderUndone = "autopilot.record.undone";

        [English(
            "Automatic Point Spacing",
            "How far you have to move before another point drops by itself, in metres.")]
        public const string RecorderSpacing = "autopilot.record.spacing";

        [English(
            "Automatic Points",
            "Drop points on their own as you drive. Turn this off to place every point by hand.")]
        public const string RecorderAuto = "autopilot.record.auto";

        [English("Save Path", "Give this route a name and keep it.")]
        public const string RecorderSave = "autopilot.record.save";

        [English("Name this path")]
        public const string RecorderNamePrompt = "autopilot.record.name.prompt";

        [English("Description (optional)")]
        public const string RecorderDescriptionPrompt = "autopilot.record.desc.prompt";

        [English("Saved ~g~{name}~s~ with {count} point(s).")]
        public const string RecorderSaved = "autopilot.record.saved";

        [English("~r~You already have a path called {name}.")]
        public const string RecorderNameTaken = "autopilot.record.nametaken";

        [English("~r~That could not be saved. A newer version of vMenu may have written it.")]
        public const string RecorderRefused = "autopilot.record.refused";

        [English("~r~There is nothing to save yet. Record or drop at least one point first.")]
        public const string RecorderEmpty = "autopilot.record.empty";

        [English("~r~Discard", "Throw away everything recorded so far and start over.")]
        public const string RecorderDiscard = "autopilot.record.discard";

        [English("~r~Throw away all {count} point(s)? Press enter again to confirm.")]
        public const string RecorderDiscardConfirm = "autopilot.record.discard.confirm";

        [English("Recording discarded.")]
        public const string RecorderDiscarded = "autopilot.record.discarded";

        #endregion
    }
}

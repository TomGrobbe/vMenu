namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class PersonalVehicle
    {
        [English("Personal Vehicle")]
        public const string Title = "personalvehicle.title";

        [English("Personal Vehicle Menu")]
        public const string Subtitle = "personalvehicle.subtitle";

        [English(
            "Mark a vehicle you spawned as your own, keep an eye on it from anywhere, and empty it or delete it wherever it has got to.")]
        public const string LinkDescription = "personalvehicle.link.desc";

        [English("Personal Vehicle")]
        public const string BlipName = "personalvehicle.blip.name";

        [English("Set Current Vehicle")]
        public const string SetCurrent = "personalvehicle.setcurrent";

        [English("Marks the vehicle you are driving as your personal vehicle. It has to be one you spawned yourself.")]
        public const string SetCurrentDescription = "personalvehicle.setcurrent.desc";

        [English("Current Vehicle")]
        public const string Status = "personalvehicle.status";

        [English("None")]
        public const string StatusNone = "personalvehicle.status.none";

        [English("You have not marked a vehicle yet. Spawn one, get in it, and pick Set Current Vehicle above.")]
        public const string StatusNoneDescription = "personalvehicle.status.none.desc";

        [English("Nobody is in your vehicle at the moment.")]
        public const string StatusEmpty = "personalvehicle.status.empty";

        [English("Currently occupied by ~y~{players}~s~.")]
        public const string StatusOccupied = "personalvehicle.status.occupied";

        [English(
            "Your vehicle is in a different routing bucket, so its position cannot be shown. You can still empty it or delete it.")]
        public const string StatusOtherWorld = "personalvehicle.status.otherworld";

        [English("Show Blip")]
        public const string ShowBlip = "personalvehicle.blip";

        [English(
            "Puts a blip on the map for your personal vehicle and keeps it there wherever the vehicle goes, even when somebody else is driving it.")]
        public const string ShowBlipDescription = "personalvehicle.blip.desc";

        [English("Set Waypoint To Vehicle")]
        public const string Waypoint = "personalvehicle.waypoint";

        [English("Marks your personal vehicle as your waypoint, so the map draws a route to it.")]
        public const string WaypointDescription = "personalvehicle.waypoint.desc";

        [English("Waypoint set to your personal vehicle.")]
        public const string WaypointSet = "personalvehicle.waypoint.set";

        [English("Kick Everyone Out")]
        public const string Kick = "personalvehicle.kick";

        [English(
            "Throws everybody else out of your personal vehicle, whether they want to leave or not. You keep your own seat, so this works whether you are in it or on the other side of the map.")]
        public const string KickDescription = "personalvehicle.kick.desc";

        [English("Threw ~y~{count}~s~ player(s) out.")]
        public const string Kicked = "personalvehicle.kick.done";

        [English("There was nobody else in your vehicle.")]
        public const string KickedNobody = "personalvehicle.kick.nobody";

        [English("Could not empty your personal vehicle.")]
        public const string KickFailed = "personalvehicle.kick.failed";

        [English("The owner of this vehicle threw you out of it.")]
        public const string ThrownOut = "personalvehicle.kick.thrownout";

        [English("Delete Vehicle")]
        public const string Delete = "personalvehicle.delete";

        [English("Removes your personal vehicle from the world, wherever it happens to be.")]
        public const string DeleteDescription = "personalvehicle.delete.desc";

        [English("Are you sure? Your personal vehicle will be removed wherever it is. Press again to confirm.")]
        public const string DeleteConfirm = "personalvehicle.delete.confirm";

        [English("Your personal vehicle has been deleted.")]
        public const string Deleted = "personalvehicle.delete.done";

        [English("Could not delete your personal vehicle.")]
        public const string DeleteFailed = "personalvehicle.delete.failed";

        [English("Forget Vehicle")]
        public const string Forget = "personalvehicle.forget";

        [English(
            "Stops treating this vehicle as yours and takes its blip off the map. The vehicle itself is left where it is.")]
        public const string ForgetDescription = "personalvehicle.forget.desc";

        [English("That is no longer your personal vehicle.")]
        public const string Forgotten = "personalvehicle.forget.done";

        [English("~g~{vehicle}~s~ is now your personal vehicle.")]
        public const string Set = "personalvehicle.set.done";

        [English("Could not mark that vehicle as yours.")]
        public const string SetFailed = "personalvehicle.set.failed";

        [English("You can only mark a vehicle you spawned yourself.")]
        public const string NotSpawnedByYou = "personalvehicle.set.notyours";

        [English("You are not in a vehicle, so there is nothing to mark as yours.")]
        public const string NoVehicle = "personalvehicle.novehicle";

        [English("You have to be driving a vehicle to mark it as yours.")]
        public const string NotDriver = "personalvehicle.notdriver";

        [English("You have not marked a personal vehicle yet.")]
        public const string NoneMarked = "personalvehicle.nonemarked";

        [English("Nothing has been heard about your vehicle yet. Try again in a moment.")]
        public const string NoPosition = "personalvehicle.noposition";

        [English("Your personal vehicle is gone.")]
        public const string Gone = "personalvehicle.gone";

        [English("You are not allowed to do that to your personal vehicle.")]
        public const string Denied = "personalvehicle.denied";

        [English("You are doing that too quickly. Wait a moment and try again.")]
        public const string TooFast = "personalvehicle.toofast";

        [English("Nobody is close enough to your vehicle to do that to it right now.")]
        public const string NobodyNearby = "personalvehicle.nobodynearby";

        [English("Lock Doors")]
        public const string Lock = "personalvehicle.lock";

        [English(
            "Locks or unlocks your personal vehicle from wherever you are. Somebody has to be near it for this to reach.")]
        public const string LockDescription = "personalvehicle.lock.desc";

        [English("Your vehicle is now ~y~locked~s~.")]
        public const string Locked = "personalvehicle.lock.locked";

        [English("Your vehicle is now ~g~unlocked~s~.")]
        public const string Unlocked = "personalvehicle.lock.unlocked";

        [English("Could not change the locks on your vehicle.")]
        public const string LockFailed = "personalvehicle.lock.failed";

        [English("Engine")]
        public const string Engine = "personalvehicle.engine";

        [English("Starts or stops your personal vehicle's engine, wherever it is parked.")]
        public const string EngineDescription = "personalvehicle.engine.desc";

        [English("Your vehicle's engine is ~g~running~s~.")]
        public const string EngineStarted = "personalvehicle.engine.started";

        [English("Your vehicle's engine is ~y~off~s~.")]
        public const string EngineStopped = "personalvehicle.engine.stopped";

        [English("Could not touch your vehicle's engine.")]
        public const string EngineFailed = "personalvehicle.engine.failed";

        [English("Lights")]
        public const string Lights = "personalvehicle.lights";

        [English("Leaves your vehicle's lights to the game, forces them off, or forces them on.")]
        public const string LightsDescription = "personalvehicle.lights.desc";

        [English("Automatic")]
        public const string LightsAutomatic = "personalvehicle.lights.automatic";

        [English("Off")]
        public const string LightsOff = "personalvehicle.lights.off";

        [English("On")]
        public const string LightsOn = "personalvehicle.lights.on";

        [English("Your vehicle's lights have been set.")]
        public const string LightsSet = "personalvehicle.lights.set";

        [English("Could not change your vehicle's lights.")]
        public const string LightsFailed = "personalvehicle.lights.failed";

        [English("Doors")]
        public const string Doors = "personalvehicle.doors";

        [English("Opens and closes the doors, bonnet and boot of your personal vehicle from a distance.")]
        public const string DoorsDescription = "personalvehicle.doors.desc";

        [English("Personal Vehicle Doors")]
        public const string DoorsSubtitle = "personalvehicle.doors.subtitle";

        [English("Opens this door if it is closed, and closes it if it is open.")]
        public const string DoorDescription = "personalvehicle.door.desc";

        [English("Your vehicle's door has been moved.")]
        public const string DoorToggled = "personalvehicle.door.toggled";

        [English("Could not move your vehicle's doors.")]
        public const string DoorFailed = "personalvehicle.door.failed";

        [English("Open All Doors")]
        public const string OpenAllDoors = "personalvehicle.doors.openall";

        [English("Opens every door your vehicle has, including the bonnet and the boot.")]
        public const string OpenAllDoorsDescription = "personalvehicle.doors.openall.desc";

        [English("Close All Doors")]
        public const string ShutAllDoors = "personalvehicle.doors.shutall";

        [English("Closes every door your vehicle has, including the bonnet and the boot.")]
        public const string ShutAllDoorsDescription = "personalvehicle.doors.shutall.desc";

        [English("Your vehicle's doors are ~y~open~s~.")]
        public const string DoorsOpened = "personalvehicle.doors.opened";

        [English("Your vehicle's doors are ~g~closed~s~.")]
        public const string DoorsShut = "personalvehicle.doors.closed";

        [English("Windows")]
        public const string Windows = "personalvehicle.windows";

        [English("Winds the windows of your personal vehicle up or down from a distance.")]
        public const string WindowsDescription = "personalvehicle.windows.desc";

        [English("Personal Vehicle Windows")]
        public const string WindowsSubtitle = "personalvehicle.windows.subtitle";

        [English("The game will not say whether this window is up or down, so pick what you want it to be.")]
        public const string WindowDescription = "personalvehicle.window.desc";

        [English("Roll Up")]
        public const string WindowRollUp = "personalvehicle.window.rollup";

        [English("Roll Down")]
        public const string WindowRollDown = "personalvehicle.window.rolldown";

        [English("That window is ~g~up~s~.")]
        public const string WindowUp = "personalvehicle.window.up";

        [English("That window is ~y~down~s~.")]
        public const string WindowDown = "personalvehicle.window.down";

        [English("Could not move your vehicle's windows.")]
        public const string WindowFailed = "personalvehicle.window.failed";

        [English("Roll Up All Windows")]
        public const string RollUpAllWindows = "personalvehicle.windows.upall";

        [English("Winds every window of your vehicle back up.")]
        public const string RollUpAllWindowsDescription = "personalvehicle.windows.upall.desc";

        [English("Roll Down All Windows")]
        public const string RollDownAllWindows = "personalvehicle.windows.downall";

        [English("Winds every window of your vehicle all the way down.")]
        public const string RollDownAllWindowsDescription = "personalvehicle.windows.downall.desc";

        [English("Your vehicle's windows are ~g~up~s~.")]
        public const string WindowsUp = "personalvehicle.windows.up";

        [English("Your vehicle's windows are ~y~down~s~.")]
        public const string WindowsDown = "personalvehicle.windows.down";

        [English("Play Horn Tune")]
        public const string Horn = "personalvehicle.horn";

        [English("Sounds a little tune on your vehicle's horn, picked at random. Everybody near it hears it.")]
        public const string HornDescription = "personalvehicle.horn.desc";

        [English("Your vehicle is sounding off.")]
        public const string HornPlayed = "personalvehicle.horn.played";

        [English("Could not sound your vehicle's horn.")]
        public const string HornFailed = "personalvehicle.horn.failed";

        [English("Explode Vehicle")]
        public const string Explode = "personalvehicle.explode";

        [English("Blows your personal vehicle up where it stands, and forgets it afterwards.")]
        public const string ExplodeDescription = "personalvehicle.explode.desc";

        [English("~r~Are you sure you want to blow this vehicle up? Press enter again to confirm.")]
        public const string ExplodeConfirm = "personalvehicle.explode.confirm";

        [English("~r~Your personal vehicle has been blown up~s~.")]
        public const string Exploded = "personalvehicle.explode.done";

        [English("Could not blow your vehicle up.")]
        public const string ExplodeFailed = "personalvehicle.explode.failed";

        [English("Your vehicle has ~y~vehicle god mode~s~ on, so it cannot be blown up.")]
        public const string ExplodeGodMode = "personalvehicle.explode.godmode";

        [English("Electric Door Handles")]
        public const string Electrocute = "personalvehicle.electrocute";

        [English(
            "Anybody except you who tries to open a door of your personal vehicle, or grabs its handlebars, gets an electric shock.")]
        public const string ElectrocuteDescription = "personalvehicle.electrocute.desc";

        [English("Your personal vehicle now shocks anybody who touches its doors.")]
        public const string ElectrocuteOn = "personalvehicle.electrocute.on";

        [English("Your personal vehicle no longer shocks people.")]
        public const string ElectrocuteOff = "personalvehicle.electrocute.off";

        [English("Could not change the electric door handles on your personal vehicle.")]
        public const string ElectrocuteFailed = "personalvehicle.electrocute.failed";
    }
}

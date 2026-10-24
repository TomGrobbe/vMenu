namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class Admin
    {
        [English("Admin")]
        public const string Title = "admin.title";

        [English("Staff Tools")]
        public const string Subtitle = "admin.subtitle";

        [English("The tools that only your staff team can reach. Nobody without staff access can even see this menu.")]
        public const string LinkDescription = "admin.link.desc";

        [English("Players")]
        public const string PlayersGroup = "admin.players.group";

        [English("Things you can do to whoever is standing closest to you.")]
        public const string PlayersGroupDescription = "admin.players.group.desc";

        [English("Vehicles")]
        public const string VehiclesGroup = "admin.vehicles.group";

        [English("Getting rid of vehicles, one at a time or all at once.")]
        public const string VehiclesGroupDescription = "admin.vehicles.group.desc";

        [English("Server")]
        public const string ServerGroup = "admin.server.group";

        [English("Things that affect everybody on the server at once.")]
        public const string ServerGroupDescription = "admin.server.group.desc";

        [English("There is ~y~nobody~s~ close enough to you for that.")]
        public const string NobodyNearby = "admin.nobodynearby";

        [English("They moved out of reach before that went through.")]
        public const string MovedAway = "admin.movedaway";

        [English("The player you were carrying has left, so you are not carrying anybody now.")]
        public const string CarriedLeft = "admin.carriedleft";

        [English("That did not work.")]
        public const string Failed = "admin.failed";

        [English("You are not allowed to do that.")]
        public const string Denied = "admin.denied";

        [English("You are doing that too often. Give it ~y~{seconds}~s~ second(s).")]
        public const string TooFast = "admin.toofast";

        [English("~y~{player}~s~ is not on the server any more.")]
        public const string NotFound = "admin.notfound";

        [English("~y~{player}~s~ is still loading in, so there is nothing to act on yet.")]
        public const string StillJoining = "admin.stilljoining";

        [English("Clear Area")]
        public const string ClearArea = "admin.cleararea";

        [English(
            "Tidy the world up around you. Wrecks, dropped props, wandering people, scorch marks, dirt and broken street lights all go back to how they started. This is not only for you: everybody standing near you has the same patch of world cleaned up at the same moment.")]
        public const string ClearAreaDescription = "admin.cleararea.desc";

        [English(
            "~r~Are you sure?~s~ This clears the world for everybody near you, not only for you. Press again to go ahead.")]
        public const string ClearAreaConfirm = "admin.cleararea.confirm";

        [English("The area around you has been cleared.")]
        public const string ClearAreaDone = "admin.cleararea.done";

        [English("You are clearing the area too often. Give it a moment.")]
        public const string ClearAreaTooFast = "admin.cleararea.toofast";

        [English("You are not allowed to clear the area.")]
        public const string ClearAreaDenied = "admin.cleararea.denied";

        [English("The area could not be cleared.")]
        public const string ClearAreaFailed = "admin.cleararea.failed";

        [English("Freeze Closest Player")]
        public const string Freeze = "admin.freeze";

        [English(
            "Pin whoever is standing closest to you to the spot, so they cannot walk, run or drive away. Press it again on the same person to let them move.")]
        public const string FreezeDescription = "admin.freeze.desc";

        [English("~y~{player}~s~ has been frozen.")]
        public const string FreezeDone = "admin.freeze.done";

        [English("~g~{player}~s~ can move again.")]
        public const string UnfreezeDone = "admin.unfreeze.done";

        [English("A staff member has ~y~frozen~s~ you. Please wait for them.")]
        public const string FrozenByStaff = "admin.frozen.byStaff";

        [English("You can ~g~move again~s~.")]
        public const string UnfrozenByStaff = "admin.unfrozen.byStaff";

        [English("Grab Closest Player")]
        public const string Grab = "admin.grab";

        [English(
            "Pick up whoever is standing closest to you and carry them with you wherever you go. They cannot get away on their own, so remember to put them down again.")]
        public const string GrabDescription = "admin.grab.desc";

        [English("Release Player")]
        public const string Release = "admin.release";

        [English("Put down the player you are carrying. They do not have to be anywhere near you for this to work.")]
        public const string ReleaseDescription = "admin.release.desc";

        [English("You are carrying ~y~{player}~s~.")]
        public const string GrabDone = "admin.grab.done";

        [English("You put ~g~{player}~s~ down.")]
        public const string ReleaseDone = "admin.release.done";

        [English("~y~{player}~s~ is already being carried by another staff member.")]
        public const string GrabTaken = "admin.grab.taken";

        [English("A staff member has ~y~picked you up~s~.")]
        public const string GrabbedByStaff = "admin.grabbed.byStaff";

        [English("You have been ~g~put down~s~.")]
        public const string ReleasedByStaff = "admin.released.byStaff";

        [English("Delete Vehicle")]
        public const string DeleteVehicle = "admin.deletevehicle";

        [English(
            "Delete the vehicle in front of you, or the one you are sat in whether you are driving it or not. Ordinary players can only delete a vehicle they are driving.")]
        public const string DeleteVehicleDescription = "admin.deletevehicle.desc";

        [English("Delete Empty Vehicles")]
        public const string DeleteEmptyVehicles = "admin.deleteemptyvehicles";

        [English(
            "Removes vehicles nobody is sat in, anywhere on the server. The map keeps its own traffic, and anything with somebody in it is left alone, so this only clears up what players spawned and walked away from.")]
        public const string DeleteEmptyVehiclesDescription = "admin.deleteemptyvehicles.desc";

        [English(
            "~r~Are you sure?~s~ Every player spawned vehicle nobody is sat in goes, everywhere, for everybody. Press again to go ahead.")]
        public const string DeleteEmptyVehiclesConfirm = "admin.deleteemptyvehicles.confirm";

        [English("Delete All Vehicles")]
        public const string DeleteAllVehicles = "admin.deleteallvehicles";

        [English(
            "Remove every vehicle on the whole server, including the ones people are sat in right now. Only reach for this when a wipe is really what you want.")]
        public const string DeleteAllVehiclesDescription = "admin.deleteallvehicles.desc";

        [English(
            "~r~Are you really sure?~s~ This yanks people out of vehicles they are driving, across the entire server. Press again to go ahead.")]
        public const string DeleteAllVehiclesConfirm = "admin.deleteallvehicles.confirm";

        [English("Deleted ~g~{count}~s~ vehicle(s).")]
        public const string DeleteVehiclesDone = "admin.deletevehicles.done";

        [English("Send Announcement")]
        public const string Announce = "admin.announce";

        [English("Put a message on the screen of everybody on the server, wherever they happen to be.")]
        public const string AnnounceDescription = "admin.announce.desc";

        [English("What should everybody be told?")]
        public const string AnnouncePrompt = "admin.announce.prompt";

        [English("An empty announcement tells nobody anything, so write a few words first.")]
        public const string AnnounceEmpty = "admin.announce.empty";

        [English("Your announcement went out to ~g~{count}~s~ player(s).")]
        public const string AnnounceDone = "admin.announce.done";

        [English("{text}")]
        public const string AnnounceBanner = "admin.announce.banner";

        [English("ANNOUNCEMENT")]
        public const string AnnounceHeading = "admin.announce.heading";

        [English("Scheduled Announcements")]
        public const string Schedule = "admin.schedule";

        [English("Announcements that send themselves, either every so many minutes or at a set time of day.")]
        public const string ScheduleDescription = "admin.schedule.desc";

        [English("{count} scheduled")]
        public const string ScheduleSubtitle = "admin.schedule.subtitle";

        [English("Nothing Scheduled")]
        public const string ScheduleEmpty = "admin.schedule.empty";

        [English(
            "No announcements are set to send themselves. Add one here, or fill in config/announcements.json on the server.")]
        public const string ScheduleEmptyDescription = "admin.schedule.empty.desc";

        [English("every {minutes}m")]
        public const string ScheduleEveryLabel = "admin.schedule.every";

        [English("at {time}")]
        public const string ScheduleAtLabel = "admin.schedule.at";

        [English(" ~g~G~s~")]
        public const string ScheduleGameMark = "admin.schedule.gamemark";

        [English("Every {minutes} minute(s)")]
        public const string ScheduleEveryReal = "admin.schedule.every.real";

        [English("Every {minutes} ~g~game~s~ minute(s)")]
        public const string ScheduleEveryGame = "admin.schedule.every.game";

        [English("At {time}")]
        public const string ScheduleAtReal = "admin.schedule.at.real";

        [English("At {time} ~g~game time~s~")]
        public const string ScheduleAtGame = "admin.schedule.at.game";

        [English(
            "Type @ and a number of minutes for a repeat, like `@45`. Or type a time of day on a 24 hour clock, like `20:00`. Add `game` on the end of either to use the clock inside GTA instead of the real one, so `@5 game` repeats every 5 in game minutes and `20:00 game` lands at eight in the evening in game.")]
        public const string ScheduleTriggerHelp = "admin.schedule.trigger.help";

        [English("Only you and other staff ever see this. It is what the announcement is called in this list.")]
        public const string ScheduleNameHelp = "admin.schedule.name.help";

        [English("This is what every player on the server reads. Colour codes like ~y~ work here.")]
        public const string ScheduleTextHelp = "admin.schedule.text.help";

        [English("{trigger} · {text}\n~y~Press enter to manage announcement.")]
        public const string ScheduleRowDescription = "admin.schedule.row.desc";

        [English("Switched Off")]
        public const string ScheduleDisabled = "admin.schedule.disabled";

        [English(
            "Scheduled announcements are switched off on this server, so nothing listed here will send itself. Set vMenu.Enhanced.Admin.ScheduledAnnouncements to true in your configuration.cfg to turn them back on.")]
        public const string ScheduleDisabledDescription = "admin.schedule.disabled.desc";

        [English("Add Announcement")]
        public const string ScheduleAdd = "admin.schedule.add";

        [English("Write a new scheduled announcement and save it to the server.")]
        public const string ScheduleAddDescription = "admin.schedule.add.desc";

        [English("What should this announcement be called?")]
        public const string ScheduleNamePrompt = "admin.schedule.name.prompt";

        [English("What should the announcement say?")]
        public const string ScheduleTextPrompt = "admin.schedule.text.prompt";

        [English("When should this announcement send?")]
        public const string ScheduleTriggerPrompt = "admin.schedule.trigger.prompt";

        [English(
            "That is not a repeat or a time of day. Try ~b~@45~s~ for every 45 minutes, or ~b~20:00~s~ for a time.")]
        public const string ScheduleTriggerInvalid = "admin.schedule.trigger.invalid";

        [English("~g~{name}~s~ has been added to the schedule.")]
        public const string ScheduleAdded = "admin.schedule.added";

        [English("Remove")]
        public const string ScheduleRemove = "admin.schedule.remove";

        [English("Take this announcement off the schedule for good.")]
        public const string ScheduleRemoveDescription = "admin.schedule.remove.desc";

        [English("~y~{name}~s~ has been taken off the schedule.")]
        public const string ScheduleRemoved = "admin.schedule.removed";

        [English("Refresh All Permissions")]
        public const string RefreshPermissions = "admin.refreshpermissions";

        [English(
            "Hand everybody on the server a fresh copy of what they are allowed to do. Use it once you have actually changed somebody's permissions on the server, so their menu catches up without them having to rejoin.")]
        public const string RefreshPermissionsDescription = "admin.refreshpermissions.desc";

        [English("Refreshed permissions for ~g~{count}~s~ player(s).")]
        public const string RefreshPermissionsDone = "admin.refreshpermissions.done";

        [English("Return To The Default World")]
        public const string ResetRoutingBucket = "admin.resetroutingbucket";

        [English(
            "Put yourself back in the world everybody else is in. Teleporting to a player in a separate world takes you along with them, and this is the way back out again.")]
        public const string ResetRoutingBucketDescription = "admin.resetroutingbucket.desc";

        [English("Moved you out of world ~y~{bucket}~s~ and back into the default world.")]
        public const string ResetRoutingBucketDone = "admin.resetroutingbucket.done";

        [English("You are already in the default world.")]
        public const string ResetRoutingBucketAlready = "admin.resetroutingbucket.already";

        [English("See Private Messages")]
        public const string SeePrivateMessages = "admin.seeprivatemessages";

        [English(
            "Show a copy of every private message players send each other. Messages you send or receive yourself are not copied.")]
        public const string SeePrivateMessagesDescription = "admin.seeprivatemessages.desc";

        [English("~o~PM~s~ ~y~{sender}~s~ to ~y~{target}~s~: {message}")]
        public const string PrivateMessageCopy = "admin.seeprivatemessages.copy";
    }
}

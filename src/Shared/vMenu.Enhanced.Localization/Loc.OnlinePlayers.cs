namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class OnlinePlayers
    {
        [English("Online Players")]
        public const string Title = "onlineplayers.title";

        [English("See who is online and act on them.")]
        public const string LinkDescription = "onlineplayers.link.desc";

        [English("Page {page}/{pages} · {count} players")]
        public const string Subtitle = "onlineplayers.subtitle";

        [English("Page {page}/{pages} · {count} matching \"{query}\"")]
        public const string SubtitleSearch = "onlineplayers.subtitle.search";

        [English("· Outdated")]
        public const string SubtitleOutdated = "onlineplayers.subtitle.outdated";

        [English("Somebody joined or left. Close and reopen this menu for an up to date list.")]
        public const string OutdatedNotice = "onlineplayers.outdated.notice";

        [English("That was the end of the list, so you are back on page ~y~{page}~s~ of {pages}.")]
        public const string PageWrapped = "onlineplayers.page.wrapped";

        [English(
            "Server id ~y~{id}~s~. Press enter for what you can do to this player, or left and right to change the page.")]
        public const string RowDescription = "onlineplayers.row.desc";

        [English(
            "No players found",
            "Nothing matched what you searched for. Press space to search again, or leave the box empty to see everybody.")]
        public const string NoResults = "onlineplayers.noresults";

        [English("Search")]
        public const string SearchButton = "onlineplayers.search.button";

        [English("Search the player list")]
        public const string SearchBinding = "onlineplayers.search.binding";

        [English("Previous Page")]
        public const string PreviousPageButton = "onlineplayers.page.previous";

        [English("Next Page")]
        public const string NextPageButton = "onlineplayers.page.next";

        [English("Search by name, server id or identifier")]
        public const string SearchPrompt = "onlineplayers.search.prompt";

        [English("~y~{count}~s~ player(s) match \"{query}\".")]
        public const string SearchResults = "onlineplayers.search.results";

        [English("Showing everybody again.")]
        public const string SearchCleared = "onlineplayers.search.cleared";

        [English("Server id {id}")]
        public const string ActionsSubtitle = "onlineplayers.actions.subtitle";

        [English("Send Message", "Send this player a private message. ~r~Staff may be able to read private messages.")]
        public const string SendMessage = "onlineplayers.message";

        [English("Message to {player}")]
        public const string SendMessagePrompt = "onlineplayers.message.prompt";

        [English("~g~{player}~s~ received your message.")]
        public const string SendMessageDelivered = "onlineplayers.message.delivered";

        [English("Message from ~y~{player}~s~: {message}")]
        public const string MessageReceived = "onlineplayers.message.received";

        [English("Teleport To Player", "Move yourself to wherever this player is standing right now.")]
        public const string TeleportTo = "onlineplayers.teleport";

        [English("Teleported to ~g~{player}~s~.")]
        public const string TeleportToDone = "onlineplayers.teleport.done";

        [English("Summon Player", "Bring this player to where you are standing.")]
        public const string Summon = "onlineplayers.summon";

        [English("Summoned ~g~{player}~s~.")]
        public const string SummonDone = "onlineplayers.summon.done";

        [English("You have been summoned by ~y~{player}~s~.")]
        public const string SummonedBy = "onlineplayers.summon.by";

        [English("~y~{player}~s~ is already standing right next to you.")]
        public const string SummonNearby = "onlineplayers.summon.nearby";

        [English("~y~{player}~s~ is already in your vehicle.")]
        public const string SummonRiding = "onlineplayers.summon.riding";

        [English(
            "Summon Player Into Vehicle",
            "Bring this player straight into a free seat of the vehicle you are in. Nothing happens if you are not in a vehicle, or if every seat is taken.")]
        public const string SummonIntoVehicle = "onlineplayers.summonvehicle";

        [English("~g~{player}~s~ is now in your vehicle.")]
        public const string SummonIntoVehicleDone = "onlineplayers.summonvehicle.done";

        [English("You are not in a vehicle, so there is nowhere to summon ~y~{player}~s~ to.")]
        public const string SummonIntoVehicleNoVehicle = "onlineplayers.summonvehicle.novehicle";

        [English("Every seat in your vehicle is taken, so ~y~{player}~s~ was left where they are.")]
        public const string SummonIntoVehicleFull = "onlineplayers.summonvehicle.full";

        [English("You have been summoned into ~y~{player}~s~'s vehicle.")]
        public const string SummonedIntoVehicleBy = "onlineplayers.summonvehicle.by";

        [English(
            "Teleport Into Vehicle",
            "Put yourself in a free seat of whatever this player is driving or riding in. If they are on foot you are simply moved to them instead.")]
        public const string TeleportIntoVehicle = "onlineplayers.teleportvehicle";

        [English("You are now in ~g~{player}~s~'s vehicle.")]
        public const string TeleportIntoVehicleDone = "onlineplayers.teleportvehicle.done";

        [English("~y~{player}~s~ is not in a vehicle, so you were moved to them instead.")]
        public const string TeleportIntoVehicleOnFoot = "onlineplayers.teleportvehicle.onfoot";

        [English("Every seat in ~y~{player}~s~'s vehicle is taken, so you were left standing next to it.")]
        public const string TeleportIntoVehicleFull = "onlineplayers.teleportvehicle.full";

        [English(
            "Set Wanted Level",
            "Give this player a wanted level. It is asked for rather than forced, so if they have never wanted switched on it will not take, and you will be told.")]
        public const string SetWantedLevel = "onlineplayers.wanted";

        [English("None")]
        public const string SetWantedLevelNone = "onlineplayers.wanted.none";

        [English("~g~{player}~s~ is now wanted at ~y~{stars}~s~ star(s).")]
        public const string SetWantedLevelDone = "onlineplayers.wanted.done";

        [English(
            "~y~{player}~s~ would not take ~y~{stars}~s~ star(s) and is on ~y~{actual}~s~ instead. They most likely have never wanted switched on.")]
        public const string SetWantedLevelBlocked = "onlineplayers.wanted.blocked";

        [English("Kill Player", "Kill this player where they stand.")]
        public const string Kill = "onlineplayers.kill";

        [English("Killed ~y~{player}~s~.")]
        public const string KillDone = "onlineplayers.kill.done";

        [English("You have been killed by ~y~{player}~s~.")]
        public const string Killed = "onlineplayers.kill.received";

        [English("Kick Player", "Disconnect this player from the server, with a reason they will see.")]
        public const string Kick = "onlineplayers.kick";

        [English("Why is {player} being kicked?")]
        public const string KickPrompt = "onlineplayers.kick.prompt";

        [English("Kicked ~y~{player}~s~.")]
        public const string KickDone = "onlineplayers.kick.done";

        [English(
            "Delete Their Vehicle",
            "Remove the vehicle this player is driving. They have to be in the driver seat, being a passenger is not enough.")]
        public const string DeleteVehicle = "onlineplayers.deletevehicle";

        [English(
            "~r~Press again~s~ to delete the vehicle ~y~{player}~s~ is driving. Everybody riding along loses it too.")]
        public const string DeleteVehicleConfirm = "onlineplayers.deletevehicle.confirm";

        [English("Deleted the vehicle ~y~{player}~s~ was driving.")]
        public const string DeleteVehicleDone = "onlineplayers.deletevehicle.done";

        [English("~y~{player}~s~ is on foot, so there is no vehicle to delete.")]
        public const string DeleteVehicleOnFoot = "onlineplayers.deletevehicle.onfoot";

        [English("~y~{player}~s~ is only a passenger, so the vehicle is not theirs to lose.")]
        public const string DeleteVehicleNotDriving = "onlineplayers.deletevehicle.notdriving";

        [English(
            "Explode Their Vehicle",
            "Blow up the vehicle this player is driving. They have to be in the driver seat, being a passenger is not enough.")]
        public const string ExplodeVehicle = "onlineplayers.explodevehicle";

        [English(
            "~r~Press again~s~ to blow up the vehicle ~y~{player}~s~ is driving. Everybody riding along goes with it.")]
        public const string ExplodeVehicleConfirm = "onlineplayers.explodevehicle.confirm";

        [English("Blew up the vehicle ~y~{player}~s~ was driving.")]
        public const string ExplodeVehicleDone = "onlineplayers.explodevehicle.done";

        [English("~y~{player}~s~ is on foot, so there is no vehicle to blow up.")]
        public const string ExplodeVehicleOnFoot = "onlineplayers.explodevehicle.onfoot";

        [English("~y~{player}~s~ is only a passenger, so the vehicle is not theirs to lose.")]
        public const string ExplodeVehicleNotDriving = "onlineplayers.explodevehicle.notdriving";

        [English("The vehicle of ~y~{player}~s~ has ~y~vehicle god mode~s~ on, so it cannot be blown up.")]
        public const string ExplodeVehicleGodMode = "onlineplayers.explodevehicle.godmode";

        [English("Could not blow up the vehicle ~y~{player}~s~ is driving.")]
        public const string ExplodeVehicleFailed = "onlineplayers.explodevehicle.failed";

        [English("Check God Mode", "Ask this player's game whether they have player or vehicle god mode switched on.")]
        public const string CheckStatus = "onlineplayers.status";

        [English("~y~{player}~s~: player god mode {player_god}, vehicle god mode {vehicle_god}.")]
        public const string CheckStatusResult = "onlineplayers.status.result";

        [English("~r~on~s~")]
        public const string StatusOn = "onlineplayers.status.on";

        [English("~g~off~s~")]
        public const string StatusOff = "onlineplayers.status.off";

        [English("~c~unknown~s~")]
        public const string StatusUnknown = "onlineplayers.status.unknown";

        [English("Set Waypoint", "Put a waypoint on the map where this player is right now. It does not follow them.")]
        public const string Waypoint = "onlineplayers.waypoint";

        [English("Waypoint set on ~g~{player}~s~.")]
        public const string WaypointDone = "onlineplayers.waypoint.done";

        [English(
            "View Identifiers",
            "See the accounts this player is connected with, such as their licence, Steam and Discord ids.")]
        public const string Identifiers = "onlineplayers.identifiers";

        [English("Identifiers")]
        public const string IdentifiersSubtitle = "onlineplayers.identifiers.subtitle";

        [English("{identifier}\n\nPress enter to print it to the console.")]
        public const string IdentifierDescription = "onlineplayers.identifier.desc";

        [English("No identifiers", "The server did not report any identifiers for this player.")]
        public const string IdentifiersNone = "onlineplayers.identifiers.none";

        [English("Printed to the console. Open it with ~y~F8~s~ to copy it.")]
        public const string IdentifierPrinted = "onlineplayers.identifier.printed";

        [English(
            "In NoClip",
            "Put this player into noclip right now, or take them back out of it. This works whether or not they are allowed noclip themselves, and somebody you put into it can always land and step out of it on their own.")]
        public const string NoClip = "onlineplayers.noclip";

        [English("~g~{player}~s~ is now in noclip.")]
        public const string NoClipOnDone = "onlineplayers.noclip.on.done";

        [English("~y~{player}~s~ is no longer in noclip.")]
        public const string NoClipOffDone = "onlineplayers.noclip.off.done";

        [English("A staff member has put you into ~y~noclip~s~.")]
        public const string NoClipOnByStaff = "onlineplayers.noclip.on.received";

        [English("A staff member has taken you out of ~y~noclip~s~.")]
        public const string NoClipOffByStaff = "onlineplayers.noclip.off.received";

        [English("A staff member has set a ~y~waypoint~s~ on your map.")]
        public const string WaypointByStaff = "onlineplayers.waypoint.received";

        [English("You have been ~y~teleported~s~ by a staff member.")]
        public const string TeleportedByStaff = "onlineplayers.teleport.received";

        [English("A staff member has ~g~healed~s~ you.")]
        public const string HealedByStaff = "onlineplayers.heal.received";

        [English("A staff member has given you ~g~body armor~s~.")]
        public const string ArmorByStaff = "onlineplayers.armor.received";

        [English("A staff member has spawned a ~g~vehicle~s~ for you.")]
        public const string VehicleSpawnedByStaff = "onlineplayers.spawnvehicle.received";

        [English(
            "Allow NoClip",
            "Lend this player noclip, so they can switch it on and off themselves for as long as you leave this ticked. Anybody who is already allowed noclip in their own right shows as ticked and cannot be changed here.")]
        public const string NoClipAccess = "onlineplayers.noclipaccess";

        [English("~g~{player}~s~ can now use noclip.")]
        public const string NoClipAccessOnDone = "onlineplayers.noclipaccess.on.done";

        [English("~y~{player}~s~ can no longer use noclip.")]
        public const string NoClipAccessOffDone = "onlineplayers.noclipaccess.off.done";

        [English("A staff member has lent you ~y~noclip~s~. Your noclip key switches it on and off.")]
        public const string NoClipAccessLentByStaff = "onlineplayers.noclipaccess.on.received";

        [English("A staff member has taken your ~y~noclip~s~ away again.")]
        public const string NoClipAccessTakenByStaff = "onlineplayers.noclipaccess.off.received";

        [English(
            "Refresh Permissions",
            "Hand this player a fresh copy of what they are allowed to do. Use it once you have actually changed their permissions on the server, so their menu catches up without them having to rejoin.")]
        public const string RefreshPermissions = "onlineplayers.refreshpermissions";

        [English("Refreshed permissions for ~g~{player}~s~.")]
        public const string RefreshPermissionsDone = "onlineplayers.refreshpermissions.done";

        [English(
            "Open In txAdmin",
            "Opens this player's page in the txAdmin panel.\nNote, if you get an error saying player does not exist, that's a txAdmin bug which will hopefully be fixed soon.")]
        public const string TxAdmin = "onlineplayers.txadmin";

        [English("You are not allowed to do that.")]
        public const string Denied = "onlineplayers.denied";

        [English("~y~{player}~s~ is no longer on the server, or still joining.")]
        public const string NotFound = "onlineplayers.notfound";

        [English("You cannot do that to yourself.")]
        public const string NotYourself = "onlineplayers.notyourself";

        [English("~y~{player}~s~ is still joining the server, so that has to wait a moment.")]
        public const string StillJoining = "onlineplayers.stilljoining";

        [English("You are doing that too quickly. Try again in ~y~{seconds}~s~ second(s).")]
        public const string TooManyActions = "onlineplayers.toomanyactions";

        [English("That did not work. Try again in a moment.")]
        public const string Failed = "onlineplayers.failed";
    }
}

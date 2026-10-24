namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class PropSpawner
    {
        [English("Prop Spawner")]
        public const string Title = "propspawner.title";

        [English("Prop Spawner")]
        public const string Subtitle = "propspawner.subtitle";

        [English("Put objects into the world and push them around until they sit where you want them.")]
        public const string LinkDescription = "propspawner.link.desc";

        [English(
            "Spawn Prop By Name",
            "Type the model name of a prop. It appears in front of you for you to position.")]
        public const string SpawnByName = "propspawner.spawnbyname";

        [English("Prop model name")]
        public const string SpawnByNamePrompt = "propspawner.spawnbyname.prompt";

        [English("That is not a prop this server has.")]
        public const string InvalidModel = "propspawner.invalidmodel";

        [English("The prop could not be spawned.")]
        public const string SpawnFailed = "propspawner.spawnfailed";

        [English("You are already placing something. Finish that first.")]
        public const string AlreadyPlacing = "propspawner.alreadyplacing";

        [English("You have {max} props out already. Delete some before spawning more.")]
        public const string TooMany = "propspawner.toomany";

        [English("Prop placed.")]
        public const string Placed = "propspawner.placed";

        [English("Prop discarded.")]
        public const string Cancelled = "propspawner.cancelled";

        [English("Recently Spawned", "The props you have spawned before, newest first. Kept between sessions.")]
        public const string Recents = "propspawner.recents";

        [English("Recently Spawned")]
        public const string RecentsSubtitle = "propspawner.recents.subtitle";

        [English("Nothing yet", "Spawn a prop by name and it will show up here.")]
        public const string RecentsEmpty = "propspawner.recents.empty";

        [English("Spawn this prop again.")]
        public const string RecentRowDescription = "propspawner.recents.row.desc";

        [English(
            "Networked",
            "Everybody on the server sees the prop. Turn this off and it only exists on your own machine.")]
        public const string Networked = "propspawner.networked";

        [English("You are not allowed to spawn props other people can see, so every prop you spawn is yours alone.")]
        public const string NetworkedLocked = "propspawner.networked.locked";

        [English(
            "Frozen In Place",
            "The prop stays exactly where you put it. Turn this off and physics takes over the moment you let go, so it can fall or roll.")]
        public const string Frozen = "propspawner.frozen";

        [English(
            "Snap To Ground",
            "Drops the prop onto whatever is under it when you place it, instead of leaving it floating.")]
        public const string SnapToGround = "propspawner.snapground";

        [English("Placement Distance")]
        public const string Distance = "propspawner.distance";

        [English(
            "How far in front of you the prop floats while you position it. Currently {metres} metres, and the scroll wheel changes it while placing.")]
        public const string DistanceDescription = "propspawner.distance.desc";

        [English("Delete All My Props", "Removes every prop you have spawned this session.")]
        public const string DeleteAll = "propspawner.deleteall";

        [English("~r~Are you sure?~s~ Press again to delete all of them.")]
        public const string DeleteAllConfirm = "propspawner.deleteall.confirm";

        [English("Removed {count} prop(s).")]
        public const string Deleted = "propspawner.deleted";

        [English("You have no props out.")]
        public const string NothingToDelete = "propspawner.nothingtodelete";

        [English("Place")]
        public const string ButtonPlace = "propspawner.button.place";

        [English("Cancel")]
        public const string ButtonCancel = "propspawner.button.cancel";

        [English("Rotate")]
        public const string ButtonRotate = "propspawner.button.rotate";

        [English("Nearer / Further")]
        public const string ButtonDistance = "propspawner.button.distance";

        [English("Snap To Ground: On")]
        public const string ButtonSnapOn = "propspawner.button.snap.on";

        [English("Snap To Ground: Off")]
        public const string ButtonSnapOff = "propspawner.button.snap.off";

        [English("Saved Prop Sets")]
        public const string Sets = "propspawner.sets";

        [English("Groups of props saved together, so you can put a whole arrangement back at once.")]
        public const string SetsDescription = "propspawner.sets.desc";

        [English("Saved Prop Sets")]
        public const string SetsSubtitle = "propspawner.sets.subtitle";

        [English("Create Set", "Start a new, empty set. Add props to it by placing them.")]
        public const string SetsCreate = "propspawner.sets.create";

        [English("Name for the set")]
        public const string SetsName = "propspawner.sets.name";

        [English("Description, or leave it empty")]
        public const string SetsDescriptionPrompt = "propspawner.sets.descriptionprompt";

        [English("You already have a set with that name.")]
        public const string SetsNameTaken = "propspawner.sets.nametaken";

        [English("Set created.")]
        public const string SetsSaved = "propspawner.sets.saved";

        [English("The set could not be saved.")]
        public const string SetsSaveFailed = "propspawner.sets.savefailed";

        [English("No sets yet", "Create one, then add props to it by placing them.")]
        public const string SetsEmpty = "propspawner.sets.empty";

        [English("Holds {count} prop(s). Open it to spawn, add or remove.")]
        public const string SetsRowDescription = "propspawner.sets.row.desc";

        [English("Spawn This Set", "Puts every prop in this set back exactly where it was saved.")]
        public const string SetsSpawn = "propspawner.sets.spawn";

        [English("Spawned {count} prop(s).")]
        public const string SetsSpawned = "propspawner.sets.spawned";

        [English(
            "Add A Prop To This Set",
            "Type a model name, place it where you want it, and it is saved into this set.")]
        public const string SetsAddProp = "propspawner.sets.addprop";

        [English("Prop added to {name}.")]
        public const string SetsPropAdded = "propspawner.sets.propadded";

        [English("Remove A Prop", "Pick one of the props in this set and take it out.")]
        public const string SetsRemoveList = "propspawner.sets.removelist";

        [English("Remove A Prop")]
        public const string SetsRemoveProp = "propspawner.sets.removeprop";

        [English("Standing at {where}. Take it out of this set.")]
        public const string SetsRemovePropDescription = "propspawner.sets.removeprop.desc";

        [English("~r~Are you sure?~s~ Press again to take it out of the set.")]
        public const string SetsRemovePropConfirm = "propspawner.sets.removeprop.confirm";

        [English("Prop taken out of the set.")]
        public const string SetsPropRemoved = "propspawner.sets.propremoved";

        [English("Nothing in this set", "Add a prop to it first.")]
        public const string SetsNoProps = "propspawner.sets.noprops";

        [English("Rename Set", "Change the name or the description of this set.")]
        public const string SetsRename = "propspawner.sets.rename";

        [English("Set renamed.")]
        public const string SetsRenamed = "propspawner.sets.renamed";

        [English("Delete Set", "Throws the whole set away. The props already in the world stay where they are.")]
        public const string SetsDelete = "propspawner.sets.delete";

        [English("~r~Are you sure?~s~ Press again to delete the set.")]
        public const string SetsDeleteConfirm = "propspawner.sets.delete.confirm";

        [English("Set deleted.")]
        public const string SetsDeleted = "propspawner.sets.deleted";

        [English("That set was saved by a newer version of vMenu, so this one will not change it.")]
        public const string SetsNewerBuild = "propspawner.sets.newerbuild";

        [English(
            "Remove This Set From The World",
            "Takes back down the props this set last put up. The set itself is kept.")]
        public const string SetsUnspawn = "propspawner.sets.unspawn";

        [English("~r~Are you sure?~s~ Press again to take them all back down.")]
        public const string SetsUnspawnConfirm = "propspawner.sets.unspawn.confirm";

        [English("Took {count} prop(s) back down.")]
        public const string SetsUnspawned = "propspawner.sets.unspawned";

        [English("Nothing from this set is standing.")]
        public const string SetsNothingStanding = "propspawner.sets.nothingstanding";

        [English(
            "Nearby Props",
            "Props around you, nearest first. The one you are looking at is outlined in the world so you can be sure which it is.")]
        public const string Nearby = "propspawner.nearby";

        [English("Nearby Props")]
        public const string NearbySubtitle = "propspawner.nearby.subtitle";

        [English("Nothing nearby", "There are no props near you that you are allowed to remove.")]
        public const string NearbyEmpty = "propspawner.nearby.empty";

        [English("Yours, {distance}m away. Outlined in the world.")]
        public const string NearbyRowMine = "propspawner.nearby.row.mine";

        [English("Somebody else's, {distance}m away. Outlined in the world.")]
        public const string NearbyRowOther = "propspawner.nearby.row.other";

        [English("~r~Are you sure?~s~ Press again to remove this prop.")]
        public const string NearbyRemoveConfirm = "propspawner.nearby.remove.confirm";

        [English("Prop removed.")]
        public const string NearbyRemoved = "propspawner.nearby.removed";

        [English("You are only allowed to remove props you spawned yourself.")]
        public const string NearbyDenied = "propspawner.nearby.denied";
    }
}

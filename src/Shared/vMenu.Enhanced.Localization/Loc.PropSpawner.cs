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

        [English("Spawn Prop By Name")]
        public const string SpawnByName = "propspawner.spawnbyname";

        [English("Type the model name of a prop. It appears in front of you for you to position.")]
        public const string SpawnByNameDescription = "propspawner.spawnbyname.desc";

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

        [English("Recently Spawned")]
        public const string Recents = "propspawner.recents";

        [English("The props you have spawned before, newest first. Kept between sessions.")]
        public const string RecentsDescription = "propspawner.recents.desc";

        [English("Recently Spawned")]
        public const string RecentsSubtitle = "propspawner.recents.subtitle";

        [English("Nothing yet")]
        public const string RecentsEmpty = "propspawner.recents.empty";

        [English("Spawn a prop by name and it will show up here.")]
        public const string RecentsEmptyDescription = "propspawner.recents.empty.desc";

        [English("Spawn this prop again.")]
        public const string RecentRowDescription = "propspawner.recents.row.desc";

        [English("Networked")]
        public const string Networked = "propspawner.networked";

        [English("Everybody on the server sees the prop. Turn this off and it only exists on your own machine.")]
        public const string NetworkedDescription = "propspawner.networked.desc";

        [English("You are not allowed to spawn props other people can see, so every prop you spawn is yours alone.")]
        public const string NetworkedLocked = "propspawner.networked.locked";

        [English("Frozen In Place")]
        public const string Frozen = "propspawner.frozen";

        [English(
            "The prop stays exactly where you put it. Turn this off and physics takes over the moment you let go, so it can fall or roll.")]
        public const string FrozenDescription = "propspawner.frozen.desc";

        [English("Snap To Ground")]
        public const string SnapToGround = "propspawner.snapground";

        [English("Drops the prop onto whatever is under it when you place it, instead of leaving it floating.")]
        public const string SnapToGroundDescription = "propspawner.snapground.desc";

        [English("Placement Distance")]
        public const string Distance = "propspawner.distance";

        [English(
            "How far in front of you the prop floats while you position it. Currently {metres} metres, and the scroll wheel changes it while placing.")]
        public const string DistanceDescription = "propspawner.distance.desc";

        [English("Delete All My Props")]
        public const string DeleteAll = "propspawner.deleteall";

        [English("Removes every prop you have spawned this session.")]
        public const string DeleteAllDescription = "propspawner.deleteall.desc";

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

        [English("Create Set")]
        public const string SetsCreate = "propspawner.sets.create";

        [English("Start a new, empty set. Add props to it by placing them.")]
        public const string SetsCreateDescription = "propspawner.sets.create.desc";

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

        [English("No sets yet")]
        public const string SetsEmpty = "propspawner.sets.empty";

        [English("Create one, then add props to it by placing them.")]
        public const string SetsEmptyDescription = "propspawner.sets.empty.desc";

        [English("Holds {count} prop(s). Open it to spawn, add or remove.")]
        public const string SetsRowDescription = "propspawner.sets.row.desc";

        [English("Spawn This Set")]
        public const string SetsSpawn = "propspawner.sets.spawn";

        [English("Puts every prop in this set back exactly where it was saved.")]
        public const string SetsSpawnDescription = "propspawner.sets.spawn.desc";

        [English("Spawned {count} prop(s).")]
        public const string SetsSpawned = "propspawner.sets.spawned";

        [English("Add A Prop To This Set")]
        public const string SetsAddProp = "propspawner.sets.addprop";

        [English("Type a model name, place it where you want it, and it is saved into this set.")]
        public const string SetsAddPropDescription = "propspawner.sets.addprop.desc";

        [English("Prop added to {name}.")]
        public const string SetsPropAdded = "propspawner.sets.propadded";

        [English("Remove A Prop")]
        public const string SetsRemoveList = "propspawner.sets.removelist";

        [English("Pick one of the props in this set and take it out.")]
        public const string SetsRemoveListDescription = "propspawner.sets.removelist.desc";

        [English("Remove A Prop")]
        public const string SetsRemoveProp = "propspawner.sets.removeprop";

        [English("Standing at {where}. Take it out of this set.")]
        public const string SetsRemovePropDescription = "propspawner.sets.removeprop.desc";

        [English("~r~Are you sure?~s~ Press again to take it out of the set.")]
        public const string SetsRemovePropConfirm = "propspawner.sets.removeprop.confirm";

        [English("Prop taken out of the set.")]
        public const string SetsPropRemoved = "propspawner.sets.propremoved";

        [English("Nothing in this set")]
        public const string SetsNoProps = "propspawner.sets.noprops";

        [English("Add a prop to it first.")]
        public const string SetsNoPropsDescription = "propspawner.sets.noprops.desc";

        [English("Rename Set")]
        public const string SetsRename = "propspawner.sets.rename";

        [English("Change the name or the description of this set.")]
        public const string SetsRenameDescription = "propspawner.sets.rename.desc";

        [English("Set renamed.")]
        public const string SetsRenamed = "propspawner.sets.renamed";

        [English("Delete Set")]
        public const string SetsDelete = "propspawner.sets.delete";

        [English("Throws the whole set away. The props already in the world stay where they are.")]
        public const string SetsDeleteDescription = "propspawner.sets.delete.desc";

        [English("~r~Are you sure?~s~ Press again to delete the set.")]
        public const string SetsDeleteConfirm = "propspawner.sets.delete.confirm";

        [English("Set deleted.")]
        public const string SetsDeleted = "propspawner.sets.deleted";

        [English("That set was saved by a newer version of vMenu, so this one will not change it.")]
        public const string SetsNewerBuild = "propspawner.sets.newerbuild";

        [English("Remove This Set From The World")]
        public const string SetsUnspawn = "propspawner.sets.unspawn";

        [English("Takes back down the props this set last put up. The set itself is kept.")]
        public const string SetsUnspawnDescription = "propspawner.sets.unspawn.desc";

        [English("~r~Are you sure?~s~ Press again to take them all back down.")]
        public const string SetsUnspawnConfirm = "propspawner.sets.unspawn.confirm";

        [English("Took {count} prop(s) back down.")]
        public const string SetsUnspawned = "propspawner.sets.unspawned";

        [English("Nothing from this set is standing.")]
        public const string SetsNothingStanding = "propspawner.sets.nothingstanding";

        [English("Nearby Props")]
        public const string Nearby = "propspawner.nearby";

        [English(
            "Props around you, nearest first. The one you are looking at is outlined in the world so you can be sure which it is.")]
        public const string NearbyDescription = "propspawner.nearby.desc";

        [English("Nearby Props")]
        public const string NearbySubtitle = "propspawner.nearby.subtitle";

        [English("Nothing nearby")]
        public const string NearbyEmpty = "propspawner.nearby.empty";

        [English("There are no props near you that you are allowed to remove.")]
        public const string NearbyEmptyDescription = "propspawner.nearby.empty.desc";

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

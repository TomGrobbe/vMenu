namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class TeleportMenu
    {
        [English("Teleportation Menu")]
        public const string Title = "teleportmenu.title";

        [English("Teleportation Menu")]
        public const string Subtitle = "teleportmenu.link.subtitle";

        [English("Teleport around the map.")]
        public const string LinkDescription = "teleportmenu.link.desc";

        [English("Teleport To Waypoint")]
        public const string TeleportToWaypoint = "teleportmenu.tptowp";

        [English("Teleports you to your set waypoint on the map.")]
        public const string TeleportToWaypointDescription = "teleportmenu.tptowp.desc";

        [English("No waypoint currently set.")]
        public const string NoWaypoint = "teleportmenu.tptowp.nowp";

        [English("Waypoint is invalid.")]
        public const string WaypointInvalid = "teleportmenu.tptowp.invalid";

        [English("Teleport To Coords")]
        public const string TeleportToCoords = "teleportmenu.tptocoords";

        [English("Teleports you to the coords you type in.")]
        public const string TeleportToCoordsDescription = "teleportmenu.tptocoords.desc";

        [English("Type in coords to teleport to (X, Y, Z)")]
        public const string CoordsPrompt = "teleportmenu.tptocoords.prompt";

        [English("Type three numbers separated by a comma or a space.")]
        public const string CoordsInvalid = "teleportmenu.tptocoords.invalid";

        [English("Could not work out how high the ground is there.")]
        public const string GroundNotFound = "teleportmenu.groundnotfound";

        [English("Saved, everybody on the server can use it now.")]
        public const string Saved = "teleportmenu.saved";

        [English("You are not allowed to add teleport locations.")]
        public const string SaveDenied = "teleportmenu.save.denied";

        [English("The server could not save that.")]
        public const string SaveFailed = "teleportmenu.save.failed";

        [English("Something with that name already exists.")]
        public const string NameTaken = "teleportmenu.save.nametaken";

        [English("That category does not exist on the server anymore.")]
        public const string CategoryGone = "teleportmenu.save.categorygone";

        [English("Teleport Categories")]
        public const string TeleportCategories = "teleportmenu.categories";

        [English("Teleport to a location this server has set up.")]
        public const string TeleportCategoriesDescription = "teleportmenu.categories.desc";

        [English("No Categories")]
        public const string NoCategories = "teleportmenu.categories.none";

        [English("This server has no teleport categories set up.")]
        public const string NoCategoriesDescription = "teleportmenu.categories.none.desc";

        [English("No Locations")]
        public const string NoLocations = "teleportmenu.categories.nolocations";

        [English("This category has no locations in it.")]
        public const string NoLocationsDescription = "teleportmenu.categories.nolocations.desc";

        [English("Create Category")]
        public const string CreateCategory = "teleportmenu.categories.create";

        [English("Creates a teleportation category.")]
        public const string CreateCategoryDescription = "teleportmenu.categories.create.desc";

        [English("Create Position")]
        public const string CreatePosition = "teleportmenu.categories.position";

        [English("Creates a teleportation position inside of the current category.")]
        public const string CreatePositionDescription = "teleportmenu.categories.position.desc";

        [English("Delete Category")]
        public const string DeleteCategory = "teleportmenu.categories.delete";

        [English("Pick a category and press enter to delete it, for everybody on the server.")]
        public const string DeleteCategoryDescription = "teleportmenu.categories.delete.desc";

        [English("~r~Delete {name} and every position in it? Press enter again to confirm.")]
        public const string DeleteCategoryConfirm = "teleportmenu.categories.delete.confirm";

        [English("Delete Position")]
        public const string DeletePosition = "teleportmenu.categories.position.delete";

        [English("Pick a position and press enter to delete it, for everybody on the server.")]
        public const string DeletePositionDescription = "teleportmenu.categories.position.delete.desc";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string DeletePositionConfirm = "teleportmenu.categories.position.delete.confirm";

        [English("Deleted, it is gone for everybody on the server.")]
        public const string Deleted = "teleportmenu.deleted";

        [English("You are not allowed to delete teleport locations.")]
        public const string DeleteDenied = "teleportmenu.delete.denied";

        [English("That is not on the server anymore.")]
        public const string DeleteGone = "teleportmenu.delete.gone";

        [English("The server could not delete that.")]
        public const string DeleteFailed = "teleportmenu.delete.failed";

        [English("Name of the position you are creating")]
        public const string PositionName = "teleportmenu.categories.add.position.name";

        [English("Description of the position you are creating.")]
        public const string PositionDescription = "teleportmenu.categories.add.position.desc";

        [English("Name of the category you are creating")]
        public const string CategoryName = "teleportmenu.categories.add.category.name";

        [English("Description of the category you are creating.")]
        public const string CategoryDescription = "teleportmenu.categories.add.category.desc";

        [English("Teleport Key")]
        public const string KeyAction = "teleportmenu.key.action";

        [English("What your teleport key does. Rebind the key itself under Settings, Key Bindings, vMenu: Teleport.")]
        public const string KeyActionDescription = "teleportmenu.key.action.desc";

        [English("Disabled")]
        public const string KeyActionDisabled = "teleportmenu.key.action.disabled";

        [English("To Waypoint")]
        public const string KeyActionWaypoint = "teleportmenu.key.action.waypoint";

        [English("To Coords")]
        public const string KeyActionCoords = "teleportmenu.key.action.coords";

        [English("You are not allowed to use the teleport your key is set to.")]
        public const string KeyActionDenied = "teleportmenu.key.action.denied";
    }
}

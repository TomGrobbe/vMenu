namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class SavedVehicles
    {
        [English("Saved Vehicles")]
        public const string Title = "savedvehicles.title";

        [English("Saved Vehicles Menu")]
        public const string Subtitle = "savedvehicles.subtitle";

        [English("Save the vehicle you are driving, and bring your saved ones back.")]
        public const string LinkDescription = "savedvehicles.link.desc";

        #region Saving

        [English(
            "Save Current Vehicle",
            "Stores the vehicle you are driving, with everything you have changed on it, on this computer.")]
        public const string SaveCurrent = "savedvehicles.save";

        [English("Give this vehicle a name")]
        public const string NamePrompt = "savedvehicles.save.name";

        [English("Anything you want to remember about it? Leave this empty for nothing")]
        public const string DescriptionPrompt = "savedvehicles.save.description";

        [English("Which category should it go in? Leave this empty for none")]
        public const string CategoryPrompt = "savedvehicles.save.category";

        [English("You have to be ~y~in a vehicle~s~ to save it.")]
        public const string NoVehicle = "savedvehicles.save.novehicle";

        [English("You have to be the ~y~driver~s~ to save this vehicle.")]
        public const string NotDriver = "savedvehicles.save.notdriver";

        [English("Saved as ~g~{name}~s~.")]
        public const string Saved = "savedvehicles.save.success";

        [English("~r~Could not save~s~ that vehicle.")]
        public const string SaveFailed = "savedvehicles.save.failed";

        [English("You already have a saved vehicle called ~y~{name}~s~.")]
        public const string NameTaken = "savedvehicles.save.nametaken";

        #endregion

        #region Browsing

        [English("Uncategorised")]
        public const string Uncategorised = "savedvehicles.uncategorised";

        [English("The ~y~{count}~s~ vehicle(s) you saved into this category.")]
        public const string CategoryRowDescription = "savedvehicles.category.row.desc";

        [English("A ~y~{model}~s~. Press enter for what you can do with it.")]
        public const string VehicleRowDescription = "savedvehicles.vehicle.row.desc";

        [English(
            "Nothing Saved Yet",
            "Get into a vehicle you like and use Save Current Vehicle, and it will show up here.")]
        public const string NoVehicles = "savedvehicles.empty";

        [English(
            "This server does not have the ~r~{model}~s~ model, so this one cannot be spawned here. You can still edit or delete it.")]
        public const string ModelUnavailable = "savedvehicles.model.unavailable";

        #endregion

        #region Categories

        [English("Create Category", "Make a new group to sort your saved vehicles into.")]
        public const string CreateCategory = "savedvehicles.categories.create";

        [English("Category name")]
        public const string CategoryName = "savedvehicles.categories.name";

        [English("A short description of this category")]
        public const string CategoryDescriptionPrompt = "savedvehicles.categories.description";

        [English(
            "Edit Category",
            "Pick a category and press enter to give it a different name or description. The vehicles in it come along.")]
        public const string EditCategory = "savedvehicles.categories.edit";

        [English("That category is now ~g~{name}~s~.")]
        public const string CategoryEdited = "savedvehicles.categories.edited";

        [English("~r~Delete Category", "Removes a category. The vehicles in it are kept and move to Uncategorised.")]
        public const string DeleteCategory = "savedvehicles.categories.delete";

        [English("~r~Delete {name}? Its vehicles move to Uncategorised. Press enter again to confirm.")]
        public const string DeleteCategoryConfirm = "savedvehicles.categories.delete.confirm";

        [English("Created the ~g~{name}~s~ category.")]
        public const string CategoryCreated = "savedvehicles.categories.created";

        [English("~g~Category deleted~s~.")]
        public const string CategoryDeleted = "savedvehicles.categories.deleted";

        [English("You already have a category called ~y~{name}~s~.")]
        public const string CategoryNameTaken = "savedvehicles.categories.nametaken";

        #endregion

        #region Acting on one vehicle

        [English(
            "Spawn Vehicle",
            "Brings this vehicle back exactly as you saved it, and puts you in the driver's seat.")]
        public const string Spawn = "savedvehicles.spawn";

        [English("The ~r~{model}~s~ model is not on this server, so this vehicle cannot be spawned here.")]
        public const string SpawnModelMissing = "savedvehicles.spawn.missing";

        [English("You do not have permission to spawn ~y~{model}~s~ on this server.")]
        public const string SpawnDenied = "savedvehicles.spawn.denied";

        [English("Spawned ~g~{name}~s~.")]
        public const string RestoredExactly = "savedvehicles.spawn.exact";

        [English(
            "Spawned ~y~{name}~s~, but ~y~{count}~s~ setting(s) could not be applied. Run command ~y~vmenu_vehicle_diff {name}~s~ in F8 to see which.")]
        public const string RestoredPartially = "savedvehicles.spawn.partial";

        [English("Edit", "Give this saved vehicle a different name or description.")]
        public const string Edit = "savedvehicles.edit";

        [English("Saved as ~g~{name}~s~.")]
        public const string Edited = "savedvehicles.edit.success";

        [English("Duplicate", "Saves a second copy of this vehicle under another name, leaving this one alone.")]
        public const string Duplicate = "savedvehicles.duplicate";

        [English("Name for the copy of {name}")]
        public const string DuplicatePrompt = "savedvehicles.duplicate.prompt";

        [English("Copied to ~g~{name}~s~.")]
        public const string Duplicated = "savedvehicles.duplicate.success";

        [English(
            "Move To Category",
            "Put this vehicle into a different category. Press enter to apply the one you picked.")]
        public const string MoveToCategory = "savedvehicles.move";

        [English("Moved to ~g~{name}~s~.")]
        public const string Moved = "savedvehicles.move.success";

        [English(
            "~r~Replace With Current Vehicle",
            "Overwrites this save with the vehicle you are driving right now, keeping the name and the category.")]
        public const string Replace = "savedvehicles.replace";

        [English("~r~Overwrite {name} with the vehicle you are driving? Press enter again to confirm.")]
        public const string ReplaceConfirm = "savedvehicles.replace.confirm";

        [English("~g~{name}~s~ now matches the vehicle you are driving.")]
        public const string Replaced = "savedvehicles.replace.success";

        [English("~r~Delete", "Forgets this saved vehicle for good.")]
        public const string Delete = "savedvehicles.delete";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string DeleteConfirm = "savedvehicles.delete.confirm";

        [English("~g~Saved vehicle deleted~s~.")]
        public const string Deleted = "savedvehicles.delete.success";

        #endregion

        #region Written by a newer build

        [English("~y~Newer")]
        public const string NewerBuildLabel = "savedvehicles.newer.label";

        [English(
            "This vehicle was saved by a newer version of vMenu than the one this server runs, so some of it cannot be read here. You can still spawn, duplicate and delete it, but editing and overwriting are not offered, because either would throw away the parts this version does not understand.")]
        public const string NewerBuildDescription = "savedvehicles.newer.desc";

        [English(
            "Save As A New Vehicle",
            "Saves the vehicle you are driving under a new name, leaving the newer save untouched.")]
        public const string SaveAsNew = "savedvehicles.newer.saveasnew";

        [English(
            "~y~{name}~s~ was saved by a newer version of vMenu, so it was left alone. Save what you are driving under a new name instead.")]
        public const string OverwriteRefused = "savedvehicles.newer.refused";

        #endregion
    }
}

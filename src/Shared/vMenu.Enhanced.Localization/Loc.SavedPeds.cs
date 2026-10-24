namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class SavedPeds
    {
        [English("Saved Peds")]
        public const string Title = "savedpeds.title";

        [English("Saved Peds Menu")]
        public const string Subtitle = "savedpeds.subtitle";

        [English("Put away the ped you are wearing, and wear your saved ones again later.")]
        public const string LinkDescription = "savedpeds.link.desc";

        #region Saving

        [English(
            "Save Current Ped",
            "Stores the ped you are wearing, with everything you have dressed it in, on this computer.")]
        public const string SaveCurrent = "savedpeds.save";

        [English("Give this ped a name")]
        public const string NamePrompt = "savedpeds.save.name";

        [English("Anything you want to remember about it? Leave this empty for nothing")]
        public const string DescriptionPrompt = "savedpeds.save.description";

        [English("Which category should it go in? Leave this empty for none")]
        public const string CategoryPrompt = "savedpeds.save.category";

        [English("Saved as ~g~{name}~s~.")]
        public const string Saved = "savedpeds.save.success";

        [English("~r~Could not save~s~ that ped.")]
        public const string SaveFailed = "savedpeds.save.failed";

        [English("You already have a saved ped called ~y~{name}~s~.")]
        public const string NameTaken = "savedpeds.save.nametaken";

        [English(
            "This is a ~y~freemode ped~s~, the kind the online character creator builds. Saving it here would keep the clothes and lose the face, hair and tattoos, so use ~b~Custom Character Creator~s~ for this one.")]
        public const string FreemodeRefused = "savedpeds.save.freemode";

        #endregion

        #region Browsing

        [English("Uncategorised")]
        public const string Uncategorised = "savedpeds.uncategorised";

        [English("The ~y~{count}~s~ ped(s) you saved into this category.")]
        public const string CategoryRowDescription = "savedpeds.category.row.desc";

        [English("A ~y~{model}~s~. Press enter for what you can do with it.")]
        public const string PedRowDescription = "savedpeds.ped.row.desc";

        [English(
            "Nothing Saved Yet",
            "Dress a ped how you like it and use Save Current Ped, and it will show up here.")]
        public const string NoPeds = "savedpeds.empty";

        [English(
            "This server does not have the ~r~{model}~s~ model, so this one cannot be worn here. You can still edit or delete it.")]
        public const string ModelUnavailable = "savedpeds.model.unavailable";

        [English("unlisted ped")]
        public const string UnnamedModel = "savedpeds.model.unnamed";

        #endregion

        #region Categories

        [English("Create Category", "Make a new group to sort your saved peds into.")]
        public const string CreateCategory = "savedpeds.categories.create";

        [English("Category name")]
        public const string CategoryName = "savedpeds.categories.name";

        [English("A short description of this category")]
        public const string CategoryDescriptionPrompt = "savedpeds.categories.description";

        [English(
            "Edit Category",
            "Pick a category and press enter to give it a different name or description. The peds in it come along.")]
        public const string EditCategory = "savedpeds.categories.edit";

        [English("That category is now ~g~{name}~s~.")]
        public const string CategoryEdited = "savedpeds.categories.edited";

        [English("~r~Delete Category", "Removes a category. The peds in it are kept and move to Uncategorised.")]
        public const string DeleteCategory = "savedpeds.categories.delete";

        [English("~r~Delete {name}? Its peds move to Uncategorised. Press enter again to confirm.")]
        public const string DeleteCategoryConfirm = "savedpeds.categories.delete.confirm";

        [English("Created the ~g~{name}~s~ category.")]
        public const string CategoryCreated = "savedpeds.categories.created";

        [English("~g~Category deleted~s~.")]
        public const string CategoryDeleted = "savedpeds.categories.deleted";

        [English("You already have a category called ~y~{name}~s~.")]
        public const string CategoryNameTaken = "savedpeds.categories.nametaken";

        #endregion

        #region Acting on one ped

        [English("Spawn Ped", "Turns you into this ped, dressed exactly as you saved it.")]
        public const string Spawn = "savedpeds.spawn";

        [English("The ~r~{model}~s~ model is not on this server, so this ped cannot be worn here.")]
        public const string SpawnModelMissing = "savedpeds.spawn.missing";

        [English("You do not have permission to turn into ~y~{model}~s~ on this server.")]
        public const string SpawnDenied = "savedpeds.spawn.denied";

        [English("You are now ~g~{name}~s~.")]
        public const string RestoredExactly = "savedpeds.spawn.exact";

        [English(
            "You are now ~y~{name}~s~, but ~y~{count}~s~ piece(s) could not be put on. Run command ~y~vmenu_ped_diff {name}~s~ in F8 to see which.")]
        public const string RestoredPartially = "savedpeds.spawn.partial";

        [English("Edit", "Give this saved ped a different name or description.")]
        public const string Edit = "savedpeds.edit";

        [English("Saved as ~g~{name}~s~.")]
        public const string Edited = "savedpeds.edit.success";

        [English("Duplicate", "Saves a second copy of this ped under another name, leaving this one alone.")]
        public const string Duplicate = "savedpeds.duplicate";

        [English("Name for the copy of {name}")]
        public const string DuplicatePrompt = "savedpeds.duplicate.prompt";

        [English("Copied to ~g~{name}~s~.")]
        public const string Duplicated = "savedpeds.duplicate.success";

        [English(
            "Move To Category",
            "Put this ped into a different category. Press enter to apply the one you picked.")]
        public const string MoveToCategory = "savedpeds.move";

        [English("Moved to ~g~{name}~s~.")]
        public const string Moved = "savedpeds.move.success";

        [English(
            "~r~Replace With Current Ped",
            "Overwrites this save with the ped you are wearing right now, keeping the name and the category.")]
        public const string Replace = "savedpeds.replace";

        [English("~r~Overwrite {name} with the ped you are wearing? Press enter again to confirm.")]
        public const string ReplaceConfirm = "savedpeds.replace.confirm";

        [English("~g~{name}~s~ now matches the ped you are wearing.")]
        public const string Replaced = "savedpeds.replace.success";

        [English("~r~Delete", "Forgets this saved ped for good.")]
        public const string Delete = "savedpeds.delete";

        [English("~r~Delete {name}? Press enter again to confirm.")]
        public const string DeleteConfirm = "savedpeds.delete.confirm";

        [English("~g~Saved ped deleted~s~.")]
        public const string Deleted = "savedpeds.delete.success";

        #endregion

        #region Written by a newer build

        [English("~y~Newer")]
        public const string NewerBuildLabel = "savedpeds.newer.label";

        [English(
            "This ped was saved by a newer version of vMenu than the one this server runs, so some of it cannot be read here. You can still wear, duplicate and delete it, but editing and overwriting are not offered, because either would throw away the parts this version does not understand.")]
        public const string NewerBuildDescription = "savedpeds.newer.desc";

        [English(
            "Save As A New Ped",
            "Saves the ped you are wearing under a new name, leaving the newer save untouched.")]
        public const string SaveAsNew = "savedpeds.newer.saveasnew";

        [English(
            "~y~{name}~s~ was saved by a newer version of vMenu, so it was left alone. Save what you are wearing under a new name instead.")]
        public const string OverwriteRefused = "savedpeds.newer.refused";

        #endregion
    }
}

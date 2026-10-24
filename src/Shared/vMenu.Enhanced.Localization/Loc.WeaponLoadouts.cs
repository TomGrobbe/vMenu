namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class WeaponLoadouts
    {
        [English("Weapon Loadouts")]
        public const string Title = "weaponloadouts.title";

        [English("Weapon Loadouts Menu")]
        public const string Subtitle = "weaponloadouts.subtitle";

        [English("Save the weapons you are carrying, and get them back later.")]
        public const string LinkDescription = "weaponloadouts.link.desc";

        [English("Save Loadout", "Save the weapons you are carrying right now into a new loadout.")]
        public const string Save = "weaponloadouts.save";

        [English("Enter a name for this loadout")]
        public const string SavePrompt = "weaponloadouts.save.prompt";

        [English("Your weapons were saved as ~g~{name}~s~.")]
        public const string Saved = "weaponloadouts.saved";

        [English("~r~That loadout could not be saved.")]
        public const string SaveFailed = "weaponloadouts.save.failed";

        [English("~r~You already have a loadout called {name}.")]
        public const string SaveNameTaken = "weaponloadouts.save.nametaken";

        [English("~r~You are not carrying any weapons to save.")]
        public const string SaveNothingHeld = "weaponloadouts.save.nothingheld";

        [English("Manage Loadouts", "Look through the loadouts you have saved.")]
        public const string Manage = "weaponloadouts.manage";

        [English("Saved Loadouts")]
        public const string ManageSubtitle = "weaponloadouts.manage.subtitle";

        [English("Manage Loadout")]
        public const string ManageTitle = "weaponloadouts.manage.title";

        [English(
            "No saved loadouts",
            "You have not saved any loadouts yet. Carry the weapons you want, then use Save Loadout.")]
        public const string Empty = "weaponloadouts.empty";

        [English("Manage the ~y~{name}~s~ loadout.")]
        public const string LoadoutDescription = "weaponloadouts.loadout.desc";

        [English("{count} weapon(s)")]
        public const string WeaponCount = "weaponloadouts.loadout.count";

        [English("Equip Loadout", "Take out this loadout. Everything you are carrying now is removed first.")]
        public const string Equip = "weaponloadouts.equip";

        [English("Loadout ~g~{name}~s~ was equipped.")]
        public const string Equipped = "weaponloadouts.equipped";

        [English(
            "Loadout ~g~{name}~s~ was equipped, but ~y~{count}~s~ weapon(s) were skipped because you are not allowed them or they no longer exist.")]
        public const string EquippedWithSkipped = "weaponloadouts.equipped.skipped";

        [English("You do not have permission to equip loadouts.")]
        public const string EquipDenied = "weaponloadouts.equip.denied";

        [English("Rename Loadout", "Give this loadout a different name.")]
        public const string Rename = "weaponloadouts.rename";

        [English("Enter a new name for this loadout")]
        public const string RenamePrompt = "weaponloadouts.rename.prompt";

        [English("Renamed to ~g~{name}~s~.")]
        public const string Renamed = "weaponloadouts.renamed";

        [English("Clone Loadout", "Make a second copy of this loadout under a new name.")]
        public const string Clone = "weaponloadouts.clone";

        [English("Enter a name for the copy")]
        public const string ClonePrompt = "weaponloadouts.clone.prompt";

        [English("Copied to ~g~{name}~s~.")]
        public const string Cloned = "weaponloadouts.cloned";

        [English(
            "Set As Default Loadout",
            "Make this the loadout you get back whenever you respawn. A tick on the left marks the one you picked.")]
        public const string SetDefault = "weaponloadouts.setdefault";

        [English("~g~{name}~s~ is now your default loadout.")]
        public const string DefaultSet = "weaponloadouts.setdefault.done";

        [English(
            "~r~Replace Loadout",
            "~r~Overwrite this loadout with the weapons you are carrying right now. This cannot be undone.")]
        public const string Replace = "weaponloadouts.replace";

        [English("~g~{name}~s~ was replaced with what you are carrying.")]
        public const string Replaced = "weaponloadouts.replaced";

        [English("~r~Delete Loadout", "~r~Delete this loadout. This cannot be undone.")]
        public const string Delete = "weaponloadouts.delete";

        [English("~g~{name}~s~ was deleted.")]
        public const string Deleted = "weaponloadouts.deleted";

        [English(
            "Restore Default Loadout On Respawn",
            "Once you have picked a default loadout, get it back automatically every time you respawn, and when you join the server.")]
        public const string RestoreOnRespawn = "weaponloadouts.onrespawn";

        [English(
            "Keep Weapons On Ped Change",
            "Changing your ped normally takes your weapons with it. Leave this on and you get back exactly what you were carrying, with the same attachments, tints and ammo. A weapon that vMenu does not know about cannot be carried over.")]
        public const string KeepOnPedChange = "weaponloadouts.onpedchange";
    }
}

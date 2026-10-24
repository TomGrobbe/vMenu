namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class VehicleSpawner
    {
        [English("Vehicle Spawner")]
        public const string Title = "vehiclespawner.title";

        [English("Vehicle Spawner Menu")]
        public const string Subtitle = "vehiclespawner.subtitle";

        [English("Spawn a vehicle.")]
        public const string LinkDescription = "vehiclespawner.link.desc";

        [English("Spawn Vehicle By Category", "Spawn a vehicle from a list of vehicle categories.")]
        public const string SpawnByCategory = "vehiclespawner.bycategory";

        [English("Spawn vehicles by category")]
        public const string SpawnByCategorySubtitle = "vehiclespawner.bycategory.subtitle";

        [English("Spawn Vehicle By Name", "Spawn a vehicle by typing its model name.")]
        public const string SpawnByName = "vehiclespawner.byname";

        [English("Enter a vehicle model name")]
        public const string SpawnByNamePrompt = "vehiclespawner.byname.prompt";

        [English("~r~{model}~s~ is not a valid vehicle model.")]
        public const string SpawnByNameInvalid = "vehiclespawner.byname.invalid";

        [English("You do not have permission to spawn ~y~{model}~s~.")]
        public const string SpawnByNameDenied = "vehiclespawner.byname.denied";

        [English("Spawned ~g~{vehicle}~s~.")]
        public const string Spawned = "vehiclespawner.spawned";

        [English("Spawn a vehicle from the ~y~{category}~s~ category.")]
        public const string CategoryDescription = "vehiclespawner.category.desc";

        [English("Vehicle Spawner Menu")]
        public const string CategorySubtitle = "vehiclespawner.category.subtitle";

        [English(
            "Spawn Inside Vehicle",
            "When turned on, spawns you inside of the vehicle. Turn it off and it will spawn the car in front of you instead.")]
        public const string SpawnInside = "vehiclespawner.spawninside";

        [English("Spawn Inside: On")]
        public const string SpawnInsideButtonOn = "vehiclespawner.spawninside.button.on";

        [English("Spawn Inside: Off")]
        public const string SpawnInsideButtonOff = "vehiclespawner.spawninside.button.off";

        [English("Spawn vehicles inside or outside")]
        public const string SpawnInsideBinding = "vehiclespawner.spawninside.binding";

        [English(
            "Replace Previous Vehicle",
            "Delete the vehicle you spawned last when you spawn a new one, or the one you are driving.")]
        public const string ReplacePrevious = "vehiclespawner.replaceprevious";

        [English("This server always replaces your previous vehicle, so this cannot be turned off.")]
        public const string ReplacePreviousLocked = "vehiclespawner.replaceprevious.locked";

        [English("Replace Previous: On")]
        public const string ReplacePreviousButtonOn = "vehiclespawner.replaceprevious.button.on";

        [English("Replace Previous: Off")]
        public const string ReplacePreviousButtonOff = "vehiclespawner.replaceprevious.button.off";

        [English("Keep or replace the previous vehicle")]
        public const string ReplacePreviousBinding = "vehiclespawner.replaceprevious.binding";

        [English(
            "Your old vehicle was removed. Turn off ~y~Replace Previous Vehicle~s~ if you want to keep it next time.")]
        public const string OldVehicleRemoved = "vehiclespawner.oldvehicleremoved";

        [English(
            "Your personal vehicle was removed and forgotten. This server only lets you keep one vehicle at a time.")]
        public const string PersonalVehicleReplaced = "vehiclespawner.personalvehiclereplaced";

        [English("You are spawning vehicles too quickly. Try again in ~y~{seconds}~s~ second(s).")]
        public const string TooManySpawns = "vehiclespawner.toomanyspawns";
    }
}

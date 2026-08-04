
namespace vMenu.Enhanced.MenuFramework.Localization.Languages;

/// <summary>
/// The English strings, and the fallback for every other language.
/// </summary>
/// <remarks>
/// This table is required to be complete: anything missing here renders as a marker in game and is
/// reported by <see cref="LocalizationSelfCheck"/> at startup. Other tables may be partial.
/// </remarks>
internal static class EnglishStrings
{
    internal static LanguageTable Table { get; } = new(
        LanguageId.English,
        "English",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Loc.Framework.RestrictedDescription] = "Access to this has been restricted by the server owner.",
            [Loc.Framework.InputPlaceholder] = "Start typing…",
            [Loc.Framework.InputHint] = "Enter to confirm · Esc to cancel · ↑↓ to pick a suggestion · Tab to complete",
            [Loc.Framework.InputNoMatches] = "No matches",

            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Main Menu",

            [Loc.VehicleSpawner.Title] = "Vehicle Spawner",
            [Loc.VehicleSpawner.Subtitle] = "Vehicle Spawner Menu",
            [Loc.VehicleSpawner.LinkDescription] = "Spawn a vehicle.",
            [Loc.VehicleSpawner.SpawnByClass] = "Spawn Vehicle By Class",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Spawn a vehicle from a list of vehicle classes.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Spawn vehicles by class",
            [Loc.VehicleSpawner.SpawnByName] = "Spawn Vehicle By Name",
            [Loc.VehicleSpawner.SpawnByNameDescription] = "Spawn a vehicle by typing its model name.",
            [Loc.VehicleSpawner.SpawnByNamePrompt] = "Enter a vehicle model name",
            [Loc.VehicleSpawner.SpawnByNameInvalid] = "~r~{model}~s~ is not a valid vehicle model.",
            [Loc.VehicleSpawner.SpawnByNameDenied] = "You do not have permission to spawn ~y~{model}~s~.",
            [Loc.VehicleSpawner.Spawned] = "Spawned ~g~{vehicle}~s~.",
            [Loc.VehicleSpawner.ClassDescription] = "Spawn a vehicle from the ~y~{class}~s~ class.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Vehicle Spawner Menu",

            [Loc.VehicleOptions.Title] = "Vehicle Options",
            [Loc.VehicleOptions.Subtitle] = "Vehicle Options Menu",
            [Loc.VehicleOptions.LinkDescription] = "Options for your current vehicle.",
            [Loc.VehicleOptions.DeleteVehicle] = "Delete Vehicle",
            [Loc.VehicleOptions.DeleteVehicleDescription] = "Deletes the vehicle you're driving, or the one in front of you.",
            [Loc.VehicleOptions.DeleteNotDriver] = "You have to be the ~y~driver~s~ to delete this vehicle.",
            [Loc.VehicleOptions.DeleteNoVehicle] = "~r~No vehicle found~s~ in front of you.",
            [Loc.VehicleOptions.DeleteDenied] = "You do not have permission to delete vehicles.",
            [Loc.VehicleOptions.DeleteTooFar] = "You are ~r~too far away~s~ from that vehicle.",
            [Loc.VehicleOptions.DeleteFailed] = "~r~Could not delete~s~ that vehicle. Try again.",
            [Loc.VehicleOptions.Deleted] = "~g~Vehicle deleted~s~.",

            [Loc.MiscSettings.Title] = "Miscellaneous Settings",
            [Loc.MiscSettings.Subtitle] = "Miscellaneous Settings",
            [Loc.MiscSettings.LinkDescription] = "Change vMenu's own settings.",
            [Loc.MiscSettings.Language] = "Language",
            [Loc.MiscSettings.LanguageDescription] = "Select a language and press enter to apply it.",
            [Loc.MiscSettings.MenuRightAlignment] = "Right Align Menu",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Press Enter to toggle left or right aligned menu.",
            [Loc.MiscSettings.MenuRightAlignmentUnsupported] = "A right aligned menu is not supported here, so the menu has been left aligned.",

            [Loc.DeveloperFeatures.Title] = "Developer Features",
            [Loc.DeveloperFeatures.Subtitle] = "Developer Features",
            [Loc.DeveloperFeatures.LinkDescription] = "Development and debugging tools.",
            [Loc.DeveloperFeatures.VehicleDimensions] = "Show Vehicle Dimensions",
            [Loc.DeveloperFeatures.VehicleDimensionsDescription] = "Draws the model outlines for every vehicle that's currently close to you.",
            [Loc.DeveloperFeatures.PropDimensions] = "Show Prop Dimensions",
            [Loc.DeveloperFeatures.PropDimensionsDescription] = "Draws the model outlines for every prop that's currently close to you.",
            [Loc.DeveloperFeatures.PedDimensions] = "Show Ped Dimensions",
            [Loc.DeveloperFeatures.PedDimensionsDescription] = "Draws the model outlines for every ped that's currently close to you.",
            [Loc.DeveloperFeatures.EntityHandles] = "Show Entity Handles",
            [Loc.DeveloperFeatures.EntityHandlesDescription] = "Draws the entity handle of every nearby entity. Enable one of the outline options above for this to show up.",
            [Loc.DeveloperFeatures.EntityModels] = "Show Entity Models",
            [Loc.DeveloperFeatures.EntityModelsDescription] = "Draws the model hash of every nearby entity. Enable one of the outline options above for this to show up.",
            [Loc.DeveloperFeatures.NetworkOwners] = "Show Network Owners",
            [Loc.DeveloperFeatures.NetworkOwnersDescription] = "Draws the network owner of every nearby entity. Enable one of the outline options above for this to show up.",
            [Loc.DeveloperFeatures.DrawRadius] = "Show Dimensions Radius",
            [Loc.DeveloperFeatures.DrawRadiusDescription] = "How far away entities are still outlined and labelled. Currently {radius}.",
            [Loc.DeveloperFeatures.BoxOpacity] = "Dimensions Box Opacity",
            [Loc.DeveloperFeatures.BoxOpacityDescription] = "How solidly the coloured boxes are filled in. The outlines and labels around them are not affected. Currently {opacity}.",
        });
}

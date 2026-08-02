
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
            [Loc.VehicleSpawner.ClassDescription] = "Spawn a vehicle from the ~y~{class}~s~ class.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Vehicle Spawner Menu",

            [Loc.MiscSettings.Title] = "Miscellaneous Settings",
            [Loc.MiscSettings.Subtitle] = "Miscellaneous Settings",
            [Loc.MiscSettings.LinkDescription] = "Change vMenu's own settings.",
            [Loc.MiscSettings.Language] = "Language",
            [Loc.MiscSettings.LanguageDescription] = "Select a language and press enter to apply it.",
            [Loc.MiscSettings.MenuRightAlignment] = "Right Align Menu",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Press Enter to toggle left or right aligned menu.",
        });
}


namespace vMenu.Enhanced.MenuFramework.Localization.Languages;

/// <summary>
/// Dutch. Partial tables are fine — anything absent falls back to <see cref="EnglishStrings"/>.
/// </summary>
internal static class DutchStrings
{
    internal static LanguageTable Table { get; } = new(
        new LanguageId("nl"),
        "Nederlands",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Loc.Framework.RestrictedDescription] = "De servereigenaar heeft de toegang hiertoe beperkt.",
            [Loc.Framework.InputPlaceholder] = "Begin met typen…",
            [Loc.Framework.InputHint] = "Enter om te bevestigen · Esc om te annuleren · ↑↓ om een suggestie te kiezen · Tab om aan te vullen",
            [Loc.Framework.InputNoMatches] = "Geen resultaten",

            // "vMenu Enhanced" is a product name, so it is deliberately left untranslated.
            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Hoofdmenu",

            [Loc.VehicleSpawner.Title] = "Voertuigspawner",
            [Loc.VehicleSpawner.Subtitle] = "Voertuigspawnermenu",
            [Loc.VehicleSpawner.LinkDescription] = "Spawn een voertuig.",
            [Loc.VehicleSpawner.SpawnByClass] = "Voertuig spawnen op klasse",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Spawn een voertuig uit een lijst met voertuigklassen.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Voertuigen spawnen op klasse",
            [Loc.VehicleSpawner.SpawnByName] = "Voertuig spawnen op naam",
            [Loc.VehicleSpawner.SpawnByNameDescription] = "Spawn een voertuig door de modelnaam te typen.",
            [Loc.VehicleSpawner.SpawnByNamePrompt] = "Voer een voertuigmodelnaam in",
            [Loc.VehicleSpawner.SpawnByNameInvalid] = "~r~{model}~s~ is geen geldig voertuigmodel.",
            [Loc.VehicleSpawner.SpawnByNameDenied] = "Je hebt geen toestemming om ~y~{model}~s~ te spawnen.",
            [Loc.VehicleSpawner.ClassDescription] = "Spawn een voertuig uit de klasse ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Voertuigspawnermenu",

            [Loc.MiscSettings.Title] = "Overige instellingen",
            [Loc.MiscSettings.Subtitle] = "Overige instellingen",
            [Loc.MiscSettings.LinkDescription] = "Pas de instellingen van vMenu aan.",
            [Loc.MiscSettings.Language] = "Taal",
            [Loc.MiscSettings.LanguageDescription] = "Selecteer een taal en druk op enter om deze toe te passen.",
            [Loc.MiscSettings.MenuRightAlignment] = "Menu rechts uitlijnen",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Druk op enter om het menu links of rechts uit te lijnen.",
        });
}

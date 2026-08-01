namespace vMenu.Enhanced.Localization.Languages;

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

            // "vMenu Enhanced" is a product name, so it is deliberately left untranslated.
            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Hoofdmenu",

            [Loc.VehicleSpawner.Title] = "Voertuigspawner",
            [Loc.VehicleSpawner.Subtitle] = "Voertuigspawnermenu",
            [Loc.VehicleSpawner.LinkDescription] = "Spawn een voertuig.",
            [Loc.VehicleSpawner.SpawnByClass] = "Voertuig spawnen op klasse",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Spawn een voertuig uit een lijst met voertuigklassen.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Voertuigen spawnen op klasse",
            [Loc.VehicleSpawner.ClassDescription] = "Spawn een voertuig uit de klasse ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Voertuigspawnermenu",

            [Loc.MiscSettings.Title] = "Overige instellingen",
            [Loc.MiscSettings.Subtitle] = "Overige instellingen",
            [Loc.MiscSettings.LinkDescription] = "Pas de instellingen van vMenu aan.",
            [Loc.MiscSettings.Language] = "Taal",
            [Loc.MiscSettings.LanguageDescription] = "Selecteer een taal en druk op enter om deze toe te passen.",
        });
}

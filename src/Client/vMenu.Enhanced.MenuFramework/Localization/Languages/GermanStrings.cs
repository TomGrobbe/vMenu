namespace vMenu.Enhanced.Localization.Languages;

/// <summary>
/// German. Partial tables are fine — anything absent falls back to <see cref="EnglishStrings"/>.
/// </summary>
internal static class GermanStrings
{
    internal static LanguageTable Table { get; } = new(
        new LanguageId("de"),
        "Deutsch",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Loc.Framework.RestrictedDescription] = "Der Serverbesitzer hat den Zugriff darauf eingeschränkt.",

            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Hauptmenü",

            [Loc.VehicleSpawner.Title] = "Fahrzeug-Spawner",
            [Loc.VehicleSpawner.Subtitle] = "Fahrzeug-Spawner-Menü",
            [Loc.VehicleSpawner.LinkDescription] = "Ein Fahrzeug spawnen.",
            [Loc.VehicleSpawner.SpawnByClass] = "Fahrzeug nach Klasse spawnen",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Spawne ein Fahrzeug aus einer Liste von Fahrzeugklassen.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Fahrzeuge nach Klasse spawnen",
            [Loc.VehicleSpawner.ClassDescription] = "Spawne ein Fahrzeug aus der Klasse ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Fahrzeug-Spawner-Menü",

            [Loc.MiscSettings.Title] = "Sonstige Einstellungen",
            [Loc.MiscSettings.Subtitle] = "Sonstige Einstellungen",
            [Loc.MiscSettings.LinkDescription] = "Ändere die Einstellungen von vMenu.",
            [Loc.MiscSettings.Language] = "Sprache",
            [Loc.MiscSettings.LanguageDescription] = "Wähle eine Sprache und drücke Enter, um sie anzuwenden.",
        });
}


namespace vMenu.Enhanced.MenuFramework.Localization.Languages;

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
            [Loc.Framework.InputPlaceholder] = "Tippe los…",
            [Loc.Framework.InputHint] = "Enter zum Bestätigen · Esc zum Abbrechen · ↑↓ für einen Vorschlag · Tab zum Vervollständigen",
            [Loc.Framework.InputNoMatches] = "Keine Treffer",

            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Hauptmenü",

            [Loc.VehicleSpawner.Title] = "Fahrzeug-Spawner",
            [Loc.VehicleSpawner.Subtitle] = "Fahrzeug-Spawner-Menü",
            [Loc.VehicleSpawner.LinkDescription] = "Ein Fahrzeug spawnen.",
            [Loc.VehicleSpawner.SpawnByClass] = "Fahrzeug nach Klasse spawnen",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Spawne ein Fahrzeug aus einer Liste von Fahrzeugklassen.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Fahrzeuge nach Klasse spawnen",
            [Loc.VehicleSpawner.SpawnByName] = "Fahrzeug nach Name spawnen",
            [Loc.VehicleSpawner.SpawnByNameDescription] = "Spawne ein Fahrzeug, indem du seinen Modellnamen eingibst.",
            [Loc.VehicleSpawner.SpawnByNamePrompt] = "Gib einen Fahrzeugmodellnamen ein",
            [Loc.VehicleSpawner.SpawnByNameInvalid] = "~r~{model}~s~ ist kein gültiges Fahrzeugmodell.",
            [Loc.VehicleSpawner.SpawnByNameDenied] = "Du hast keine Berechtigung, ~y~{model}~s~ zu spawnen.",
            [Loc.VehicleSpawner.Spawned] = "~g~{vehicle}~s~ wurde gespawnt.",
            [Loc.VehicleSpawner.ClassDescription] = "Spawne ein Fahrzeug aus der Klasse ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Fahrzeug-Spawner-Menü",

            [Loc.VehicleOptions.Title] = "Fahrzeugoptionen",
            [Loc.VehicleOptions.Subtitle] = "Fahrzeugoptionen",
            [Loc.VehicleOptions.LinkDescription] = "Optionen für dein aktuelles Fahrzeug.",
            [Loc.VehicleOptions.DeleteVehicle] = "Fahrzeug löschen",
            [Loc.VehicleOptions.DeleteVehicleDescription] = "Löscht das Fahrzeug, das du fährst, oder das vor dir.",
            [Loc.VehicleOptions.DeleteNotDriver] = "Du musst der ~y~Fahrer~s~ sein, um dieses Fahrzeug zu löschen.",
            [Loc.VehicleOptions.DeleteNoVehicle] = "~r~Kein Fahrzeug~s~ vor dir gefunden.",
            [Loc.VehicleOptions.DeleteDenied] = "Du hast keine Berechtigung, Fahrzeuge zu löschen.",
            [Loc.VehicleOptions.DeleteTooFar] = "Du bist ~r~zu weit entfernt~s~ von diesem Fahrzeug.",
            [Loc.VehicleOptions.DeleteFailed] = "~r~Konnte das Fahrzeug nicht löschen~s~. Versuch es erneut.",
            [Loc.VehicleOptions.Deleted] = "~g~Fahrzeug gelöscht~s~.",

            [Loc.MiscSettings.Title] = "Sonstige Einstellungen",
            [Loc.MiscSettings.Subtitle] = "Sonstige Einstellungen",
            [Loc.MiscSettings.LinkDescription] = "Ändere die Einstellungen von vMenu.",
            [Loc.MiscSettings.Language] = "Sprache",
            [Loc.MiscSettings.LanguageDescription] = "Wähle eine Sprache und drücke Enter, um sie anzuwenden.",
            [Loc.MiscSettings.MenuRightAlignment] = "Menü rechts ausrichten",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Drücke Enter, um das Menü links oder rechts auszurichten.",
            [Loc.MiscSettings.MenuRightAlignmentUnsupported] = "Eine rechtsbündige Ausrichtung wird hier nicht unterstützt, daher wurde das Menü links ausgerichtet.",

            [Loc.DeveloperFeatures.Title] = "Entwicklerfunktionen",
            [Loc.DeveloperFeatures.Subtitle] = "Entwicklerfunktionen",
            [Loc.DeveloperFeatures.LinkDescription] = "Werkzeuge zum Entwickeln und Debuggen.",
            [Loc.DeveloperFeatures.VehicleDimensions] = "Fahrzeugabmessungen anzeigen",
            [Loc.DeveloperFeatures.VehicleDimensionsDescription] = "Zeichnet die Modellumrisse jedes Fahrzeugs in deiner Nähe.",
            [Loc.DeveloperFeatures.PropDimensions] = "Prop-Abmessungen anzeigen",
            [Loc.DeveloperFeatures.PropDimensionsDescription] = "Zeichnet die Modellumrisse jedes Props in deiner Nähe.",
            [Loc.DeveloperFeatures.PedDimensions] = "Ped-Abmessungen anzeigen",
            [Loc.DeveloperFeatures.PedDimensionsDescription] = "Zeichnet die Modellumrisse jedes Peds in deiner Nähe.",
            [Loc.DeveloperFeatures.EntityHandles] = "Entity-Handles anzeigen",
            [Loc.DeveloperFeatures.EntityHandlesDescription] = "Zeichnet das Entity-Handle jeder Entity in der Nähe. Aktiviere dafür eine der Umriss-Optionen oben.",
            [Loc.DeveloperFeatures.EntityModels] = "Entity-Modelle anzeigen",
            [Loc.DeveloperFeatures.EntityModelsDescription] = "Zeichnet den Modell-Hash jeder Entity in der Nähe. Aktiviere dafür eine der Umriss-Optionen oben.",
            [Loc.DeveloperFeatures.NetworkOwners] = "Netzwerkbesitzer anzeigen",
            [Loc.DeveloperFeatures.NetworkOwnersDescription] = "Zeichnet den Netzwerkbesitzer jeder Entity in der Nähe. Aktiviere dafür eine der Umriss-Optionen oben.",
            [Loc.DeveloperFeatures.DrawRadius] = "Anzeigereichweite",
            [Loc.DeveloperFeatures.DrawRadiusDescription] = "Wie weit entfernt Entities noch umrissen und beschriftet werden. Aktuell {radius}.",
            [Loc.DeveloperFeatures.BoxOpacity] = "Deckkraft der Maßboxen",
            [Loc.DeveloperFeatures.BoxOpacityDescription] = "Wie stark die farbigen Boxen ausgefüllt werden. Umrisse und Beschriftungen bleiben unverändert. Aktuell {opacity}.",
        });
}

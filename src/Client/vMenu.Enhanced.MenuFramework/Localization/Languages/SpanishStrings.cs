
namespace vMenu.Enhanced.MenuFramework.Localization.Languages;

/// <summary>
/// Spanish. Partial tables are fine — anything absent falls back to <see cref="EnglishStrings"/>.
/// </summary>
internal static class SpanishStrings
{
    internal static LanguageTable Table { get; } = new(
        new LanguageId("es"),
        "Español",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Loc.Framework.RestrictedDescription] = "El propietario del servidor ha restringido el acceso a esto.",
            [Loc.Framework.InputPlaceholder] = "Empieza a escribir…",
            [Loc.Framework.InputHint] = "Intro para confirmar · Esc para cancelar · ↑↓ para elegir una sugerencia · Tab para completar",
            [Loc.Framework.InputNoMatches] = "Sin coincidencias",

            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Menú principal",

            [Loc.VehicleSpawner.Title] = "Generador de vehículos",
            [Loc.VehicleSpawner.Subtitle] = "Menú del generador de vehículos",
            [Loc.VehicleSpawner.LinkDescription] = "Genera un vehículo.",
            [Loc.VehicleSpawner.SpawnByClass] = "Generar vehículo por clase",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Genera un vehículo a partir de una lista de clases de vehículos.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Generar vehículos por clase",
            [Loc.VehicleSpawner.SpawnByName] = "Generar vehículo por nombre",
            [Loc.VehicleSpawner.SpawnByNameDescription] = "Genera un vehículo escribiendo el nombre de su modelo.",
            [Loc.VehicleSpawner.SpawnByNamePrompt] = "Introduce el nombre de un modelo de vehículo",
            [Loc.VehicleSpawner.SpawnByNameInvalid] = "~r~{model}~s~ no es un modelo de vehículo válido.",
            [Loc.VehicleSpawner.SpawnByNameDenied] = "No tienes permiso para generar ~y~{model}~s~.",
            [Loc.VehicleSpawner.ClassDescription] = "Genera un vehículo de la clase ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Menú del generador de vehículos",

            [Loc.MiscSettings.Title] = "Ajustes varios",
            [Loc.MiscSettings.Subtitle] = "Ajustes varios",
            [Loc.MiscSettings.LinkDescription] = "Cambia los ajustes de vMenu.",
            [Loc.MiscSettings.Language] = "Idioma",
            [Loc.MiscSettings.LanguageDescription] = "Selecciona un idioma y pulsa intro para aplicarlo.",
            [Loc.MiscSettings.MenuRightAlignment] = "Alinear el menú a la derecha",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Pulsa intro para alinear el menú a la izquierda o a la derecha.",
        });
}

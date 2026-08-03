
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
            [Loc.VehicleSpawner.Spawned] = "~g~{vehicle}~s~ generado.",
            [Loc.VehicleSpawner.ClassDescription] = "Genera un vehículo de la clase ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Menú del generador de vehículos",

            [Loc.VehicleOptions.Title] = "Opciones de vehículo",
            [Loc.VehicleOptions.Subtitle] = "Menú de opciones de vehículo",
            [Loc.VehicleOptions.LinkDescription] = "Opciones para tu vehículo actual.",
            [Loc.VehicleOptions.DeleteVehicle] = "Eliminar vehículo",
            [Loc.VehicleOptions.DeleteVehicleDescription] = "Elimina el vehículo que conduces o el que tienes delante.",
            [Loc.VehicleOptions.DeleteNotDriver] = "Tienes que ser el ~y~conductor~s~ para eliminar este vehículo.",
            [Loc.VehicleOptions.DeleteNoVehicle] = "~r~No se ha encontrado ningún vehículo~s~ delante de ti.",
            [Loc.VehicleOptions.DeleteDenied] = "No tienes permiso para eliminar vehículos.",
            [Loc.VehicleOptions.DeleteTooFar] = "Estás ~r~demasiado lejos~s~ de ese vehículo.",
            [Loc.VehicleOptions.DeleteFailed] = "~r~No se ha podido eliminar~s~ ese vehículo. Inténtalo de nuevo.",
            [Loc.VehicleOptions.Deleted] = "~g~Vehículo eliminado~s~.",

            [Loc.MiscSettings.Title] = "Ajustes varios",
            [Loc.MiscSettings.Subtitle] = "Ajustes varios",
            [Loc.MiscSettings.LinkDescription] = "Cambia los ajustes de vMenu.",
            [Loc.MiscSettings.Language] = "Idioma",
            [Loc.MiscSettings.LanguageDescription] = "Selecciona un idioma y pulsa intro para aplicarlo.",
            [Loc.MiscSettings.MenuRightAlignment] = "Alinear el menú a la derecha",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Pulsa intro para alinear el menú a la izquierda o a la derecha.",

            [Loc.DeveloperFeatures.Title] = "Funciones de desarrollo",
            [Loc.DeveloperFeatures.Subtitle] = "Funciones de desarrollo",
            [Loc.DeveloperFeatures.LinkDescription] = "Herramientas de desarrollo y depuración.",
            [Loc.DeveloperFeatures.VehicleDimensions] = "Mostrar dimensiones de vehículos",
            [Loc.DeveloperFeatures.VehicleDimensionsDescription] = "Dibuja el contorno del modelo de cada vehículo que tengas cerca.",
            [Loc.DeveloperFeatures.PropDimensions] = "Mostrar dimensiones de props",
            [Loc.DeveloperFeatures.PropDimensionsDescription] = "Dibuja el contorno del modelo de cada prop que tengas cerca.",
            [Loc.DeveloperFeatures.PedDimensions] = "Mostrar dimensiones de peds",
            [Loc.DeveloperFeatures.PedDimensionsDescription] = "Dibuja el contorno del modelo de cada ped que tengas cerca.",
            [Loc.DeveloperFeatures.EntityHandles] = "Mostrar handles de entidad",
            [Loc.DeveloperFeatures.EntityHandlesDescription] = "Dibuja el handle de cada entidad cercana. Activa una de las opciones de contorno de arriba para verlo.",
            [Loc.DeveloperFeatures.EntityModels] = "Mostrar modelos de entidad",
            [Loc.DeveloperFeatures.EntityModelsDescription] = "Dibuja el hash del modelo de cada entidad cercana. Activa una de las opciones de contorno de arriba para verlo.",
            [Loc.DeveloperFeatures.NetworkOwners] = "Mostrar propietarios de red",
            [Loc.DeveloperFeatures.NetworkOwnersDescription] = "Dibuja el propietario de red de cada entidad cercana. Activa una de las opciones de contorno de arriba para verlo.",
            [Loc.DeveloperFeatures.DrawRadius] = "Distancia de dibujado",
            [Loc.DeveloperFeatures.DrawRadiusDescription] = "Hasta qué distancia se siguen contorneando y etiquetando las entidades. Actualmente {radius}.",
            [Loc.DeveloperFeatures.BoxOpacity] = "Opacidad de las cajas de dimensiones",
            [Loc.DeveloperFeatures.BoxOpacityDescription] = "Cuánto se rellenan las cajas de color. Los contornos y las etiquetas alrededor no cambian. Actualmente {opacity}.",
        });
}

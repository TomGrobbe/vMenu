
namespace vMenu.Enhanced.MenuFramework.Localization.Languages;

/// <summary>
/// French. Partial tables are fine — anything absent falls back to <see cref="EnglishStrings"/>.
/// </summary>
internal static class FrenchStrings
{
    internal static LanguageTable Table { get; } = new(
        new LanguageId("fr"),
        "Français",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Loc.Framework.RestrictedDescription] = "Le propriétaire du serveur a restreint l'accès à cet élément.",
            [Loc.Framework.InputPlaceholder] = "Commencez à taper…",
            [Loc.Framework.InputHint] = "Entrée pour confirmer · Échap pour annuler · ↑↓ pour choisir une suggestion · Tab pour compléter",
            [Loc.Framework.InputNoMatches] = "Aucun résultat",

            [Loc.MainMenu.Title] = "vMenu Enhanced",
            [Loc.MainMenu.Subtitle] = "Menu principal",

            [Loc.VehicleSpawner.Title] = "Générateur de véhicules",
            [Loc.VehicleSpawner.Subtitle] = "Menu du générateur de véhicules",
            [Loc.VehicleSpawner.LinkDescription] = "Faire apparaître un véhicule.",
            [Loc.VehicleSpawner.SpawnByClass] = "Faire apparaître un véhicule par classe",
            [Loc.VehicleSpawner.SpawnByClassDescription] = "Faites apparaître un véhicule à partir d'une liste de classes de véhicules.",
            [Loc.VehicleSpawner.SpawnByClassSubtitle] = "Faire apparaître des véhicules par classe",
            [Loc.VehicleSpawner.SpawnByName] = "Faire apparaître un véhicule par nom",
            [Loc.VehicleSpawner.SpawnByNameDescription] = "Faites apparaître un véhicule en saisissant le nom de son modèle.",
            [Loc.VehicleSpawner.SpawnByNamePrompt] = "Saisissez un nom de modèle de véhicule",
            [Loc.VehicleSpawner.SpawnByNameInvalid] = "~r~{model}~s~ n'est pas un modèle de véhicule valide.",
            [Loc.VehicleSpawner.SpawnByNameDenied] = "Vous n'avez pas la permission de faire apparaître ~y~{model}~s~.",
            [Loc.VehicleSpawner.Spawned] = "~g~{vehicle}~s~ est apparu.",
            [Loc.VehicleSpawner.ClassDescription] = "Faites apparaître un véhicule de la classe ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Menu du générateur de véhicules",

            [Loc.MiscSettings.Title] = "Paramètres divers",
            [Loc.MiscSettings.Subtitle] = "Paramètres divers",
            [Loc.MiscSettings.LinkDescription] = "Modifier les paramètres de vMenu.",
            [Loc.MiscSettings.Language] = "Langue",
            [Loc.MiscSettings.LanguageDescription] = "Sélectionnez une langue et appuyez sur Entrée pour l'appliquer.",
            [Loc.MiscSettings.MenuRightAlignment] = "Aligner le menu à droite",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Appuyez sur Entrée pour aligner le menu à gauche ou à droite.",

            [Loc.DeveloperFeatures.Title] = "Fonctions de développement",
            [Loc.DeveloperFeatures.Subtitle] = "Fonctions de développement",
            [Loc.DeveloperFeatures.LinkDescription] = "Outils de développement et de débogage.",
            [Loc.DeveloperFeatures.VehicleDimensions] = "Afficher les dimensions des véhicules",
            [Loc.DeveloperFeatures.VehicleDimensionsDescription] = "Dessine le contour du modèle de chaque véhicule proche de vous.",
            [Loc.DeveloperFeatures.PropDimensions] = "Afficher les dimensions des props",
            [Loc.DeveloperFeatures.PropDimensionsDescription] = "Dessine le contour du modèle de chaque prop proche de vous.",
            [Loc.DeveloperFeatures.PedDimensions] = "Afficher les dimensions des peds",
            [Loc.DeveloperFeatures.PedDimensionsDescription] = "Dessine le contour du modèle de chaque ped proche de vous.",
            [Loc.DeveloperFeatures.EntityHandles] = "Afficher les handles d'entité",
            [Loc.DeveloperFeatures.EntityHandlesDescription] = "Dessine le handle de chaque entité proche. Activez l'une des options de contour ci-dessus pour le voir.",
            [Loc.DeveloperFeatures.EntityModels] = "Afficher les modèles d'entité",
            [Loc.DeveloperFeatures.EntityModelsDescription] = "Dessine le hash du modèle de chaque entité proche. Activez l'une des options de contour ci-dessus pour le voir.",
            [Loc.DeveloperFeatures.NetworkOwners] = "Afficher les propriétaires réseau",
            [Loc.DeveloperFeatures.NetworkOwnersDescription] = "Dessine le propriétaire réseau de chaque entité proche. Activez l'une des options de contour ci-dessus pour le voir.",
            [Loc.DeveloperFeatures.DrawRadius] = "Distance d'affichage",
            [Loc.DeveloperFeatures.DrawRadiusDescription] = "Jusqu'à quelle distance les entités restent entourées et étiquetées.",
        });
}

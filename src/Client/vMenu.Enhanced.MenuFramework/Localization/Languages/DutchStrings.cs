
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
            [Loc.VehicleSpawner.Spawned] = "~g~{vehicle}~s~ gespawnd.",
            [Loc.VehicleSpawner.ClassDescription] = "Spawn een voertuig uit de klasse ~y~{class}~s~.",
            [Loc.VehicleSpawner.ClassSubtitle] = "Voertuigspawnermenu",

            [Loc.VehicleOptions.Title] = "Voertuigopties",
            [Loc.VehicleOptions.Subtitle] = "Voertuigoptiesmenu",
            [Loc.VehicleOptions.LinkDescription] = "Opties voor je huidige voertuig.",
            [Loc.VehicleOptions.DeleteVehicle] = "Voertuig verwijderen",
            [Loc.VehicleOptions.DeleteVehicleDescription] = "Verwijdert het voertuig waarin je rijdt, of dat voor je staat.",
            [Loc.VehicleOptions.DeleteNotDriver] = "Je moet de ~y~bestuurder~s~ zijn om dit voertuig te verwijderen.",
            [Loc.VehicleOptions.DeleteNoVehicle] = "~r~Geen voertuig gevonden~s~ voor je.",
            [Loc.VehicleOptions.DeleteDenied] = "Je hebt geen toestemming om voertuigen te verwijderen.",
            [Loc.VehicleOptions.DeleteTooFar] = "Je bent ~r~te ver weg~s~ van dat voertuig.",
            [Loc.VehicleOptions.DeleteFailed] = "~r~Kon dat voertuig niet verwijderen~s~. Probeer het opnieuw.",
            [Loc.VehicleOptions.Deleted] = "~g~Voertuig verwijderd~s~.",

            [Loc.MiscSettings.Title] = "Overige instellingen",
            [Loc.MiscSettings.Subtitle] = "Overige instellingen",
            [Loc.MiscSettings.LinkDescription] = "Pas de instellingen van vMenu aan.",
            [Loc.MiscSettings.Language] = "Taal",
            [Loc.MiscSettings.LanguageDescription] = "Selecteer een taal en druk op enter om deze toe te passen.",
            [Loc.MiscSettings.MenuRightAlignment] = "Menu rechts uitlijnen",
            [Loc.MiscSettings.MenuRightAlignmentDescription] = "Druk op enter om het menu links of rechts uit te lijnen.",

            [Loc.DeveloperFeatures.Title] = "Ontwikkelaarsfuncties",
            [Loc.DeveloperFeatures.Subtitle] = "Ontwikkelaarsfuncties",
            [Loc.DeveloperFeatures.LinkDescription] = "Hulpmiddelen voor ontwikkelen en debuggen.",
            [Loc.DeveloperFeatures.VehicleDimensions] = "Voertuigafmetingen tonen",
            [Loc.DeveloperFeatures.VehicleDimensionsDescription] = "Tekent de modelomlijning van elk voertuig dat op dit moment bij je in de buurt is.",
            [Loc.DeveloperFeatures.PropDimensions] = "Propafmetingen tonen",
            [Loc.DeveloperFeatures.PropDimensionsDescription] = "Tekent de modelomlijning van elke prop die op dit moment bij je in de buurt is.",
            [Loc.DeveloperFeatures.PedDimensions] = "Pedafmetingen tonen",
            [Loc.DeveloperFeatures.PedDimensionsDescription] = "Tekent de modelomlijning van elke ped die op dit moment bij je in de buurt is.",
            [Loc.DeveloperFeatures.EntityHandles] = "Entity handles tonen",
            [Loc.DeveloperFeatures.EntityHandlesDescription] = "Tekent de entity handle van elke entity in de buurt. Zet hierboven een van de omlijningsopties aan om dit te zien.",
            [Loc.DeveloperFeatures.EntityModels] = "Entity modellen tonen",
            [Loc.DeveloperFeatures.EntityModelsDescription] = "Tekent de modelhash van elke entity in de buurt. Zet hierboven een van de omlijningsopties aan om dit te zien.",
            [Loc.DeveloperFeatures.NetworkOwners] = "Netwerkeigenaren tonen",
            [Loc.DeveloperFeatures.NetworkOwnersDescription] = "Tekent de netwerkeigenaar van elke entity in de buurt. Zet hierboven een van de omlijningsopties aan om dit te zien.",
            [Loc.DeveloperFeatures.DrawRadius] = "Weergaveafstand",
            [Loc.DeveloperFeatures.DrawRadiusDescription] = "Tot hoe ver entities nog omlijnd en gelabeld worden.",
        });
}

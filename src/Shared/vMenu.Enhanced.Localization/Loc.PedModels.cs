namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class PedModels
    {
        [English("Ped Models")]
        public const string Title = "pedmodels.title";

        [English("Ped Models Menu")]
        public const string Subtitle = "pedmodels.subtitle";

        [English("Turn yourself into a different ped.")]
        public const string LinkDescription = "pedmodels.link.desc";

        [English("Turn into this ped.\nModel: ~b~{model}~s~\nName: ~b~{label}~s~")]
        public const string PedDescription = "pedmodels.ped.desc";

        [English(
            "~r~{model}~s~ is not a valid ped model. It is listed in config/ped-models.json, but the game does not know it.")]
        public const string PedInvalid = "pedmodels.ped.invalid";

        [English("Turn into a ped from the ~y~{category}~s~ group.")]
        public const string CategoryDescription = "pedmodels.category.desc";

        [English("Ped Models Menu")]
        public const string CategorySubtitle = "pedmodels.category.subtitle";

        [English("Filter: {query} ({count} of {total})")]
        public const string CategorySubtitleFiltered = "pedmodels.category.subtitle.filtered";

        [English("Filter")]
        public const string FilterButton = "pedmodels.filter.button";

        [English("Filter the ped list")]
        public const string FilterBinding = "pedmodels.filter.binding";

        [English("Filter {category} by name")]
        public const string FilterPrompt = "pedmodels.filter.prompt";

        [English(
            "~g~{count}~s~ ped(s) match ~y~{query}~s~. Press space again and leave the box empty to show them all.")]
        public const string FilterApplied = "pedmodels.filter.applied";

        [English("No ped in ~y~{category}~s~ matches ~r~{query}~s~, so the list was left alone.")]
        public const string FilterNoMatches = "pedmodels.filter.nomatches";

        [English("Showing every ped in ~y~{category}~s~ again.")]
        public const string FilterCleared = "pedmodels.filter.cleared";

        [English("Spawn Ped By Name")]
        public const string SpawnByName = "pedmodels.byname";

        [English("Turn into a ped by typing its model name.")]
        public const string SpawnByNameDescription = "pedmodels.byname.desc";

        [English("Enter a ped model name")]
        public const string SpawnByNamePrompt = "pedmodels.byname.prompt";

        [English("~r~{model}~s~ is not a ped model this server has.")]
        public const string SpawnByNameInvalid = "pedmodels.byname.invalid";

        [English("You do not have permission to turn into ~y~{model}~s~.")]
        public const string SpawnByNameDenied = "pedmodels.byname.denied";

        [English("You are now ~g~{ped}~s~.")]
        public const string Spawned = "pedmodels.spawned";

        [English("~r~{model}~s~ could not be loaded.")]
        public const string SpawnFailed = "pedmodels.spawnfailed";

        [English("No ped models")]
        public const string Empty = "pedmodels.empty";

        [English("The server owner has not put any peds in config/ped-models.json, so there is nothing to pick from.")]
        public const string EmptyDescription = "pedmodels.empty.desc";
    }
}

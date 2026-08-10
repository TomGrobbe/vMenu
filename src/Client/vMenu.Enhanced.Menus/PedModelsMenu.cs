using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;

using PedModelsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PedModels;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Turns the player into a different ped, grouped the way the server owner grouped them in
/// <c>config/ped-models.json</c>.
/// </summary>
[VMenu(
    TitleKey = Loc.PedModels.Title,
    SubtitleKey = Loc.PedModels.Subtitle,
    DescriptionKey = Loc.PedModels.LinkDescription,
    Permission = PedModelsPermissions.Menu)]
public sealed class PedModelsMenu : MenuDefinition
{
    /// <summary>
    /// Roughly how many characters fit on one row before the model name on the left and the display
    /// name on the right run into each other.
    /// </summary>
    // MenuAPI draws one from each edge and never measures either, so nothing stops them meeting in
    // the middle. Counting characters is an approximation, the font not being fixed width, so this
    // sits a little under what actually fits.
    private const int RowCharacters = 38;

    /// <summary>Below this there is too little display name left to be worth reading.</summary>
    private const int ShortestLabel = 6;

    private PedModelCategory[] _categories = [];

    public override async Task PrepareAsync()
    {
        await PedModelSync.WaitForFirstAsync();

        _categories = [.. PedModelSync.Categories];
    }

    protected override void Build(MenuBuilder menu)
    {
        // Never sorted. The order in the config file is the order here, so an owner can put their own
        // peds wherever they want them.
        foreach (var category in _categories)
        {
            // Copied out of the loop variable so each entry's callbacks capture its own category.
            var current = category;

            menu.Entries.Add(new SubmenuEntry
            {
                Text = MenuText.Literal(current.Name),
                Description = MenuText.Key(
                    Loc.PedModels.CategoryDescription,
                    ("category", MenuText.Literal(current.Name))),
                MenuTitle = MenuText.Literal(current.Name),
                MenuSubtitle = MenuText.Key(Loc.PedModels.CategorySubtitle),
                Gate = MenuGate.When(() => ClientPedPermissions.CanSpawnCategory(current.Name)),
                Build = categoryMenu => BuildCategoryMenu(categoryMenu, current),
            });
        }

        if (_categories.Length == 0)
        {
            // A row rather than an empty menu, so the reason is on screen instead of leaving the
            // player wondering whether something is broken.
            menu.Entries.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.PedModels.Empty),
                Description = MenuText.Key(Loc.PedModels.EmptyDescription),
            });
        }

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PedModels.SpawnByName),
            Description = MenuText.Key(Loc.PedModels.SpawnByNameDescription),
            Gate = PedModelsPermissions.SpawnByName,
            OnSelectedAsync = _ => SpawnByNameAsync(),
        });
    }

    private static void BuildCategoryMenu(MenuBuilder categoryMenu, PedModelCategory category)
    {
        var categoryName = category.Name;

        foreach (var ped in category.Peds)
        {
            var model = ped.Model;
            var label = ped.Label;

            categoryMenu.Entries.Add(new ButtonEntry
            {
                // The model name reads better than the name the list gives it, so it leads and the
                // other one sits on the right, giving way when there is no room for both.
                Text = MenuText.Literal(model),
                Label = RowLabel(model, label),
                Description = MenuText.Key(
                    Loc.PedModels.PedDescription,
                    ("model", MenuText.Literal(model)),
                    ("label", MenuText.Literal(label))),
                Gate = MenuGate.When(() => ClientPedPermissions.CanSpawnPed(model, categoryName)),
                OnSelectedAsync = _ => SpawnAsync(model, label, categoryName),
            });
        }
    }

    /// <summary>
    /// The display name, shortened to whatever the model name left behind, or dropped when that is
    /// nothing worth reading. Both names are in the description either way, so nothing is lost.
    /// </summary>
    private static MenuText RowLabel(string model, string label)
    {
        var room = RowCharacters - model.Length;

        if (room >= label.Length)
        {
            return MenuText.Literal(label);
        }

        return room < ShortestLabel
            ? MenuText.Empty
            : MenuText.Literal(label[..(room - 1)] + '…');
    }

    private async Task SpawnByNameAsync()
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.PedModels.SpawnByNamePrompt),
            maxLength: 30,
            suggestions: SpawnableSuggestions());

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var model = typed.Trim().ToLowerInvariant();

        if (!PedSpawning.IsSpawnable(model))
        {
            Notifications.Error(MenuText.Key(Loc.PedModels.SpawnByNameInvalid, ("model", MenuText.Literal(model))));
            return;
        }

        // A ped that is in the list answers to its category, and one that is not is only reachable
        // through this row, which has already been gated.
        var known = Find(model);

        if (known is { } found && !ClientPedPermissions.CanSpawnPed(found.Model, found.Category))
        {
            Notifications.Warning(MenuText.Key(Loc.PedModels.SpawnByNameDenied, ("model", MenuText.Literal(model))));
            return;
        }

        await SpawnAsync(model, known?.Label ?? model, known?.Category ?? string.Empty);
    }

    /// <summary>Built per opening: a permission refresh in between changes what belongs in it.</summary>
    private IReadOnlyList<InputSuggestion> SpawnableSuggestions() =>
        [.. _categories
            .SelectMany(category => category.Peds.Select(ped => (ped.Model, ped.Label, category.Name)))
            .Where(ped => ClientPedPermissions.CanSpawnPed(ped.Model, ped.Name))
            .Select(ped => new InputSuggestion
            {
                Value = ped.Model,
                Label = ped.Label,
                Detail = ped.Name,
            })];

    private (string Model, string Label, string Category)? Find(string model)
    {
        foreach (var category in _categories)
        {
            foreach (var ped in category.Peds)
            {
                if (string.Equals(ped.Model, model, StringComparison.OrdinalIgnoreCase))
                {
                    return (ped.Model, ped.Label, category.Name);
                }
            }
        }

        return null;
    }

    private static async Task SpawnAsync(string model, string label, string categoryName)
    {
        // Re-checked because a permission refresh can land between drawing and selecting. An empty
        // category means the ped came from the by name row, which has its own check.
        if (categoryName.Length > 0 && !ClientPedPermissions.CanSpawnPed(model, categoryName))
        {
            return;
        }

        if (!await PedSpawning.SetPlayerModelAsync(model))
        {
            Notifications.Error(MenuText.Key(Loc.PedModels.SpawnFailed, ("model", MenuText.Literal(model))));
            return;
        }

        Notifications.Success(MenuText.Key(Loc.PedModels.Spawned, ("ped", MenuText.Literal(label))));
    }
}

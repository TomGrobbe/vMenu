using System.Globalization;

using MenuAPI;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players;

internal sealed class PedCategoryFilter(PedModelCategory category)
{
    private const int QueryMaxLength = 30;

    private const int QueryDisplayLength = 20;

    private readonly PedModelCategory _category = category;

    private MenuBuilder? _menu;

    private string _query = string.Empty;

    private int _matches = category.Peds.Count;

    private bool _prompting;

    internal static string SearchText(string model, string label) =>
        (model + ' ' + label).ToLowerInvariant();

    internal void Attach(MenuBuilder menu)
    {
        _menu = menu;

        menu.InstructionalButtons.Add(new ButtonHint
        {
            Name = "pedfilter",
            Description = MenuText.Key(Loc.PedModels.FilterBinding),
            DefaultKey = "SPACE",
            DefaultButton = "R1_INDEX",
            ShadowedControl = Control.Jump,
            Text = MenuText.Key(Loc.PedModels.FilterButton),
            Handler = (_, _) => _ = PromptAsync(),
        });
    }

    internal string Subtitle()
    {
        var localizer = Localizer.Current;

        if (_query.Length == 0)
        {
            return localizer.Get(Loc.PedModels.CategorySubtitle);
        }

        return MenuText.Key(
            Loc.PedModels.CategorySubtitleFiltered,
            ("query", MenuText.Literal(Shorten(_query))),
            ("count", MenuText.Literal(Number(_matches))),
            ("total", MenuText.Literal(Number(_category.Peds.Count)))).Resolve(localizer);
    }

    private async Task PromptAsync()
    {
        if (_menu is not { } menu || _prompting)
        {
            return;
        }

        _prompting = true;

        try
        {
            var typed = await UserInput.GetTextAsync(
                MenuText.Key(Loc.PedModels.FilterPrompt, ("category", MenuText.Literal(_category.Name))),
                QueryMaxLength,
                _query,
                Suggestions());

            if (typed is not null)
            {
                Apply(menu, typed.Trim());
            }
        }
        finally
        {
            _prompting = false;
        }
    }

    private void Apply(MenuBuilder menu, string query)
    {
        var category = MenuText.Literal(_category.Name);

        if (query.Length == 0)
        {
            _query = string.Empty;
            _matches = _category.Peds.Count;

            menu.SetUserFilter(null);
            menu.Menu.MenuSubtitle = Subtitle();

            Notifications.Info(MenuText.Key(Loc.PedModels.FilterCleared, ("category", category)));

            return;
        }

        var needle = query.ToLowerInvariant();
        var matches = Count(menu, needle);

        if (matches == 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.PedModels.FilterNoMatches,
                ("category", category),
                ("query", MenuText.Literal(query))));

            return;
        }

        _query = query;
        _matches = matches;

        menu.SetUserFilter(item => Matches(item, needle));
        menu.Menu.MenuSubtitle = Subtitle();

        Notifications.Info(MenuText.Key(
            Loc.PedModels.FilterApplied,
            ("count", MenuText.Literal(Number(matches))),
            ("query", MenuText.Literal(query))));
    }

    private static int Count(MenuBuilder menu, string needle)
    {
        var matches = 0;

        foreach (var entry in menu.Entries)
        {
            if (entry.Item is { } item && Matches(item, needle))
            {
                matches++;
            }
        }

        return matches;
    }

    private static bool Matches(MenuItem item, string needle) =>
        item.ItemData is string text && text.Contains(needle);

    private IReadOnlyList<InputSuggestion> Suggestions()
    {
        var rows = new InputSuggestion[_category.Peds.Count];

        for (var index = 0; index < rows.Length; index++)
        {
            var ped = _category.Peds[index];

            rows[index] = new InputSuggestion
            {
                Value = ped.Model,
                Label = ped.Label,
                Detail = _category.Name,
            };
        }

        return rows;
    }

    private static string Shorten(string value) =>
        value.Length <= QueryDisplayLength ? value : value[..(QueryDisplayLength - 1)] + "…";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}

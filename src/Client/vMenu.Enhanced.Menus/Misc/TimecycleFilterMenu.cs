using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Misc;

internal sealed class TimecycleFilterMenu
{
    private const int QueryMaxLength = 40;

    private const int QueryDisplayLength = 20;

    private const int ButtonIntervalMs = 500;

    // A key mapping's icon only comes back from group 0.
    private const int ControlGroup = 0;

    // A pair of icons draws as nothing without this. A single one still works, which hides it.
    private const string IconSeparator = "%b_998%";

    private const int KeyboardGroup = 2;

    private readonly Dictionary<string, CheckboxEntry> _rows = new(StringComparer.OrdinalIgnoreCase);

    private MenuBuilder? _menu;

    private TickHandle? _buttons;

    private string _query = string.Empty;

    private int _matches = TimecycleModifiers.Names.Count;

    private bool _prompting;

    private bool _open;

    private bool _buttonsKeyboard;

    private bool _buttonsStale = true;

    private string? _highlighted;

    internal void Build(MenuBuilder menu)
    {
        _menu = menu;

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.TimecycleFilter),
            Description = MenuText.Key(Loc.DisplaySettings.TimecycleFilterDescription),
            OnSelectedAsync = _ => PromptAsync(),
        });

        foreach (var name in TimecycleModifiers.Names)
        {
            menu.Entries.Add(Row(name));
        }

        TimecycleKeyBindings.Register(
            () => Nudge(1),
            () => Nudge(-1),
            ClearAll,
            Search,
            BackToTop);

        _buttons = TickRegistry.Register(
            "Display.TimecycleButtons",
            SyncButtons,
            TickRate.Every(ButtonIntervalMs),
            () => _open,
            onStopped: ClearButtons,
            autoStart: false);

        menu.OnOpened = opened =>
        {
            _open = true;
            _buttonsStale = true;
            _highlighted = null;

            _buttons?.Reevaluate();

            opened.Menu.MenuSubtitle = Subtitle();
        };

        menu.OnClosed = _ =>
        {
            _open = false;

            _buttons?.Reevaluate();
        };

        menu.OnIndexChanged = changed =>
            _highlighted = changed.NewItem?.ItemData is string data ? Original(data) : null;

        TimecycleState.Changed += Repaint;

        TimecycleState.Changed += () => _buttonsStale = true;
    }

    internal string Subtitle()
    {
        var localizer = Localizer.Current;

        if (_query.Length == 0)
        {
            return localizer.Get(Loc.DisplaySettings.TimecycleSubtitle);
        }

        return MenuText.Key(
            Loc.DisplaySettings.TimecycleSubtitleFiltered,
            ("query", MenuText.Literal(Shorten(_query))),
            ("count", MenuText.Literal(Number(_matches))),
            ("total", MenuText.Literal(Number(TimecycleModifiers.Names.Count)))).Resolve(localizer);
    }

    private CheckboxEntry Row(string name)
    {
        var entry = new CheckboxEntry
        {
            Text = MenuText.Literal(name),
            Description = MenuText.Key(Loc.DisplaySettings.TimecycleRowDescription),

            ReadState = () => TimecycleState.IsActive(name),
            OnChanged = _ => TimecycleState.Toggle(name),

            Configure = item => item.ItemData = name.ToLowerInvariant(),
        };

        _rows[name] = entry;

        return entry;
    }

    // Pokes the items directly. A refresh here would re-resolve ~730 entries on every change.
    private void Repaint()
    {
        foreach (var pair in _rows)
        {
            if (pair.Value.Item is not { } item)
            {
                continue;
            }

            var active = TimecycleState.IsActive(pair.Key);

            if (item is MenuCheckboxItem checkbox)
            {
                checkbox.Checked = active;
            }

            item.Description = DescriptionFor(active);
        }
    }

    private static string DescriptionFor(bool active)
    {
        var localizer = Localizer.Current;

        if (!active)
        {
            return localizer.Get(Loc.DisplaySettings.TimecycleRowDescription);
        }

        return MenuText.Key(
            Loc.DisplaySettings.TimecycleRowActive,
            ("intensity", MenuText.Literal(Number(TimecycleState.Intensity))),
            ("max", MenuText.Literal(Number(TimecycleState.MaxIntensity)))).Resolve(localizer);
    }

    private void Nudge(int by)
    {
        API.RunOnMainThread(() =>
        {
            if (!_open || !Native.IsUsingKeyboardAndMouse(KeyboardGroup))
            {
                return;
            }

            TimecycleState.SetIntensity(TimecycleState.Intensity + by);
        });
    }

    private void Search()
    {
        API.RunOnMainThread(() =>
        {
            if (!_open || !Native.IsUsingKeyboardAndMouse(KeyboardGroup))
            {
                return;
            }

            _ = PromptAsync();
        });
    }

    private void BackToTop()
    {
        API.RunOnMainThread(() =>
        {
            if (!_open || !Native.IsUsingKeyboardAndMouse(KeyboardGroup) || _menu is not { } menu)
            {
                return;
            }

            menu.Menu.RefreshIndex(0, 0);
        });
    }

    private void ClearAll()
    {
        API.RunOnMainThread(() =>
        {
            if (!_open || !Native.IsUsingKeyboardAndMouse(KeyboardGroup))
            {
                return;
            }

            TimecycleState.ClearAll();
        });
    }

    private static string? Original(string data)
    {
        foreach (var name in TimecycleModifiers.Names)
        {
            if (string.Equals(name, data, StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
        }

        return null;
    }

    private void SyncButtons()
    {
        if (_menu is not { } menu)
        {
            return;
        }

        var keyboard = Native.IsUsingKeyboardAndMouse(KeyboardGroup);

        if (!_buttonsStale && keyboard == _buttonsKeyboard)
        {
            return;
        }

        _buttonsStale = false;
        _buttonsKeyboard = keyboard;

        menu.Menu.CustomInstructionalButtons.Clear();

        if (!keyboard)
        {
            return;
        }

        var localizer = Localizer.Current;

        menu.Menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(TimecycleKeyBindings.IntensityDownControl)
                + IconSeparator
                + Icon(TimecycleKeyBindings.IntensityUpControl),
            MenuText.Key(
                Loc.DisplaySettings.TimecycleIntensityButton,
                ("intensity", MenuText.Literal(Number(TimecycleState.Intensity))),
                ("max", MenuText.Literal(Number(TimecycleState.MaxIntensity)))).Resolve(localizer)));

        menu.Menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(TimecycleKeyBindings.ClearControl),
            localizer.Get(Loc.DisplaySettings.TimecycleClearButton)));

        menu.Menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(TimecycleKeyBindings.SearchControl),
            localizer.Get(Loc.DisplaySettings.TimecycleFilter)));

        menu.Menu.CustomInstructionalButtons.Add(new Menu.InstructionalButton(
            Icon(TimecycleKeyBindings.TopControl),
            localizer.Get(Loc.DisplaySettings.TimecycleTopButton)));
    }

    private void ClearButtons()
    {
        _menu?.Menu.CustomInstructionalButtons.Clear();

        _buttonsStale = true;
    }

    private static string Icon(int control) =>
        Native.GetControlInstructionalButton(ControlGroup, control, true);

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
                MenuText.Key(Loc.DisplaySettings.TimecycleFilterPrompt),
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
        if (query.Length == 0)
        {
            _query = string.Empty;
            _matches = TimecycleModifiers.Names.Count;

            menu.SetUserFilter(null);
            menu.Menu.MenuSubtitle = Subtitle();

            Notifications.Info(MenuText.Key(Loc.DisplaySettings.TimecycleFilterCleared));

            return;
        }

        var needle = query.ToLowerInvariant();
        var matches = Count(menu, needle);

        if (matches == 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.DisplaySettings.TimecycleFilterNoMatches,
                ("query", MenuText.Literal(query))));

            return;
        }

        _query = query;
        _matches = matches;

        menu.SetUserFilter(item => Matches(item, needle));
        menu.Menu.MenuSubtitle = Subtitle();

        Notifications.Info(MenuText.Key(
            Loc.DisplaySettings.TimecycleFilterApplied,
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
        item.ItemData is not string text || text.Contains(needle);

    private static IReadOnlyList<InputSuggestion> Suggestions()
    {
        var rows = new InputSuggestion[TimecycleModifiers.Names.Count];

        for (var index = 0; index < rows.Length; index++)
        {
            var name = TimecycleModifiers.Names[index];

            rows[index] = new InputSuggestion { Value = name, Label = name };
        }

        return rows;
    }

    private static string Shorten(string value) =>
        value.Length <= QueryDisplayLength ? value : value[..(QueryDisplayLength - 1)] + "…";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}

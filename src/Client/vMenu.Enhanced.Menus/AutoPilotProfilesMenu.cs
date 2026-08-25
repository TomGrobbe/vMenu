using System.Globalization;

using MenuAPI;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Menus.Vehicles.AutoPilot;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.AutoPilot.ProfilesTitle,
    SubtitleKey = Loc.AutoPilot.ProfilesSubtitle,
    DescriptionKey = Loc.AutoPilot.ProfilesLinkDescription,
    Permission = PlayerOptionsPermissions.AutoPilot)]
public sealed class AutoPilotProfilesMenu : MenuDefinition
{
    private const int NameLength = 40;

    private const int DescriptionLength = 100;

    private static readonly AutoPilotCategory[] Categories =
    [
        AutoPilotCategory.Vehicle,
        AutoPilotCategory.Plane,
        AutoPilotCategory.Boat,
        AutoPilotCategory.Helicopter,
    ];

    private DetachedMenu? _listMenu;

    private DetachedMenu? _editMenu;

    private AutoPilotCategory _category;

    private SavedDrivingProfile? _profile;

    private SavedDrivingProfileEntry? _entry;

    protected override void Build(MenuBuilder menu)
    {
        _listMenu = menu.AddDetachedMenu(
            MenuText.From(CategoryName),
            MenuText.Key(Loc.AutoPilot.ProfilesSubtitle),
            _ => { });

        _listMenu.Builder.OnOpened = _ => Refill(_listMenu, ListRows());

        _editMenu = menu.AddDetachedMenu(
            MenuText.From(() => _profile?.Name ?? string.Empty),
            MenuText.Key(Loc.AutoPilot.ProfilesSubtitle),
            _ => { },
            MenuGate.When(() => _profile is not null));

        _editMenu.Builder.OnOpened = _ => Refill(_editMenu, EditRows());

        menu.AddRange(RootRows());

        menu.OnOpened = _ => Refill(menu, RootRows());
    }

    private List<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>();

        foreach (var category in Categories)
        {
            var picked = category;
            var saved = DrivingProfileStore.InCategory(category).Count;
            var presets = AutoPilotPresets.For(category).Count;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(NameKey(category)),
                Description = MenuText.Key(
                    Loc.AutoPilot.CategoryDescription,
                    ("count", Num(saved + presets)),
                    ("presets", Num(presets))),
                Label = MenuText.Literal("→"),
                OnSelected = _ =>
                {
                    _category = picked;

                    _listMenu?.Open();
                },
            });
        }

        return rows;
    }

    private List<MenuEntry> ListRows()
    {
        var rows = new List<MenuEntry>
        {
            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.CreateProfile),
                Description = MenuText.Key(Loc.AutoPilot.CreateProfileDescription),
                OnSelectedAsync = _ => CreateAsync(),
            },
        };

        foreach (var preset in AutoPilotPresets.For(_category))
        {
            rows.Add(Row(preset, entry: null));
        }

        var saved = DrivingProfileStore.InCategory(_category);

        if (saved.Count == 0)
        {
            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.ProfilesEmpty),
                Description = MenuText.Key(Loc.AutoPilot.ProfilesEmptyDescription),
                ReadEnabled = () => false,
            });

            return rows;
        }

        foreach (var entry in saved)
        {
            rows.Add(Row(entry.Profile, entry));
        }

        return rows;
    }

    private ButtonEntry Row(SavedDrivingProfile profile, SavedDrivingProfileEntry? entry)
    {
        var preset = entry is null;

        return new ButtonEntry
        {
            Text = MenuText.Literal(profile.Name),
            Description = MenuText.Key(
                preset ? Loc.AutoPilot.ProfilePresetDescription : Loc.AutoPilot.ProfileRowDescription,
                ("description", MenuText.Literal(profile.Description)),
                ("value", Num(profile.Flags))),
            Label = MenuText.Literal(profile.Flags.ToString(CultureInfo.InvariantCulture)),
            ReadLeftIcon = () => AutoPilotDefaults.IsSelected(profile.Category, profile.Name)
                ? MenuItem.Icon.TICK
                : MenuItem.Icon.NONE,
            OnSelected = _ =>
            {
                _profile = profile;
                _entry = entry;

                _editMenu?.Open();
            },
        };
    }

    private List<MenuEntry> EditRows()
    {
        var rows = new List<MenuEntry>();

        if (_profile is not { } profile)
        {
            return rows;
        }

        rows.Add(new SeparatorEntry
        {
            Text = MenuText.Key(
                Loc.AutoPilot.ProfileValue,
                ("value", MenuText.From(() => (_profile?.Flags ?? 0).ToString(CultureInfo.InvariantCulture))),
                ("hex", MenuText.From(() => "0x" + (_profile?.Flags ?? 0).ToString("X8", CultureInfo.InvariantCulture)))),
            Description = MenuText.Key(Loc.AutoPilot.ProfileValueDescription),
            ShowArrows = false,
        });

        foreach (var flag in Flags(profile.Category))
        {
            rows.Add(FlagRow(flag));
        }

        rows.AddRange(Extras(profile.Category));

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ProfileUse),
            Description = MenuText.Key(Loc.AutoPilot.ProfileUseDescription),
            ReadEnabled = () => !AutoPilotDefaults.IsSelected(profile.Category, profile.Name),
            OnSelected = _ =>
            {
                AutoPilotDefaults.Select(profile.Category, profile.Name);

                Notifications.Success(MenuText.Key(Loc.AutoPilot.ProfileInUse, ("name", profile.Name)));

                Refresh();
            },
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ProfileDuplicate),
            Description = MenuText.Key(Loc.AutoPilot.ProfileDuplicateDescription),
            OnSelectedAsync = _ => DuplicateAsync(profile),
        });

        if (_entry is null)
        {
            return rows;
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ProfileRename),
            Description = MenuText.Key(Loc.AutoPilot.ProfileRenameDescription),
            LockedDescription = LockReason(),
            ReadEnabled = Editable,
            OnSelectedAsync = _ => RenameAsync(),
        });

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ProfileDelete),
            Description = MenuText.Key(Loc.AutoPilot.ProfileDeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.AutoPilot.ProfileDeleteConfirm, ("name", profile.Name)),
            ReadEnabled = Editable,
            OnConfirmed = _ => Delete(),
        });

        return rows;
    }

    private CheckboxEntry FlagRow(DrivingFlag flag)
    {
        return new CheckboxEntry
        {
            Text = MenuText.Literal(flag.Label),

            Description = MenuText.Key(Loc.AutoPilot.ProfileFlagDescription, ("flag", flag.Name)),
            LockedDescription = LockReason(),
            ReadEnabled = Editable,
            ReadState = () => _profile is { } profile && (profile.Flags & flag.Value) != 0,
            OnChanged = changed =>
            {
                if (_entry is not { } entry)
                {
                    return;
                }

                var before = entry.Profile.Flags;
                var after = changed.Checked ? before | flag.Value : before & ~flag.Value;

                if (!DrivingProfileStore.Write(entry, profile => profile.Flags = after, profile => profile.Flags = before))
                {
                    Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileRefused));
                }

                Refresh();
            },
        };
    }

    private List<MenuEntry> Extras(AutoPilotCategory category)
    {
        var rows = new List<MenuEntry>
        {
            Number(
                Loc.AutoPilot.ProfileCruiseSpeed,
                Loc.AutoPilot.ProfileCruiseSpeedDescription,
                () => (int)(_profile?.CruiseSpeed ?? 0f),
                value => value.CruiseSpeed,
                (profile, value) => profile.CruiseSpeed = value,
                step: 5,
                max: 150,
                zeroKey: Loc.AutoPilot.CruiseSpeedAuto),
        };

        if (category is not (AutoPilotCategory.Plane or AutoPilotCategory.Helicopter))
        {
            return rows;
        }

        rows.Add(Number(
            Loc.AutoPilot.ProfileFlightHeight,
            Loc.AutoPilot.ProfileFlightHeightDescription,
            () => _profile?.FlightHeight ?? 0,
            profile => profile.FlightHeight,
            (profile, value) => profile.FlightHeight = (int)value,
            step: 10,
            max: 1000,
            zeroKey: null));

        rows.Add(Number(
            Loc.AutoPilot.ProfileMinHeight,
            Loc.AutoPilot.ProfileMinHeightDescription,
            () => _profile?.MinHeightAboveTerrain ?? 0,
            profile => profile.MinHeightAboveTerrain,
            (profile, value) => profile.MinHeightAboveTerrain = (int)value,
            step: 5,
            max: 500,
            zeroKey: null));

        if (category is not AutoPilotCategory.Plane)
        {
            return rows;
        }

        rows.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ProfilePrecise),
            Description = MenuText.Key(Loc.AutoPilot.ProfilePreciseDescription),
            LockedDescription = LockReason(),
            ReadEnabled = Editable,
            ReadState = () => _profile?.Precise ?? false,
            OnChanged = changed =>
            {
                if (_entry is not { } entry)
                {
                    return;
                }

                var before = entry.Profile.Precise;

                if (!DrivingProfileStore.Write(entry, profile => profile.Precise = changed.Checked, profile => profile.Precise = before))
                {
                    Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileRefused));
                }

                Refresh();
            },
        });

        return rows;
    }

    private DynamicListEntry Number(
        string textKey,
        string descriptionKey,
        Func<int> read,
        Func<SavedDrivingProfile, float> current,
        Action<SavedDrivingProfile, float> apply,
        int step,
        int max,
        string? zeroKey)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            LockedDescription = LockReason(),
            ReadEnabled = Editable,
            ReadValue = () => Display(read(), zeroKey),
            Change = changing =>
            {
                if (_entry is not { } entry)
                {
                    return Display(read(), zeroKey);
                }

                var before = current(entry.Profile);
                var after = Math.Clamp(read() + (changing.Left ? -step : step), 0, max);

                if (!DrivingProfileStore.Write(entry, profile => apply(profile, after), profile => apply(profile, before)))
                {
                    Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileRefused));
                }

                Refresh();

                return Display(read(), zeroKey);
            },
        };
    }

    private static string Display(int value, string? zeroKey) =>
        value == 0 && zeroKey is not null
            ? Localizer.Current.Get(zeroKey)
            : value.ToString(CultureInfo.InvariantCulture);

    private async Task CreateAsync()
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.ProfileNamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.ProfileDescriptionPrompt), DescriptionLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (Reserved(_category, name))
        {
            return;
        }

        var created = new SavedDrivingProfile
        {
            Name = name,
            Description = answers[1].Trim(),
            Category = _category,
        };

        if (!Report(DrivingProfileStore.Save(created, replacing: false), name))
        {
            return;
        }

        if (DrivingProfileStore.Load(name) is { } stored)
        {
            _profile = stored.Profile;
            _entry = stored;

            _editMenu?.Open();
        }
    }

    private async Task DuplicateAsync(SavedDrivingProfile profile)
    {
        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.AutoPilot.ProfileDuplicatePrompt),
            NameLength,
            profile.Name);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        var name = typed.Trim();

        if (Reserved(profile.Category, name))
        {
            return;
        }

        var outcome = _entry is { } entry
            ? DrivingProfileStore.Duplicate(entry, name)
            : DrivingProfileStore.Save(
                new SavedDrivingProfile
                {
                    Name = name,
                    Description = profile.Description,
                    Category = profile.Category,
                    Flags = profile.Flags,
                    CruiseSpeed = profile.CruiseSpeed,
                    FlightHeight = profile.FlightHeight,
                    MinHeightAboveTerrain = profile.MinHeightAboveTerrain,
                    Precise = profile.Precise,
                },
                replacing: false);

        if (!Report(outcome, name))
        {
            return;
        }

        if (DrivingProfileStore.Load(name) is { } saved)
        {
            _profile = saved.Profile;
            _entry = saved;

            Rebuild();
        }
    }

    private async Task RenameAsync()
    {
        if (_entry is not { } entry)
        {
            return;
        }

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.ProfileRenamePrompt), NameLength, entry.Profile.Name),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.ProfileDescriptionPrompt), DescriptionLength, entry.Profile.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (Reserved(entry.Profile.Category, name))
        {
            return;
        }

        if (!DrivingProfileStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileNameTaken, ("name", name)));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.AutoPilot.ProfileRenamed, ("name", name)));

        Rebuild();
    }

    private void Delete()
    {
        if (_entry is not { } entry)
        {
            return;
        }

        var name = entry.Profile.Name;

        DrivingProfileStore.Delete(name);
        AutoPilotDefaults.Forget(entry.Profile.Category, name);

        _profile = null;
        _entry = null;

        Notifications.Success(MenuText.Key(Loc.AutoPilot.ProfileDeleted, ("name", name)));

        Rebuild();

        _editMenu?.Menu.GoBack();
    }

    private bool Report(SaveOutcome outcome, string name)
    {
        switch (outcome)
        {
            case SaveOutcome.Saved:
                Notifications.Success(MenuText.Key(Loc.AutoPilot.ProfileSaved, ("name", name)));

                Rebuild();

                return true;

            case SaveOutcome.NameTaken:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileNameTaken, ("name", name)));

                return false;

            default:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileRefused));

                return false;
        }
    }

    private void Refresh()
    {
        if (_listMenu is { } list)
        {
            MenuRegistry.Refresh(list.Menu);
        }

        if (_editMenu is { } edit)
        {
            MenuRegistry.Refresh(edit.Menu);
        }
    }

    private void Rebuild()
    {
        if (_listMenu is { } list)
        {
            Refill(list, ListRows());
        }

        if (_editMenu is { } edit)
        {
            Refill(edit, EditRows());
        }
    }

    private static bool Reserved(AutoPilotCategory category, string name)
    {
        if (!DrivingProfileStore.IsReserved(category, name))
        {
            return false;
        }

        Notifications.Error(MenuText.Key(Loc.AutoPilot.ProfileNameReserved, ("name", name)));

        return true;
    }

    private bool Editable() => _entry is { IsFromNewerBuild: false };

    private MenuText LockReason() =>
        MenuText.From(() => Localizer.Current.Get(
            _entry is null ? Loc.AutoPilot.ProfilePresetLocked : Loc.AutoPilot.ProfileNewerBuild));

    private string CategoryName() => Localizer.Current.Get(NameKey(_category));

    private static string NameKey(AutoPilotCategory category) => category switch
    {
        AutoPilotCategory.Plane => Loc.AutoPilot.CategoryPlane,
        AutoPilotCategory.Boat => Loc.AutoPilot.CategoryBoat,
        AutoPilotCategory.Helicopter => Loc.AutoPilot.CategoryHelicopter,
        _ => Loc.AutoPilot.CategoryVehicle,
    };

    private static IReadOnlyList<DrivingFlag> Flags(AutoPilotCategory category) => category switch
    {
        AutoPilotCategory.Boat => DrivingFlags.Boat,
        AutoPilotCategory.Helicopter => DrivingFlags.Heli,

        _ => DrivingFlags.Driving,
    };

    private static MenuText Num(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));

    private static void Refill(DetachedMenu menu, IReadOnlyList<MenuEntry> rows) => Refill(menu.Builder, rows);

    private static void Refill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }
}

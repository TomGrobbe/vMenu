using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>
/// Paint, inside and out.
/// </summary>
/// <remarks>
/// The game keeps the colour and the finish apart: the same id looks completely different as
/// metallic, matte or chrome. That is why the finish is its own row rather than a set of duplicate
/// colour groups, which is how legacy vMenu ended up listing "classic" and "metallic" over the same
/// table of colours.
/// </remarks>
internal static class ColorsSection
{
    /// <summary>How many steps the paint fade slider has. The native wants zero to one.</summary>
    private const int FadeSteps = 20;

    /// <summary>What the game wants for wheels painted the colour they came in.</summary>
    private const int DefaultAlloy = 156;

    /// <summary>The paint finish that makes a colour shift with the viewing angle.</summary>
    private const int ChameleonFinish = 6;

    private const int NormalFinish = 0;

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        menu.OnOpened = _ => SectionRows.Fill(menu, Rows());
    }

    private static List<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>
        {
            new SubmenuEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.PrimaryColor),
                Description = MenuText.Key(Loc.VehicleOptions.PrimaryColorDescription),
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.PrimaryColor),
                Build = BuildPrimary,
            },
            new SubmenuEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.SecondaryColor),
                Description = MenuText.Key(Loc.VehicleOptions.SecondaryColorDescription),
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.SecondaryColor),
                Build = BuildSecondary,
            },

            ColorRow(
                Loc.VehicleOptions.PearlescentColor,
                Loc.VehicleOptions.PearlescentColorDescription,
                VehicleColorTables.Classic,
                ReadPearlescent,
                SetPearlescent),

            WheelColorRow(),

            ColorRow(
                Loc.VehicleOptions.DashboardColor,
                Loc.VehicleOptions.DashboardColorDescription,
                VehicleColorTables.Classic,
                ReadDashboard,
                color => Apply(current => Native.SetVehicleDashboardColour(current, color))),

            ColorRow(
                Loc.VehicleOptions.InteriorColor,
                Loc.VehicleOptions.InteriorColorDescription,
                VehicleColorTables.Classic,
                ReadInterior,
                color => Apply(current => Native.SetVehicleInteriorColour(current, color))),
        };

        // Its own row rather than a sixth colour group, so it is findable and so picking one sets the
        // finish along with the colour.
        if (VehicleColorTables.HasChameleonPaints)
        {
            rows.Add(ChameleonRow());
        }

        if (Native.GetNumberOfVehicleColours(handle) is > 0 and var combinations)
        {
            rows.Add(CombinationRow(combinations));
        }

        rows.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.PaintFade),
            Description = MenuText.Key(Loc.VehicleOptions.PaintFadeDescription),
            Min = 0,
            Max = FadeSteps,
            ShowDivider = true,
            ReadPosition = () => SectionRows.Driven() is { } current
                ? Math.Clamp((int)Math.Round(Native.GetVehicleEnveffScale(current) * FadeSteps), 0, FadeSteps)
                : 0,
            OnMoved = moved => Apply(current =>
                Native.SetVehicleEnveffScale(current, (float)moved.NewPosition / FadeSteps)),
        });

        return rows;
    }

    #region Primary and secondary

    private static void BuildPrimary(MenuBuilder menu)
    {
        menu.Entries.Add(FinishRow(
            () => ReadPaintType(primary: true),
            finish => Apply(handle =>
            {
                Native.GetVehicleColours(handle, out var color, out _);
                Native.GetVehicleExtraColours(handle, out var pearlescent, out _);

                Native.SetVehicleModColor_1(handle, finish, color, pearlescent);
            })));

        foreach (var group in VehicleColorTables.BodyGroups)
        {
            menu.Entries.Add(ColorRow(
                group.NameKey,
                Loc.VehicleOptions.PrimaryColorDescription,
                group.Colors,
                ReadPrimary,
                SetPrimary));
        }

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.CustomColor),
            Description = MenuText.Key(Loc.VehicleOptions.CustomColorDescription),
            MenuSubtitle = MenuText.Key(Loc.VehicleOptions.PrimaryColor),
            Build = rgb => RgbPicker.Build(rgb, PrimaryRgb()),
        });
    }

    private static void BuildSecondary(MenuBuilder menu)
    {
        menu.Entries.Add(FinishRow(
            () => ReadPaintType(primary: false),
            finish => Apply(handle =>
            {
                Native.GetVehicleColours(handle, out _, out var color);

                Native.SetVehicleModColor_2(handle, finish, color);
            })));

        foreach (var group in VehicleColorTables.BodyGroups)
        {
            menu.Entries.Add(ColorRow(
                group.NameKey,
                Loc.VehicleOptions.SecondaryColorDescription,
                group.Colors,
                ReadSecondary,
                SetSecondary));
        }

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.CustomColor),
            Description = MenuText.Key(Loc.VehicleOptions.CustomColorDescription),
            MenuSubtitle = MenuText.Key(Loc.VehicleOptions.SecondaryColor),
            Build = rgb => RgbPicker.Build(rgb, SecondaryRgb()),
        });
    }

    private static ListEntry FinishRow(Func<int> read, Action<int> write)
    {
        var options = new List<MenuText>(VehicleOptionTables.PaintFinishKeys.Count);

        foreach (var key in VehicleOptionTables.PaintFinishKeys)
        {
            options.Add(MenuText.Key(key));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.PaintFinish),
            Description = MenuText.Key(Loc.VehicleOptions.PaintFinishDescription),
            Options = options,

            // Not clamped. A finish the game reports that this list does not have is a value vMenu
            // does not understand, and clamping it landed on the last entry, which is how every
            // vehicle came up reading as chameleon.
            ReadSelectedIndex = () => read() is var finish && finish >= 0 && finish < options.Count ? finish : 0,

            OnIndexChanged = changed => write(changed.NewIndex),
        };
    }

    /// <summary>
    /// The paints that shift colour with the angle you look at them from.
    /// </summary>
    /// <remarks>
    /// Both body colours and the finish move together, because that is what the game means by a
    /// chameleon paint. Picking an ordinary colour afterwards puts the finish back, which is what
    /// makes the two directions behave the same way.
    /// </remarks>
    private static ListEntry ChameleonRow()
    {
        var colors = VehicleColorTables.Chameleon;

        var options = new List<MenuText>(colors.Count);

        foreach (var color in colors)
        {
            options.Add(color.Text);
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.ColorGroupChameleon),
            Description = MenuText.Key(Loc.VehicleOptions.ChameleonPaintDescription),
            Options = options,
            ReadSelectedIndex = () => Math.Max(0, VehicleColorTables.IndexOf(colors, ReadPrimary())),
            OnIndexChanged = changed =>
            {
                if (changed.NewIndex >= 0 && changed.NewIndex < colors.Count)
                {
                    SetChameleon(colors[changed.NewIndex].Id);
                }
            },
        };
    }

    private static RgbTarget PrimaryRgb() => new()
    {
        Read = () =>
        {
            if (SectionRows.Driven() is not { } handle || !Native.GetIsVehiclePrimaryColourCustom(handle))
            {
                return null;
            }

            Native.GetVehicleCustomPrimaryColour(handle, out var red, out var green, out var blue);

            return new RgbValue(red, green, blue);
        },
        Write = (red, green, blue) => Apply(handle => Native.SetVehicleCustomPrimaryColour(handle, red, green, blue)),
        Clear = () => Apply(Native.ClearVehicleCustomPrimaryColour),
    };

    private static RgbTarget SecondaryRgb() => new()
    {
        Read = () =>
        {
            if (SectionRows.Driven() is not { } handle || !Native.GetIsVehicleSecondaryColourCustom(handle))
            {
                return null;
            }

            Native.GetVehicleCustomSecondaryColour(handle, out var red, out var green, out var blue);

            return new RgbValue(red, green, blue);
        },
        Write = (red, green, blue) => Apply(handle => Native.SetVehicleCustomSecondaryColour(handle, red, green, blue)),
        Clear = () => Apply(Native.ClearVehicleCustomSecondaryColour),
    };

    #endregion

    #region Rows built from a colour table

    private static ListEntry ColorRow(
        string textKey,
        string descriptionKey,
        IReadOnlyList<VehicleColorOption> colors,
        Func<int> readCurrent,
        Action<int> write)
    {
        var options = new List<MenuText>(colors.Count);

        foreach (var color in colors)
        {
            options.Add(color.Text);
        }

        return new ListEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            Options = options,

            // A colour from another group, or one mixed by hand, is in none of these lists, so the
            // row rests on its first entry rather than jumping somewhere that means nothing.
            ReadSelectedIndex = () => Math.Max(0, VehicleColorTables.IndexOf(colors, readCurrent())),
            OnIndexChanged = changed =>
            {
                if (changed.NewIndex >= 0 && changed.NewIndex < colors.Count)
                {
                    write(colors[changed.NewIndex].Id);
                }
            },
        };
    }

    private static ListEntry WheelColorRow()
    {
        // The default is not in any table, so it is prepended and everything else shifts by one.
        var options = new List<MenuText>(VehicleColorTables.Classic.Count + 1)
        {
            MenuText.Key(Loc.VehicleOptions.ColorDefaultAlloy),
        };

        foreach (var color in VehicleColorTables.Classic)
        {
            options.Add(color.Text);
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.WheelColor),
            Description = MenuText.Key(Loc.VehicleOptions.WheelColorDescription),
            Options = options,
            ReadSelectedIndex = () =>
            {
                var current = ReadWheelColor();

                return current == DefaultAlloy
                    ? 0
                    : Math.Max(0, VehicleColorTables.IndexOf(VehicleColorTables.Classic, current) + 1);
            },
            OnIndexChanged = changed =>
            {
                var color = changed.NewIndex == 0
                    ? DefaultAlloy
                    : VehicleColorTables.Classic[changed.NewIndex - 1].Id;

                SetWheelColor(color);
            },
        };
    }

    private static ListEntry CombinationRow(int combinations)
    {
        var options = new List<MenuText>(combinations);

        for (var index = 0; index < combinations; index++)
        {
            var number = (index + 1).ToString(CultureInfo.InvariantCulture);

            options.Add(MenuText.Key(
                Loc.VehicleOptions.PresetCombinationValue,
                ("number", MenuText.Literal(number))));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.PresetCombination),
            Description = MenuText.Key(Loc.VehicleOptions.PresetCombinationDescription),
            Options = options,
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? Math.Clamp(Native.GetVehicleColourCombination(handle), 0, combinations - 1)
                : 0,
            OnIndexChanged = changed => Apply(handle => Native.SetVehicleColourCombination(handle, changed.NewIndex)),
        };
    }

    #endregion

    #region Reading and writing

    private static int ReadPaintType(bool primary)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        try
        {
            if (primary)
            {
                Native.GetVehicleModColor_1(handle, out var type, out _, out _);

                return type;
            }

            Native.GetVehicleModColor_2(handle, out var secondaryType, out _);

            return secondaryType;
        }
        catch (Exception)
        {
            // The generated wrapper reads every output slot whether or not the game filled it, and
            // the runtime throws rather than handing back a zero when it did not.
            return 0;
        }
    }

    private static int ReadPrimary()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleColours(handle, out var primary, out _);

        return primary;
    }

    private static int ReadSecondary()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleColours(handle, out _, out var secondary);

        return secondary;
    }

    private static int ReadPearlescent()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleExtraColours(handle, out var pearlescent, out _);

        return pearlescent;
    }

    private static int ReadWheelColor()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return DefaultAlloy;
        }

        Native.GetVehicleExtraColours(handle, out _, out var wheel);

        return wheel;
    }

    private static int ReadDashboard()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleDashboardColour(handle, out var dashboard);

        return dashboard;
    }

    private static int ReadInterior()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleInteriorColour(handle, out var interior);

        return interior;
    }

    // A custom colour sits on top of the id, so picking from a list without dropping it would look
    // like the row did nothing at all. A chameleon finish goes the same way: leaving it on while
    // painting the car an ordinary colour is what made coming back out of chameleon behave
    // differently from going into it.
    private static void SetPrimary(int color) => Apply(handle =>
    {
        Native.ClearVehicleCustomPrimaryColour(handle);
        Native.GetVehicleExtraColours(handle, out var pearlescent, out _);
        Native.SetVehicleModColor_1(handle, Ordinary(ReadPaintType(primary: true)), color, pearlescent);

        Native.GetVehicleColours(handle, out _, out var secondary);
        Native.SetVehicleColours(handle, color, secondary);
    });

    /// <inheritdoc cref="SetPrimary"/>
    private static void SetSecondary(int color) => Apply(handle =>
    {
        Native.ClearVehicleCustomSecondaryColour(handle);
        Native.SetVehicleModColor_2(handle, Ordinary(ReadPaintType(primary: false)), color);

        Native.GetVehicleColours(handle, out var primary, out _);
        Native.SetVehicleColours(handle, primary, color);
    });

    /// <summary>A chameleon paint covers the whole car, so both colours and both finishes move.</summary>
    private static void SetChameleon(int color) => Apply(handle =>
    {
        Native.ClearVehicleCustomPrimaryColour(handle);
        Native.ClearVehicleCustomSecondaryColour(handle);

        Native.GetVehicleExtraColours(handle, out var pearlescent, out _);

        Native.SetVehicleModColor_1(handle, ChameleonFinish, color, pearlescent);
        Native.SetVehicleModColor_2(handle, ChameleonFinish, color);
        Native.SetVehicleColours(handle, color, color);
    });

    /// <summary>The finish to keep when painting an ordinary colour, which chameleon never is.</summary>
    private static int Ordinary(int finish) => finish == ChameleonFinish ? NormalFinish : finish;

    private static void SetPearlescent(int color) => Apply(handle =>
    {
        Native.GetVehicleExtraColours(handle, out _, out var wheel);
        Native.SetVehicleExtraColours(handle, color, wheel);
    });

    private static void SetWheelColor(int color) => Apply(handle =>
    {
        Native.GetVehicleExtraColours(handle, out var pearlescent, out _);
        Native.SetVehicleExtraColours(handle, pearlescent, color);
    });

    private static void Apply(Action<int> change)
    {
        if (SectionRows.DrivenWithModKit() is { } handle)
        {
            change(handle);
        }
    }

    #endregion
}

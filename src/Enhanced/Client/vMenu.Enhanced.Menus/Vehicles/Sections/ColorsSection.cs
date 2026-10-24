using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class ColorsSection
{
    // How many steps the paint fade slider has. The native wants zero to one.
    private const int FadeSteps = 20;

    private sealed class Swatch(MenuText text, Action<int> apply, Func<PaintSnapshot, bool> isCurrent)
    {
        public MenuText Text { get; } = text;

        public Action<int> Apply { get; } = apply;

        public Func<PaintSnapshot, bool> IsCurrent { get; } = isCurrent;
    }

    private sealed class PaintKind(ModPaintType type, string gameLabel, string fallbackKey)
    {
        public ModPaintType Type { get; } = type;

        public string GameLabel { get; } = gameLabel;

        public string FallbackKey { get; } = fallbackKey;
    }

    // The mod shop's order.
    private static readonly PaintKind[] Kinds =
    [
        new(ModPaintType.Chrome, "CMOD_COL1_0", Loc.VehicleOptions.FinishChrome),
        new(ModPaintType.Classic, "CMOD_COL1_1", Loc.VehicleOptions.ColorGroupClassic),
        new(ModPaintType.Matte, "CMOD_COL1_5", Loc.VehicleOptions.ColorGroupMatte),
        new(ModPaintType.Metallic, "CMOD_COL1_3", Loc.VehicleOptions.FinishMetallic),
        new(ModPaintType.Metals, "CMOD_COL1_4", Loc.VehicleOptions.ColorGroupMetal),
        new(ModPaintType.Pearlescent, "CMOD_COL1_6", Loc.VehicleOptions.FinishPearlescent),
        new(ModPaintType.Chameleon, "CMOD_COL1_7", Loc.VehicleOptions.FinishChameleon),
    ];

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        SectionRows.AutoFill(menu, Rows);
    }

    private static List<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>
        {
            new SubmenuEntry(Loc.VehicleOptions.PrimaryColor)
            {
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.PrimaryColor),
                Build = menu => BuildSlot(menu, primary: true),
            },
            new SubmenuEntry(Loc.VehicleOptions.SecondaryColor)
            {
                MenuSubtitle = MenuText.Key(Loc.VehicleOptions.SecondaryColor),
                Build = menu => BuildSlot(menu, primary: false),
            },

            SwatchLink(
                MenuText.Key(Loc.VehicleOptions.InteriorColor),
                MenuText.Key(Loc.VehicleOptions.InteriorColorDescription),
                FixedSwatches(
                    VehicleColorTables.Interior,
                    (vehicle, color) => Native.SetVehicleInteriorColour(vehicle, color),
                    snapshot => snapshot.Interior)),

            SwatchLink(
                MenuText.Key(Loc.VehicleOptions.DashboardColor),
                MenuText.Key(Loc.VehicleOptions.DashboardColorDescription),
                FixedSwatches(
                    VehicleColorTables.Dashboard,
                    (vehicle, color) => Native.SetVehicleDashboardColour(vehicle, color),
                    snapshot => snapshot.Dashboard)),

            SwatchLink(
                MenuText.Key(Loc.VehicleOptions.WheelColor),
                MenuText.Key(Loc.VehicleOptions.WheelColorDescription),
                FixedSwatches(
                    VehicleColorTables.Wheel,
                    (vehicle, color) =>
                    {
                        Native.GetVehicleExtraColours(vehicle, out var pearl, out _);
                        Native.SetVehicleExtraColours(vehicle, pearl, color);
                    },
                    snapshot => snapshot.Wheel)),
        };

        if (Native.GetNumberOfVehicleColours(handle) is > 0 and var combinations)
        {
            rows.Add(CombinationRow(combinations));
        }

        rows.Add(new SliderEntry(Loc.VehicleOptions.PaintFade, 0, FadeSteps)
        {
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

    private static void BuildSlot(MenuBuilder menu, bool primary)
    {
        menu.AddRange(SlotRows(primary));

        SectionRows.AutoFill(menu, () => SlotRows(primary));
    }

    private static List<MenuEntry> SlotRows(bool primary)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var description = MenuText.Key(primary
            ? Loc.VehicleOptions.PrimaryColorDescription
            : Loc.VehicleOptions.SecondaryColorDescription);

        var rows = new List<MenuEntry>();

        foreach (var kind in Kinds)
        {
            if (kind.Type == ModPaintType.Pearlescent && !primary)
            {
                continue;
            }

            var paints = ModPaintCatalog.Load(
                handle,
                kind.Type == ModPaintType.Pearlescent ? ModPaintType.Metallic : kind.Type);

            if (paints.Count == 0)
            {
                continue;
            }

            var title = GameLabels.Game(kind.GameLabel, kind.FallbackKey);

            if (kind.Type == ModPaintType.Pearlescent)
            {
                rows.Add(SwatchLink(
                    title,
                    MenuText.Key(Loc.VehicleOptions.PearlescentColorDescription),
                    PearlSwatches(paints)));
                continue;
            }

            rows.Add(SwatchLink(title, description, ModSwatches(paints, kind.Type, primary)));
        }

        rows.Add(SwatchLink(
            MenuText.Key(Loc.VehicleOptions.ColorGroupUtility),
            description,
            BodySwatches(VehicleColorTables.Utility, primary)));

        rows.Add(SwatchLink(
            MenuText.Key(Loc.VehicleOptions.ColorGroupWorn),
            description,
            BodySwatches(VehicleColorTables.Worn, primary)));

        rows.Add(new SubmenuEntry(Loc.VehicleOptions.CustomColor)
        {
            MenuSubtitle = MenuText.Key(primary ? Loc.VehicleOptions.PrimaryColor : Loc.VehicleOptions.SecondaryColor),
            Build = rgb => RgbPicker.Build(rgb, primary ? PrimaryRgb() : SecondaryRgb()),
        });

        return rows;
    }

    private static List<Swatch> ModSwatches(IReadOnlyList<ModPaint> paints, ModPaintType type, bool primary)
    {
        var swatches = new List<Swatch>(paints.Count);

        foreach (var paint in paints)
        {
            var text = GameLabels.GameOrLiteral(paint.Label, VehicleColorTables.FallbackName(paint.Label));

            swatches.Add(primary
                ? new Swatch(
                    text,
                    vehicle =>
                    {
                        Native.ClearVehicleCustomPrimaryColour(vehicle);
                        Native.SetVehicleModColor_1(vehicle, (int)type, paint.Index, 0);
                        Redraw(vehicle);
                        PaintPreview.Committed?.RestoreCustomSecondary();
                    },
                    snapshot => snapshot.CustomPrimary is null
                        && snapshot.Primary == paint.Paint
                        && SameType(snapshot.PrimaryType, type))
                : new Swatch(
                    text,
                    vehicle =>
                    {
                        Native.ClearVehicleCustomSecondaryColour(vehicle);
                        Native.SetVehicleModColor_2(vehicle, (int)type, paint.Index);
                        Redraw(vehicle);
                        PaintPreview.Committed?.RestoreCustomPrimary();
                    },
                    snapshot => snapshot.CustomSecondary is null
                        && snapshot.Secondary == paint.Paint
                        && SameType(snapshot.SecondaryType, type)));
        }

        return swatches;
    }

    // Uses the metallic list, the pearl being the colour that metallic paint would have given the body.
    private static List<Swatch> PearlSwatches(IReadOnlyList<ModPaint> metallic)
    {
        var swatches = new List<Swatch>(metallic.Count);

        foreach (var paint in metallic)
        {
            swatches.Add(new Swatch(
                GameLabels.GameOrLiteral(paint.Label, VehicleColorTables.FallbackName(paint.Label)),
                vehicle =>
                {
                    Native.GetVehicleExtraColours(vehicle, out _, out var wheel);
                    Native.SetVehicleExtraColours(vehicle, paint.Paint, wheel);
                },
                snapshot => snapshot.Pearl == paint.Paint));
        }

        return swatches;
    }

    private static List<Swatch> BodySwatches(IReadOnlyList<VehicleColorOption> colors, bool primary)
    {
        var swatches = new List<Swatch>(colors.Count);

        foreach (var color in colors)
        {
            swatches.Add(primary
                ? new Swatch(
                    color.Text,
                    vehicle =>
                    {
                        Native.ClearVehicleCustomPrimaryColour(vehicle);
                        Native.GetVehicleColours(vehicle, out _, out var secondary);
                        Native.SetVehicleColours(vehicle, color.Id, secondary);
                        PaintPreview.Committed?.RestoreCustomSecondary();
                    },
                    snapshot => snapshot.CustomPrimary is null && snapshot.Primary == color.Id)
                : new Swatch(
                    color.Text,
                    vehicle =>
                    {
                        Native.ClearVehicleCustomSecondaryColour(vehicle);
                        Native.GetVehicleColours(vehicle, out var primaryColor, out _);
                        Native.SetVehicleColours(vehicle, primaryColor, color.Id);
                        PaintPreview.Committed?.RestoreCustomPrimary();
                    },
                    snapshot => snapshot.CustomSecondary is null && snapshot.Secondary == color.Id));
        }

        return swatches;
    }

    private static List<Swatch> FixedSwatches(
        IReadOnlyList<VehicleColorOption> colors,
        Action<int, int> write,
        Func<PaintSnapshot, int> read)
    {
        var swatches = new List<Swatch>(colors.Count);

        foreach (var color in colors)
        {
            swatches.Add(new Swatch(
                color.Text,
                vehicle => write(vehicle, color.Id),
                snapshot => read(snapshot) == color.Id));
        }

        return swatches;
    }

    // A paint that predates the mod shop has no type, and the game matches it on colour alone.
    private static bool SameType(int current, ModPaintType type) =>
        current == (int)type || current is < 0 or > (int)ModPaintType.Chameleon;

    // The mod colour natives only record the shop paint, the car is not redrawn until its colours are set.
    private static void Redraw(int vehicle)
    {
        Native.GetVehicleColours(vehicle, out var primary, out var secondary);
        Native.GetVehicleExtraColours(vehicle, out var pearl, out var wheel);
        Native.SetVehicleColours(vehicle, primary, secondary);
        Native.SetVehicleExtraColours(vehicle, pearl, wheel);
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

    #region Swatch menus

    private static SubmenuEntry SwatchLink(
        MenuText text,
        MenuText description,
        IReadOnlyList<Swatch> swatches,
        Func<bool>? enabled = null) => new()
    {
        Text = text,
        Description = description,
        MenuSubtitle = text,
        ReadEnabled = enabled,
        Build = menu => BuildSwatches(menu, swatches),
    };

    // Scrolling shows a paint, enter keeps it, and leaving puts back whatever was kept, like the mod shop.
    private static void BuildSwatches(MenuBuilder menu, IReadOnlyList<Swatch> swatches)
    {
        var entries = new List<ButtonEntry>(swatches.Count);

        foreach (var swatch in swatches)
        {
            entries.Add(new ButtonEntry
            {
                Text = swatch.Text,
                ReadRightIcon = () => PaintPreview.CurrentIcon(swatch.IsCurrent),
                OnHighlighted = _ => PaintPreview.Show(swatch.Apply),
                OnSelected = _ =>
                {
                    PaintPreview.Commit(swatch.Apply);
                    menu.Refresh();
                },
            });
        }

        menu.AddRange(entries);

        // MenuAPI raises the first highlight before the open callback, so the session may already be running.
        menu.OnOpened = _ =>
        {
            ShopText.Load();
            PaintPreview.Begin();
            menu.Refresh();

            for (var index = 0; index < swatches.Count; index++)
            {
                if (PaintPreview.Committed is { } session && swatches[index].IsCurrent(session))
                {
                    menu.Select(entries[index]);

                    return;
                }
            }
        };

        menu.OnClosed = _ =>
        {
            PaintPreview.End();
            ShopText.Unload();
        };
    }

    #endregion

    #region Rows built from a colour table

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

        return new ListEntry(Loc.VehicleOptions.PresetCombination, options)
        {
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? Math.Clamp(Native.GetVehicleColourCombination(handle), 0, combinations - 1)
                : 0,
            OnIndexChanged = changed => Apply(handle => Native.SetVehicleColourCombination(handle, changed.NewIndex)),
        };
    }

    #endregion

    private static void Apply(Action<int> change)
    {
        if (SectionRows.DrivenWithModKit() is { } handle)
        {
            change(handle);
        }
    }
}

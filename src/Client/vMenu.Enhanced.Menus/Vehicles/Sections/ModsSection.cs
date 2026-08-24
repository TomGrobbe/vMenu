using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

// The slot list is asked of the game rather than hard coded, so an add-on vehicle with slots the
// base game never had shows them without vMenu knowing anything about it.
internal static class ModsSection
{
    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        menu.Menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(
            Control.Jump,
            Menu.ControlPressCheckType.JUST_PRESSED,
            (_, _) => DoorsSection.ToggleAll(),
            true));

        menu.InstructionalButtons.Add((Control.Jump, MenuText.Key(Loc.VehicleOptions.ToggleDoorsButton)));

        SectionRows.AutoFill(menu, Rows);
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>();

        foreach (var slot in VehicleModSlots.Available(handle, includeWheelSlots: false))
        {
            rows.Add(SlotRow(handle, slot));
        }

        rows.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.Turbo),
            Description = MenuText.Key(
                Loc.VehicleOptions.TurboDescription,
                ("number", VehicleModLabels.SlotNumber(VehicleModSlot.Turbo))),
            ReadState = () => Toggled(VehicleModSlot.Turbo),
            OnChanged = changed => Toggle(VehicleModSlot.Turbo, changed.Checked),
        });

        rows.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.TyreSmoke),
            Description = MenuText.Key(
                Loc.VehicleOptions.TyreSmokeDescription,
                ("number", VehicleModLabels.SlotNumber(VehicleModSlot.TyreSmoke))),
            ReadState = TyreSmokeFitted,
            OnChanged = changed => SetTyreSmoke(changed.Checked),
        });

        rows.Add(TyreSmokeColorRow());
        rows.Add(WindowTintRow());

        return rows;
    }

    // The stock part first, then everything else the game offers.
    private static ListEntry SlotRow(int handle, VehicleModSlot slot)
    {
        var count = Native.GetNumVehicleMods(handle, (int)slot);

        var options = new List<MenuText>(count + 1)
        {
            VehicleModLabels.StockName(handle, slot, count),
        };

        for (var index = 0; index < count; index++)
        {
            options.Add(VehicleModLabels.ModName(handle, slot, index, count));
        }

        var description = VehicleModLabels.SlotDescription(
            handle,
            slot,
            VehicleModLabels.BareSlotName(handle, slot),
            () => Position(slot));

        return new ListEntry
        {
            Text = SlotName(handle, slot),
            Description = description,
            Options = options,

            // Stock is -1 to the game and the first row here, so everything is shifted by one.
            ReadSelectedIndex = () => SectionRows.Driven() is { } current
                ? Native.GetVehicleMod(current, (int)slot) + 1
                : 0,

            // On scroll rather than on enter, so the part appears on the vehicle as it is picked, which is the
            // only way to see what you are choosing.
            OnIndexChanged = changed =>
            {
                Fit(slot, changed.NewIndex - 1);

                // The description carries which option is showing, and a description is only rewritten on a refresh,
                // so scrolling would leave it a step behind.
                changed.Item.Description = description.Resolve(Localizer.Current);
            },
        };
    }

    private static string Position(VehicleModSlot slot) =>
        SectionRows.DrivenWithModKit() is { } handle
            ? VehicleModLabels.Position(handle, slot)
            : string.Empty;

    private static ListEntry TyreSmokeColorRow()
    {
        var options = new List<MenuText>(VehicleSmokeColors.All.Count);

        foreach (var color in VehicleSmokeColors.All)
        {
            options.Add(MenuText.Key(color.NameKey));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.TyreSmokeColor),
            Description = MenuText.Key(Loc.VehicleOptions.TyreSmokeColorDescription),
            Options = options,
            ReadSelectedIndex = ReadTyreSmokeIndex,
            OnIndexChanged = changed => PaintSmoke(changed.NewIndex),
        };
    }

    private static ListEntry WindowTintRow()
    {
        var options = new List<MenuText>(VehicleOptionTables.WindowTints.Count);

        foreach (var tint in VehicleOptionTables.WindowTints)
        {
            options.Add(tint.Text);
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.WindowTint),
            Description = MenuText.Key(Loc.VehicleOptions.WindowTintDescription),
            Options = options,
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? VehicleOptionTables.IndexOfValue(VehicleOptionTables.WindowTints, Native.GetVehicleWindowTint(handle))
                : 0,
            OnIndexChanged = changed =>
            {
                if (SectionRows.Driven() is not { } handle
                    || changed.NewIndex < 0
                    || changed.NewIndex >= VehicleOptionTables.WindowTints.Count)
                {
                    return;
                }

                Native.SetVehicleWindowTint(handle, VehicleOptionTables.WindowTints[changed.NewIndex].Value);
            },
        };
    }

    private static int ReadTyreSmokeIndex()
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return 0;
        }

        Native.GetVehicleTyreSmokeColor(handle, out var red, out var green, out var blue);

        var index = VehicleSmokeColors.IndexOfRgb(red, green, blue);

        // A colour mixed by hand, or the white the game uses to mean no smoke at all, matches nothing in the
        // list, so the row rests on the first entry.
        return index < 0 ? 0 : index;
    }

    private static void PaintSmoke(int index)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle || VehicleSmokeColors.At(index) is not { } color)
        {
            return;
        }

        Native.SetVehicleTyreSmokeColor(handle, color.Red, color.Green, color.Blue);
    }

    private static bool TyreSmokeFitted()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return false;
        }

        if (!Native.IsToggleModOn(handle, (int)VehicleModSlot.TyreSmoke))
        {
            return false;
        }

        // White is how the game says there is no smoke, so a kit left on that colour is not fitted as far as
        // the player can tell, and the tick has to agree with what they can see.
        Native.GetVehicleTyreSmokeColor(handle, out var red, out var green, out var blue);

        return !VehicleSmokeColors.IsOff(red, green, blue);
    }

    private static void SetTyreSmoke(bool fitted)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return;
        }

        if (!fitted)
        {
            // All three, in this order. Toggling the mod off on its own leaves the smoke showing, and the game
            // only really lets go of it once the colour is back to white and the mod is gone.
            Native.SetVehicleTyreSmokeColor(
                handle,
                VehicleSmokeColors.OffRed,
                VehicleSmokeColors.OffGreen,
                VehicleSmokeColors.OffBlue);

            Native.ToggleVehicleMod(handle, (int)VehicleModSlot.TyreSmoke, false);
            Native.RemoveVehicleMod(handle, (int)VehicleModSlot.TyreSmoke);

            return;
        }

        Native.ToggleVehicleMod(handle, (int)VehicleModSlot.TyreSmoke, true);

        // Switching it on while the colour is still white would fit a kit that shows nothing, so the colour
        // the list is resting on is applied with it.
        Native.GetVehicleTyreSmokeColor(handle, out var red, out var green, out var blue);

        if (VehicleSmokeColors.IsOff(red, green, blue))
        {
            PaintSmoke(ReadTyreSmokeIndex());
        }
    }

    internal static MenuText SlotName(int handle, VehicleModSlot slot) =>
        VehicleModLabels.SlotName(handle, slot);

    private static void Fit(VehicleModSlot slot, int value)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return;
        }

        // Carried over rather than defaulted, since fitting a spoiler must not silently swap the tyres back
        // to the standard ones.
        var customTyres = Native.GetVehicleModVariation(handle, (int)VehicleModSlot.Wheels) != 0;

        Native.SetVehicleMod(handle, (int)slot, value, customTyres);
    }

    private static bool Toggled(VehicleModSlot slot) =>
        SectionRows.DrivenWithModKit() is { } handle && Native.IsToggleModOn(handle, (int)slot);

    private static void Toggle(VehicleModSlot slot, bool on)
    {
        if (SectionRows.DrivenWithModKit() is { } handle)
        {
            Native.ToggleVehicleMod(handle, (int)slot, on);
        }
    }
}

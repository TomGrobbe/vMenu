using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Appearance;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

/// <summary>Wheel family, rims, and what the tyres are made of.</summary>
internal static class WheelsSection
{
    /// <summary>Low grip tyres arrived in this game build.</summary>
    private const int DriftTyresBuild = 2372;

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        SectionRows.AutoFill(menu, Rows);
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        var rows = new List<MenuEntry>();
        var model = Native.GetEntityModel(handle);

        if (HasChangeableWheels(model))
        {
            // Declared before the rows above it, which have to be able to put it straight after
            // changing something that takes the custom tyres off with it. Assigned in two steps so
            // its own handler can reach it, since a set of rims with no custom tyre to go with them
            // refuses the change and the tick has to answer to the vehicle rather than to the press.
            CheckboxEntry? customTyres = null;

            customTyres = new CheckboxEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.CustomTyres),
                Description = MenuText.Key(Loc.VehicleOptions.CustomTyresDescription),
                ReadState = ReadCustomTyres,
                OnChanged = changed =>
                {
                    SetCustomTyres(changed.Checked);

                    Sync(customTyres!);
                },
            };

            rows.Add(WheelTypeRow(customTyres));
            rows.Add(RimRow(handle, VehicleModSlot.Wheels, customTyres));

            if (Native.IsThisModelABike(model))
            {
                rows.Add(RimRow(handle, VehicleModSlot.RearWheels, customTyres));
            }

            rows.Add(customTyres);
        }

        rows.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.BulletproofTyres),
            Description = MenuText.Key(Loc.VehicleOptions.BulletproofTyresDescription),
            ReadState = () => SectionRows.Driven() is { } current && !Native.GetVehicleTyresCanBurst(current),
            OnChanged = changed =>
            {
                if (SectionRows.Driven() is { } current)
                {
                    Native.SetVehicleTyresCanBurst(current, !changed.Checked);
                }
            },
        });

        if (Native.GetGameBuildNumber() >= DriftTyresBuild)
        {
            rows.Add(new CheckboxEntry
            {
                Text = MenuText.Key(Loc.VehicleOptions.DriftTyres),
                Description = MenuText.Key(Loc.VehicleOptions.DriftTyresDescription),
                ReadState = () => SectionRows.Driven() is { } current && Native.GetDriftTyresEnabled(current),
                OnChanged = changed => SetDriftTyres(changed.Checked),
            });
        }

        return rows;
    }

    private static ListEntry WheelTypeRow(CheckboxEntry customTyres)
    {
        var options = new List<MenuText>(VehicleOptionTables.WheelTypeKeys.Count);

        foreach (var key in VehicleOptionTables.WheelTypeKeys)
        {
            options.Add(MenuText.Key(key));
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.WheelType),
            Description = MenuText.Key(Loc.VehicleOptions.WheelTypeDescription),
            Options = options,
            ReadSelectedIndex = () => SectionRows.Driven() is { } handle
                ? Math.Clamp(Native.GetVehicleWheelType(handle), 0, options.Count - 1)
                : 0,

            // Applied as it is scrolled, like every other list in these menus. The rim row below is a
            // dynamic list precisely so it can follow this without the section being rebuilt.
            OnIndexChanged = changed => SetWheelType(changed.NewIndex, customTyres),
        };
    }

    private static void SetWheelType(int wheelType, CheckboxEntry customTyres)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return;
        }

        var custom = ReadCustomTyres();

        Native.SetVehicleWheelType(handle, wheelType);

        // The rims that were on it belong to the family that just went, so they go back to stock.
        Native.SetVehicleMod(handle, (int)VehicleModSlot.Wheels, -1, custom);

        if (Native.IsThisModelABike(Native.GetEntityModel(handle)))
        {
            Native.SetVehicleMod(handle, (int)VehicleModSlot.RearWheels, -1, custom);
        }

        Sync(customTyres);
    }

    /// <summary>
    /// Puts the custom tyres tick back in step with the vehicle.
    /// </summary>
    // Not every set of rims has a custom tyre to go with it, so fitting one can take the tyres off
    // whatever was asked for. A tick box only re-reads itself when the menu is refreshed, and the rim
    // row deliberately does not refresh anything, so it is told directly instead.
    private static void Sync(CheckboxEntry customTyres)
    {
        if (customTyres.Typed is { } item)
        {
            item.Checked = ReadCustomTyres();
        }
    }

    /// <summary>
    /// The rims, as a value worked out on demand rather than a fixed list.
    /// </summary>
    /// <remarks>
    /// A plain list would have to be thrown away and rebuilt every time the wheel family changed,
    /// because each family offers a different number of rims. This asks the game for the count each
    /// time it moves instead, so the row is always right without the menu being rebuilt underneath
    /// the player.
    /// </remarks>
    private static DynamicListEntry RimRow(int handle, VehicleModSlot slot, CheckboxEntry customTyres)
    {
        var description = VehicleModLabels.SlotDescription(
            handle,
            slot,
            VehicleModLabels.BareSlotName(handle, slot),
            () => Position(slot));

        return new DynamicListEntry
        {
            Text = ModsSection.SlotName(handle, slot),
            Description = description,
            ReadValue = () => RimText(slot),
            Change = changing =>
            {
                var value = Shift(slot, changing.Left, customTyres);

                // The description carries which rim of how many is on, and a description is only
                // rewritten on a refresh, so scrolling would leave it a step behind.
                changing.Item.Description = description.Resolve(Localizer.Current);

                return value;
            },
        };
    }

    private static string Shift(VehicleModSlot slot, bool left, CheckboxEntry customTyres)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return RimText(slot);
        }

        var count = Native.GetNumVehicleMods(handle, (int)slot);

        if (count <= 0)
        {
            return RimText(slot);
        }

        // Positions run from stock through every rim, so there is one more than the game counts.
        var positions = count + 1;
        var position = Native.GetVehicleMod(handle, (int)slot) + 1;

        position = (position + (left ? -1 : 1) + positions) % positions;

        Native.SetVehicleMod(handle, (int)slot, position - 1, ReadCustomTyres());

        Sync(customTyres);

        return RimText(slot);
    }

    private static string RimText(VehicleModSlot slot)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return string.Empty;
        }

        var localizer = Localizer.Current;

        var count = Native.GetNumVehicleMods(handle, (int)slot);
        var fitted = Native.GetVehicleMod(handle, (int)slot);

        return fitted < 0
            ? VehicleModLabels.StockName(handle, slot, count).Resolve(localizer)
            : VehicleModLabels.ModName(handle, slot, fitted, count).Resolve(localizer);
    }

    private static string Position(VehicleModSlot slot) =>
        SectionRows.DrivenWithModKit() is { } handle
            ? VehicleModLabels.Position(handle, slot)
            : string.Empty;

    /// <summary>Anything without rims to change: boats, aircraft, trains and pushbikes.</summary>
    private static bool HasChangeableWheels(uint model) =>
        !Native.IsThisModelABoat(model)
        && !Native.IsThisModelAHeli(model)
        && !Native.IsThisModelAPlane(model)
        && !Native.IsThisModelABicycle(model)
        && !Native.IsThisModelATrain(model);

    private static bool ReadCustomTyres() =>
        SectionRows.DrivenWithModKit() is { } handle
        && Native.GetVehicleModVariation(handle, (int)VehicleModSlot.Wheels) != 0;

    // The tyres are a property of the rims rather than a slot of their own, so switching them means
    // refitting the rims that are already on with the flag flipped.
    private static void SetCustomTyres(bool custom)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return;
        }

        Native.SetVehicleMod(
            handle,
            (int)VehicleModSlot.Wheels,
            Native.GetVehicleMod(handle, (int)VehicleModSlot.Wheels),
            custom);

        if (Native.IsThisModelABike(Native.GetEntityModel(handle)))
        {
            Native.SetVehicleMod(
                handle,
                (int)VehicleModSlot.RearWheels,
                Native.GetVehicleMod(handle, (int)VehicleModSlot.RearWheels),
                custom);
        }
    }

    // Only vehicles built for them will take drift tyres, and the game says nothing when one will
    // not. Asking it afterwards is the only way to tell the player rather than leave them wondering.
    private static void SetDriftTyres(bool enabled)
    {
        if (SectionRows.DrivenWithModKit() is not { } handle)
        {
            return;
        }

        Native.SetDriftTyresEnabled(handle, enabled);

        if (Native.GetDriftTyresEnabled(handle) != enabled)
        {
            Notifications.Warning(MenuText.Key(Loc.VehicleOptions.DriftTyresUnsupported));
        }
    }
}

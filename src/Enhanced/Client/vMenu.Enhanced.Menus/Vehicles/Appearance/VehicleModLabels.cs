using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

// Follows the game's own mod shop, in the same order. Where GetModTextLabel answers nothing the shop
// falls back to keys it builds itself, reproduced here and checked against the game before use.
// Names resolve when a row is drawn, because the game only answers once the mod kit has streamed in.
public static class VehicleModLabels
{
    private const int Stock = -1;

    public static bool HasGameName(int handle, VehicleModSlot slot) =>
        Native.DoesEntityExist(handle) && GameLabels.Exists(Native.GetModSlotName(handle, (int)slot));

    // The performance slots are the same thing on every vehicle: eleven is always the engine. Naming
    // them is not the guess that naming, say, slot twenty seven is.
    public static bool IsFixedMeaning(VehicleModSlot slot) => slot
        is VehicleModSlot.Engine
        or VehicleModSlot.Brakes
        or VehicleModSlot.Transmission
        or VehicleModSlot.Suspension
        or VehicleModSlot.Armour
        or VehicleModSlot.Horn
        or VehicleModSlot.Turbo;

    // A slot the game will not name is usually one an add-on vehicle reused, so asserting it is
    // "Interior 1" would be worse than admitting the guess. The description explains the star.
    public static MenuText SlotName(int handle, VehicleModSlot slot) => MenuText.From(() =>
    {
        var bare = ResolveSlotName(handle, slot);

        return IsNamed(handle, slot)
            ? bare
            : Substitute(Loc.VehicleOptions.ModSlotUnnamed, ("name", bare));
    });

    // The star belongs on the row's own name. Repeating it on every value would read as part of the part.
    public static MenuText BareSlotName(int handle, VehicleModSlot slot) =>
        MenuText.From(() => ResolveSlotName(handle, slot));

    // The game names this one better than vMenu can: "No Armor" says more than "Stock Armour" does.
    public static MenuText StockName(int handle, VehicleModSlot slot, int count) => MenuText.From(() =>
    {
        if (FallbackKey(handle, slot, Stock, count) is { } key && GameLabels.Exists(key))
        {
            return GameLabels.Text(key, string.Empty);
        }

        return Substitute(Loc.VehicleOptions.ModStock, ("slot", ResolveSlotName(handle, slot)));
    });

    // Useful to anyone building an add-on vehicle, and the one thing that stays true when the game has
    // no name for a slot, or has reused it for something the name no longer describes.
    public static MenuText SlotNumber(VehicleModSlot slot) =>
        MenuText.Literal(((int)slot).ToString(CultureInfo.InvariantCulture));

    public static MenuText SlotDescription(int handle, VehicleModSlot slot, MenuText slotName, Func<string> position) =>
        MenuText.Key(
            IsNamed(handle, slot)
                ? Loc.VehicleOptions.ModSlotDescription
                : Loc.VehicleOptions.ModSlotGuessedDescription,
            ("slot", slotName),
            ("number", SlotNumber(slot)),
            ("position", MenuText.From(position)));

    // Counted with the stock part as the first option, because that is how the lists present them.
    public static string Position(int handle, VehicleModSlot slot)
    {
        var count = Native.GetNumVehicleMods(handle, (int)slot);
        var fitted = Native.GetVehicleMod(handle, (int)slot);

        var current = (fitted + 2).ToString(CultureInfo.InvariantCulture);
        var total = (count + 1).ToString(CultureInfo.InvariantCulture);

        return current + "/" + total;
    }

    public static MenuText ModName(int handle, VehicleModSlot slot, int index, int count) =>
        MenuText.From(() => ResolveModName(handle, slot, index, count));

    // Public so the label dump reports the same answer the menus use rather than a second guess.
    public static string? NameKey(int handle, VehicleModSlot slot, int index, int count)
    {
        if (!Native.DoesEntityExist(handle))
        {
            return null;
        }

        // Horns first, like the shop does, because GetModTextLabel answers nothing for them.
        if (slot is VehicleModSlot.Horn)
        {
            return Existing(VehicleHornLabels.TextKey(handle, index)) ?? FallbackKey(handle, slot, index, count);
        }

        return Existing(Native.GetModTextLabel(handle, (int)slot, index))
            ?? FallbackKey(handle, slot, index, count);
    }

    private static bool IsNamed(int handle, VehicleModSlot slot) =>
        HasGameName(handle, slot) || IsFixedMeaning(slot);

    private static string ResolveSlotName(int handle, VehicleModSlot slot)
    {
        var fallback = VehicleModSlots.TechnicalName(slot);

        if (!Native.DoesEntityExist(handle))
        {
            return fallback;
        }

        return GameLabels.Text(Native.GetModSlotName(handle, (int)slot), fallback);
    }

    private static string ResolveModName(int handle, VehicleModSlot slot, int index, int count)
    {
        if (NameKey(handle, slot, index, count) is { } key)
        {
            return GameLabels.Text(key, string.Empty);
        }

        return Substitute(
            Loc.VehicleOptions.ModNumbered,
            ("slot", ResolveSlotName(handle, slot)),
            ("number", (index + 1).ToString(CultureInfo.InvariantCulture)));
    }

    // The shop counts its options from one with the stock part at zero, so these run one ahead of the
    // mod index. The engine runs two ahead, its list carrying a plain "Tune Engine" before the levels.
    private static string? FallbackKey(int handle, VehicleModSlot slot, int index, int count)
    {
        var option = index + 1;

        var key = slot switch
        {
            VehicleModSlot.Armour => "CMOD_ARM_" + Number(option),
            VehicleModSlot.Brakes => "CMOD_BRA_" + Number(option),
            VehicleModSlot.Transmission => "CMOD_GBX_" + Number(option),
            VehicleModSlot.Suspension => "CMOD_SUS_" + Number(option),
            VehicleModSlot.Engine => EngineKey(index, count),
            VehicleModSlot.Turbo => index < 0 ? "CMOD_TUR_0" : "CMOD_TUR_1",
            VehicleModSlot.Horn => index < 0 ? VehicleHornLabels.StockKey : null,
            VehicleModSlot.Wheels or VehicleModSlot.RearWheels => BikeWheelKey(handle, index),
            _ => null,
        };

        return Existing(key);
    }

    private static string EngineKey(int index, int count)
    {
        if (index < 0)
        {
            return "CMOD_ENG_0";
        }

        // A vehicle with a single engine upgrade gets the shop's plain "Tune Engine" rather than a level,
        // which is why this one cannot be written as an offset.
        return count < 2 ? "CMOD_ENG_1" : "CMOD_ENG_" + Number(index + 2);
    }

    // The two ranges and their offsets are the shop's, copied as they stand, and checked against the
    // game afterwards, so a build without them falls through to a numbered name.
    private static string? BikeWheelKey(int handle, int index)
    {
        if (index < 26 || !Native.IsThisModelABike(Native.GetEntityModel(handle)))
        {
            return null;
        }

        return index < 49
            ? "BIKEW_" + Number(index - 12)
            : "RWD_BIKEW_" + Number(index - 35);
    }

    private static string? Existing(string? key) => GameLabels.Exists(key ?? string.Empty) ? key : null;

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // These resolve inside a deferred callback, which hands back a plain string, so the translation has
    // to be filled in here rather than left to the framework.
    private static string Substitute(string key, params (string Name, string Value)[] arguments)
    {
        var resolved = new (string Name, MenuText Value)[arguments.Length];

        for (var index = 0; index < arguments.Length; index++)
        {
            resolved[index] = (arguments[index].Name, MenuText.Literal(arguments[index].Value));
        }

        return MenuText.Key(key, resolved).Resolve(Localizer.Current);
    }
}

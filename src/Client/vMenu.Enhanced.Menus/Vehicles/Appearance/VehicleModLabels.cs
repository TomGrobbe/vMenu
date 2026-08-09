using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

/// <summary>
/// What to call an upgrade slot and the parts in it.
/// </summary>
/// <remarks>
/// This follows what the game's own mod shop does, in the same order. For most parts
/// <c>GetModTextLabel</c> answers with a text key and that is the end of it. Where it answers with
/// nothing, the shop falls back to keys it builds itself, and those fallbacks are reproduced here:
/// the performance upgrades from a per slot prefix and a number, the horns from the identifier hash
/// of the sound they play, and a bike's later rims from their own key. Every key is checked against
/// the game before it is used, so anything that does not exist on a given build simply falls through
/// to a numbered name.
///
/// <para>
/// Every name is worked out when the row is drawn rather than when it is built. The game only
/// answers once the vehicle's mod kit has streamed in, so resolving at build time can freeze a
/// numbered fallback onto a part the game was about to be able to name.
/// </para>
/// </remarks>
public static class VehicleModLabels
{
    /// <summary>The index that means the part the vehicle left the factory with.</summary>
    private const int Stock = -1;

    /// <summary>Whether the game has a name of its own for a slot on this vehicle.</summary>
    public static bool HasGameName(int handle, VehicleModSlot slot) =>
        Native.DoesEntityExist(handle) && GameLabels.Exists(Native.GetModSlotName(handle, (int)slot));

    /// <summary>
    /// Whether vMenu's own name for a slot can be trusted even when the game offers none.
    /// </summary>
    // The performance slots are the same thing on every vehicle in the game: eleven is always the
    // engine, twelve always the brakes. Nobody reuses them for a spoiler, so naming them is not the
    // guess that naming, say, slot twenty seven is.
    public static bool IsFixedMeaning(VehicleModSlot slot) => slot
        is VehicleModSlot.Engine
        or VehicleModSlot.Brakes
        or VehicleModSlot.Transmission
        or VehicleModSlot.Suspension
        or VehicleModSlot.Armour
        or VehicleModSlot.Horn
        or VehicleModSlot.Turbo;

    /// <summary>
    /// The game's name for a slot, or vMenu's marked with a star where that is a guess.
    /// </summary>
    // A slot the game will not name is usually one an add-on vehicle has reused for something else
    // entirely, so asserting it is "Interior 1" when the vehicle has no such thing would be worse
    // than admitting the guess. The star says so in one character; the row's description explains it.
    public static MenuText SlotName(int handle, VehicleModSlot slot) => MenuText.From(() =>
    {
        var bare = ResolveSlotName(handle, slot);

        return IsNamed(handle, slot)
            ? bare
            : Substitute(Loc.VehicleOptions.ModSlotUnnamed, ("name", bare));
    });

    /// <summary>
    /// The slot's name without the star, for the values inside the row rather than the row itself.
    /// </summary>
    // The star belongs on the row's own name, where it warns that the name is a guess. Repeating it
    // on every value in the list would say the same thing over and over and read as part of the part.
    public static MenuText BareSlotName(int handle, VehicleModSlot slot) =>
        MenuText.From(() => ResolveSlotName(handle, slot));

    /// <summary>
    /// The first entry in a slot's list: the part the vehicle left the factory with.
    /// </summary>
    // The game names this one too, and rather better than vMenu can: "No Armor" and "None" say more
    // than "Stock Armour" does. Its own wording is only used where the game has none.
    public static MenuText StockName(int handle, VehicleModSlot slot, int count) => MenuText.From(() =>
    {
        if (FallbackKey(handle, slot, Stock, count) is { } key && GameLabels.Exists(key))
        {
            return GameLabels.Text(key, string.Empty);
        }

        return Substitute(Loc.VehicleOptions.ModStock, ("slot", ResolveSlotName(handle, slot)));
    });

    /// <summary>
    /// The slot's number, which every row carrying a mod puts in its description.
    /// </summary>
    // Useful to anyone building an add-on vehicle, and harmless to everyone else. It is also the one
    // thing that stays true when the game has no name for a slot, or has reused it for something the
    // name no longer describes.
    public static MenuText SlotNumber(VehicleModSlot slot) =>
        MenuText.Literal(((int)slot).ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// The description shown on a row that picks a part for a slot.
    /// </summary>
    /// <remarks>
    /// A slot vMenu had to name itself gets a different one, explaining what the star in front of the
    /// name means. Anywhere it appears, the slot number is the part that is always true.
    /// </remarks>
    /// <param name="position">Where in the slot's list the row is, resolved when the row is drawn.</param>
    public static MenuText SlotDescription(int handle, VehicleModSlot slot, MenuText slotName, Func<string> position) =>
        MenuText.Key(
            IsNamed(handle, slot)
                ? Loc.VehicleOptions.ModSlotDescription
                : Loc.VehicleOptions.ModSlotGuessedDescription,
            ("slot", slotName),
            ("number", SlotNumber(slot)),
            ("position", MenuText.From(position)));

    /// <summary>Which of a slot's options is fitted, out of how many there are.</summary>
    // Counted with the stock part as the first option, because that is how the lists present them.
    public static string Position(int handle, VehicleModSlot slot)
    {
        var count = Native.GetNumVehicleMods(handle, (int)slot);
        var fitted = Native.GetVehicleMod(handle, (int)slot);

        var current = (fitted + 2).ToString(CultureInfo.InvariantCulture);
        var total = (count + 1).ToString(CultureInfo.InvariantCulture);

        return current + "/" + total;
    }

    /// <summary>The game's name for one part in a slot, or a numbered fallback.</summary>
    /// <param name="count">How many parts the slot offers, which some of the game's own keys need.</param>
    public static MenuText ModName(int handle, VehicleModSlot slot, int index, int count) =>
        MenuText.From(() => ResolveModName(handle, slot, index, count));

    /// <summary>
    /// The key the game would name a part under, or null when nothing vMenu knows of names it.
    /// </summary>
    // Public so the label dump reports the same answer the menus use, rather than a second guess that
    // might disagree with them.
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

    /// <summary>
    /// The key the mod shop builds itself when the vehicle's artist supplied none.
    /// </summary>
    /// <remarks>
    /// The shop counts its menu options from one, with the stock part at zero, so every one of these
    /// runs one ahead of the mod index. The engine runs two ahead, its list carrying both a stock
    /// entry and a plain "Tune Engine" before the levels start, unless the vehicle has only a single
    /// engine upgrade, in which case that plain entry is the one used.
    /// </remarks>
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

        // A vehicle with a single engine upgrade gets the shop's plain "Tune Engine" rather than a
        // level, which is why this one cannot be written as an offset.
        return count < 2 ? "CMOD_ENG_1" : "CMOD_ENG_" + Number(index + 2);
    }

    /// <summary>
    /// A bike's later rims, which the game keeps under their own keys rather than with the rest.
    /// </summary>
    // The two ranges and their offsets are the shop's, copied as they stand. Both are checked against
    // the game afterwards, so a build without them falls through to a numbered name.
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

    // These resolve inside a deferred callback, which hands back a plain string, so the translation
    // has to be filled in here rather than left to the framework.
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

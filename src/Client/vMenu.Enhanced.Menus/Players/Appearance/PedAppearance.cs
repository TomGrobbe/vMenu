namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>One of a ped's body slots and what is drawn in it.</summary>
// A list of these rather than a dictionary keyed on the slot: a dictionary needs an equality
// comparer the client sandbox will not hand out, and a list serialises in a stable order.
public sealed class PedComponentValue
{
    /// <summary>The game's component id, 0 to 11.</summary>
    // Written out rather than implied by position, so covering a slot vMenu does not show today is
    // adding a value, not reinterpreting every value already stored.
    public int Slot { get; set; }

    public int Drawable { get; set; }

    public int Texture { get; set; }

    /// <summary>Which of the model's colour palettes the texture is drawn from. Almost always zero.</summary>
    // Read from the ped rather than assumed. Legacy edited with palette 0 and restored with palette
    // 1, so a saved ped could come back a different colour from the one that was saved.
    public int Palette { get; set; }
}

/// <summary>One prop the ped is wearing.</summary>
/// <remarks>
/// A slot with nothing in it is simply absent from the list. That is why nothing here is nullable
/// and nothing is -1: legacy stored -1 for "no prop" in both the drawable and the texture and had to
/// test for it at every use, which is also how it ended up writing 21 props when the game has five.
/// </remarks>
public sealed class PedPropValue
{
    /// <summary>The game's prop id: 0 hats, 1 glasses, 2 ears, 6 watches, 7 bracelets.</summary>
    public int Slot { get; set; }

    public int Drawable { get; set; }

    public int Texture { get; set; }
}

/// <summary>
/// Everything about a ped that a player can change through this menu and vMenu can put back.
/// </summary>
/// <remarks>
/// This is the shape written into a saved ped, so adding or removing anything here is a change to
/// the stored format and needs <c>SavedPed.SchemaVersion</c> raised with it.
///
/// <para>
/// Deliberately left out: everything the freemode character creator owns, meaning the head blend,
/// face features, overlays, hair colour and tattoos. Nothing that reaches this class is a freemode
/// ped, because saving one is refused until that creator exists.
/// </para>
/// </remarks>
// A plain class with settable properties. Not a record, because the generated equality reaches for
// EqualityComparer<T>.Default and the client sandbox refuses to load it.
public sealed class PedAppearance
{
    /// <summary>Best effort at save time. Empty when the model is not one this client can name.</summary>
    // The game has no reverse lookup for a ped model hash, so this is for the menu to read and for a
    // dump to be legible. Nothing spawns from it, the hash is what is used.
    public string ModelName { get; set; } = string.Empty;

    public uint ModelHash { get; set; }

    public List<PedComponentValue> Components { get; set; } = [];

    public List<PedPropValue> Props { get; set; } = [];

    /// <summary>What is in a component slot, or null when nothing was recorded for it.</summary>
    public PedComponentValue? ComponentAt(int slot)
    {
        foreach (var component in Components)
        {
            if (component.Slot == slot)
            {
                return component;
            }
        }

        return null;
    }

    /// <summary>What is worn in a prop slot, or null when the slot is empty.</summary>
    public PedPropValue? PropAt(int slot)
    {
        foreach (var prop in Props)
        {
            if (prop.Slot == slot)
            {
                return prop;
            }
        }

        return null;
    }
}

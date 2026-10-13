using vMenu.Enhanced.Data.Appearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// A class with settable properties rather than a record: generated equality reaches for
// EqualityComparer<T>.Default, which the client sandbox refuses to load.
public sealed class PedAppearance : PedOutfit
{
    // Best effort at save time, empty when the model is not one this client can name. The game has no
    // reverse lookup for a ped model hash, so this is for the menu to read and for a dump to be legible.
    // Nothing spawns from it, the hash is what is used.
    public string ModelName { get; set; } = string.Empty;

    public uint ModelHash { get; set; }
}

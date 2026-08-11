using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// The five things a ped can wear on top of its clothes.
/// </summary>
/// <remarks>
/// The ids are not contiguous: 3, 4 and 5 are unused, and so is everything above 7.
/// </remarks>
// A table rather than legacy's `tmpProp > 2 ? tmpProp + 3 : tmpProp`, which turned a menu row number
// into a prop id in five separate places and had to be undone in five more.
public static class PedPropSlots
{
    public const int Hats = 0;

    public const int Glasses = 1;

    public const int Ears = 2;

    public const int Watches = 6;

    public const int Bracelets = 7;

    public static readonly int[] All = [Hats, Glasses, Ears, Watches, Bracelets];

    /// <summary>What the menu calls this slot.</summary>
    public static string NameKey(int slot) => slot switch
    {
        Hats => Loc.PlayerAppearance.PropHats,
        Glasses => Loc.PlayerAppearance.PropGlasses,
        Ears => Loc.PlayerAppearance.PropEars,
        Watches => Loc.PlayerAppearance.PropWatches,
        _ => Loc.PlayerAppearance.PropBracelets,
    };

    /// <summary>The same name in plain English, for console output that is never translated.</summary>
    public static string TechnicalName(int slot) => slot switch
    {
        Hats => "hats",
        Glasses => "glasses",
        Ears => "ears",
        Watches => "watches",
        Bracelets => "bracelets",
        _ => "unknown",
    };
}

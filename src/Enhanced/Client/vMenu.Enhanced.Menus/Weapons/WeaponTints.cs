using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Weapons;

// The colours a weapon can be painted. Ordinary weapons have eight, Mk II weapons have their own set
// of thirty two, and which of the two a weapon uses is asked of the game rather than guessed from
// its name.
internal static class WeaponTints
{
    private static readonly string[] Standard =
    [
        Loc.WeaponOptions.TintBlack,
        Loc.WeaponOptions.TintGreen,
        Loc.WeaponOptions.TintGold,
        Loc.WeaponOptions.TintPink,
        Loc.WeaponOptions.TintArmy,
        Loc.WeaponOptions.TintLspd,
        Loc.WeaponOptions.TintOrange,
        Loc.WeaponOptions.TintPlatinum,
    ];

    private static readonly string[] MkII =
    [
        Loc.WeaponOptions.TintClassicBlack,
        Loc.WeaponOptions.TintClassicGray,
        Loc.WeaponOptions.TintClassicTwoTone,
        Loc.WeaponOptions.TintClassicWhite,
        Loc.WeaponOptions.TintClassicBeige,
        Loc.WeaponOptions.TintClassicGreen,
        Loc.WeaponOptions.TintClassicBlue,
        Loc.WeaponOptions.TintClassicEarth,
        Loc.WeaponOptions.TintClassicBrownBlack,
        Loc.WeaponOptions.TintRedContrast,
        Loc.WeaponOptions.TintBlueContrast,
        Loc.WeaponOptions.TintYellowContrast,
        Loc.WeaponOptions.TintOrangeContrast,
        Loc.WeaponOptions.TintBoldPink,
        Loc.WeaponOptions.TintBoldPurpleYellow,
        Loc.WeaponOptions.TintBoldOrange,
        Loc.WeaponOptions.TintBoldGreenPurple,
        Loc.WeaponOptions.TintBoldRedFeatures,
        Loc.WeaponOptions.TintBoldGreenFeatures,
        Loc.WeaponOptions.TintBoldCyanFeatures,
        Loc.WeaponOptions.TintBoldYellowFeatures,
        Loc.WeaponOptions.TintBoldRedWhite,
        Loc.WeaponOptions.TintBoldBlueWhite,
        Loc.WeaponOptions.TintMetallicGold,
        Loc.WeaponOptions.TintMetallicPlatinum,
        Loc.WeaponOptions.TintMetallicGrayLilac,
        Loc.WeaponOptions.TintMetallicPurpleLime,
        Loc.WeaponOptions.TintMetallicRed,
        Loc.WeaponOptions.TintMetallicGreen,
        Loc.WeaponOptions.TintMetallicBlue,
        Loc.WeaponOptions.TintMetallicWhiteAqua,
        Loc.WeaponOptions.TintMetallicRedYellow,
    ];

    // One option per tint the weapon actually has. A weapon reporting a count we have no names for still
    // gets a full list, the extras numbered, so nothing is silently unreachable.
    internal static IReadOnlyList<MenuText> Options(int count)
    {
        var names = count > Standard.Length ? MkII : Standard;
        var options = new List<MenuText>(count);

        for (var index = 0; index < count; index++)
        {
            options.Add(index < names.Length
                ? MenuText.Key(names[index])
                : MenuText.Literal((index + 1).ToString()));
        }

        return options;
    }
}

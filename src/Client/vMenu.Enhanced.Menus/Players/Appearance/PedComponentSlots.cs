using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// The twelve body slots a ped is dressed out of.
/// </summary>
/// <remarks>
/// There is no thirteenth. Legacy walked 0 to 20 when saving a ped and stored nine slots the game
/// answers zero variations for, then wrote a component variation for each of them on restore.
/// </remarks>
public static class PedComponentSlots
{
    public const int Head = 0;

    public const int Mask = 1;

    public const int Hair = 2;

    public const int Torso = 3;

    public const int Legs = 4;

    public const int Bags = 5;

    public const int Shoes = 6;

    public const int Neck = 7;

    public const int Undershirt = 8;

    public const int Armour = 9;

    public const int Decals = 10;

    public const int Tops = 11;

    public static readonly int[] All = [Head, Mask, Hair, Torso, Legs, Bags, Shoes, Neck, Undershirt, Armour, Decals, Tops];

    public static readonly int[] Clothing =
        [Mask, Torso, Legs, Bags, Shoes, Neck, Undershirt, Armour, Decals, Tops];

    /// <summary>What the menu calls this slot.</summary>
    public static string NameKey(int slot) => slot switch
    {
        Head => Loc.PlayerAppearance.ComponentHead,
        Mask => Loc.PlayerAppearance.ComponentMask,
        Hair => Loc.PlayerAppearance.ComponentHair,
        Torso => Loc.PlayerAppearance.ComponentTorso,
        Legs => Loc.PlayerAppearance.ComponentLegs,
        Bags => Loc.PlayerAppearance.ComponentBags,
        Shoes => Loc.PlayerAppearance.ComponentShoes,
        Neck => Loc.PlayerAppearance.ComponentNeck,
        Undershirt => Loc.PlayerAppearance.ComponentUndershirt,
        Armour => Loc.PlayerAppearance.ComponentArmour,
        Decals => Loc.PlayerAppearance.ComponentDecals,
        _ => Loc.PlayerAppearance.ComponentTops,
    };

    /// <summary>The same name in plain English, for console output that is never translated.</summary>
    public static string TechnicalName(int slot) => slot switch
    {
        Head => "head",
        Mask => "mask",
        Hair => "hair",
        Torso => "torso",
        Legs => "legs",
        Bags => "bags",
        Shoes => "shoes",
        Neck => "neck",
        Undershirt => "undershirt",
        Armour => "armour",
        Decals => "decals",
        Tops => "tops",
        _ => "unknown",
    };
}

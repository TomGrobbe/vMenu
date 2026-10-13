namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoCombo
{
    private const int MaxRecursion = 1;

    private const string MaleBikerTorso0 = "DLC_MP_BIKER_M_TORSO_0_0";

    private const string MaleBikerTorso1 = "DLC_MP_BIKER_M_TORSO_1_0";

    private const string MaleBikerTorso2 = "DLC_MP_BIKER_M_TORSO_2_0";

    private const string FemaleBikerTorso0 = "DLC_MP_BIKER_F_TORSO_0_0";

    private const string FemaleBikerTorso1 = "DLC_MP_BIKER_F_TORSO_1_0";

    private const string FemaleBikerTorso2 = "DLC_MP_BIKER_F_TORSO_2_0";

    private const string FemaleOpenShortTorso = "DLC_MP_H4_F_TORSO_0_0";

    private static readonly string[] FemaleBeachBareChestUndershirts =
    [
        "DLC_MP_BEACH_F_ACCS2_0", "DLC_MP_BEACH_F_ACCS2_1", "DLC_MP_BEACH_F_ACCS2_2",
        "DLC_MP_BEACH_F_ACCS2_3", "DLC_MP_BEACH_F_ACCS2_4", "DLC_MP_BEACH_F_ACCS2_5",
        "DLC_MP_BEACH_F_ACCS2_6", "DLC_MP_BEACH_F_ACCS2_7", "DLC_MP_BEACH_F_ACCS2_8",
        "DLC_MP_BEACH_F_ACCS2_9", "DLC_MP_BEACH_F_ACCS2_10", "DLC_MP_BEACH_F_ACCS2_11",
    ];

    private static readonly string[] FemaleValentineUndershirts =
    [
        "DLC_MP_VAL_F_ACCS2_0", "DLC_MP_VAL_F_ACCS2_1", "DLC_MP_VAL_F_ACCS2_2",
        "DLC_MP_VAL_F_ACCS2_3", "DLC_MP_VAL_F_ACCS2_4",
        "DLC_MP_VAL2_F_SPECIAL_0_0", "DLC_MP_VAL2_F_SPECIAL_0_1", "DLC_MP_VAL2_F_SPECIAL_0_2",
        "DLC_MP_VAL2_F_SPECIAL_0_3", "DLC_MP_VAL2_F_SPECIAL_0_4", "DLC_MP_VAL2_F_SPECIAL_0_5",
        "DLC_MP_VAL2_F_SPECIAL_0_6", "DLC_MP_VAL2_F_SPECIAL_0_7", "DLC_MP_VAL2_F_SPECIAL_0_8",
        "DLC_MP_VAL2_F_SPECIAL_0_9", "DLC_MP_VAL2_F_SPECIAL_0_10", "DLC_MP_VAL2_F_SPECIAL_0_11",
    ];

    private static readonly string[] FemaleBeachBikerVestUndershirts =
    [
        "DLC_MP_BEACH_F_ACCS0_0", "DLC_MP_BEACH_F_ACCS0_1", "DLC_MP_BEACH_F_ACCS0_2",
        "DLC_MP_BEACH_F_ACCS0_3", "DLC_MP_BEACH_F_ACCS0_4", "DLC_MP_BEACH_F_ACCS0_5",
        "DLC_MP_BEACH_F_ACCS0_6",
    ];

    private static readonly string[] FemaleOpenJacketTops =
    [
        TorsoTags.TailsJacket, TorsoTags.LowriderOpenCheck, TorsoTags.AirDraw3,
        TorsoTags.SmugglerDraw0, TorsoTags.SmugglerDraw1,
    ];

    private static readonly string[] FemaleBusinessUndershirts =
    [
        "HEIST_DRAW_5", "HEIST_DRAW_6", "HEIST_DRAW_7", "HEIST_DRAW_8", "HEIST_DRAW_9",
    ];

    internal static int? Torso(int ped, bool male, TorsoGarment top, TorsoGarment undershirt, TorsoGarment legs) =>
        male
            ? Male(ped, top, undershirt, legs, 0)
            : Female(ped, top, undershirt, legs, 0);

    private static int? Male(int ped, TorsoGarment top, TorsoGarment undershirt, TorsoGarment legs, int depth)
    {
        if (top.Has(TorsoTags.SilkRobe))
        {
            return 14;
        }

        if (top.Has(TorsoTags.SilkPyjamas))
        {
            return 6;
        }

        if (undershirt.Has(TorsoTags.StuntDraw(1)) && top.Has(TorsoTags.Jacket))
        {
            return 1;
        }

        if (undershirt.Has(TorsoTags.StuntDraw(2)) && top.Has(TorsoTags.Jacket))
        {
            return 4;
        }

        if (undershirt.HasHash(TorsoTags.UnnamedShirtTagHash))
        {
            return 4;
        }

        if (top.Has(TorsoTags.BikerVest) && !top.Has(TorsoTags.JacketOnly))
        {
            return MaleBikerVest(ped, top, undershirt, legs, depth);
        }

        if (undershirt.Has(TorsoTags.OvercoatAccessory))
        {
            return 12;
        }

        if (top.HasAny(TorsoTags.ApartmentDraw(15), TorsoTags.ApartmentDraw(24), TorsoTags.BikerDraw(6)))
        {
            return MaleQuiltedJacket(undershirt);
        }

        if (top.Has(TorsoTags.LowriderOpenCheck))
        {
            return MaleOpenCheckShirt(undershirt);
        }

        if (top.Has(TorsoTags.HeistDraw(7)))
        {
            return undershirt.IsGroup(12) ? 0 : 11;
        }

        if (top.Has(TorsoTags.HeistDraw(9)))
        {
            return MaleClosedScruffyJacket(undershirt);
        }

        if (top.Has(TorsoTags.TuxJacket))
        {
            return 12;
        }

        if (top.Has(TorsoTags.SmugglerDraw6))
        {
            return 2;
        }

        if (top.IsAnyGroup(0, 1))
        {
            return 0;
        }

        if (top.IsAnyGroup(3, 4, 6, 7, 10))
        {
            return MaleJacket(ped, undershirt);
        }

        if (top.IsGroup(11))
        {
            return MaleSweater(undershirt);
        }

        if (top.IsGroup(9))
        {
            return 0;
        }

        if (top.IsBaseDrawable(15) && undershirt.IsBaseDrawable(15))
        {
            return 15;
        }

        return null;
    }

    private static int? MaleBikerVest(int ped, TorsoGarment top, TorsoGarment undershirt, TorsoGarment legs, int depth)
    {
        if (undershirt.IsBaseDrawable(15))
        {
            if (top.HasAny(TorsoTags.BikerDraw(0), TorsoTags.BikerDraw(3), TorsoTags.BikerDraw(13)))
            {
                return Named(ped, MaleBikerTorso0);
            }

            if (top.Has(TorsoTags.BikerDraw(1)))
            {
                return Named(ped, MaleBikerTorso1);
            }

            if (top.Has(TorsoTags.BikerDraw(2)))
            {
                return Named(ped, MaleBikerTorso2);
            }

            return top.Has(TorsoTags.BikerDraw(5)) ? 5 : null;
        }

        if (depth >= MaxRecursion)
        {
            return 0;
        }

        var asTop = TorsoVariants.TopFromUndershirt(ped, male: true, undershirt);

        return Male(ped, asTop, TorsoGarment.None(PedComponentSlots.Undershirt), legs, depth + 1) ?? 0;
    }

    private static int MaleQuiltedJacket(TorsoGarment undershirt)
    {
        if (undershirt.IsBaseDrawable(15))
        {
            return 14;
        }

        if (undershirt.IsGroup(5))
        {
            return 6;
        }

        return undershirt.IsGroup(1) ? 1 : 4;
    }

    private static int MaleOpenCheckShirt(TorsoGarment undershirt)
    {
        if (undershirt.IsBaseDrawable(15))
        {
            return 14;
        }

        return undershirt.IsGroup(5) ? 6 : 1;
    }

    private static int? MaleClosedScruffyJacket(TorsoGarment undershirt)
    {
        if (undershirt.IsGroup(5))
        {
            return 6;
        }

        if (undershirt.IsBaseDrawable(15))
        {
            return 14;
        }

        return undershirt.IsGroup(1) ? 1 : null;
    }

    private static int? MaleJacket(int ped, TorsoGarment undershirt)
    {
        if (undershirt.IsGroup(5))
        {
            return 6;
        }

        if (undershirt.IsBaseDrawable(15) || undershirt.IsGroup(15))
        {
            return 14;
        }

        if (undershirt.IsGroup(10))
        {
            return 4;
        }

        if (undershirt.IsGroup(11))
        {
            return 12;
        }

        if (undershirt.IsAnyGroup(1, 14))
        {
            return 1;
        }

        if (undershirt.IsGroup(2))
        {
            return 4;
        }

        if (undershirt.Has(TorsoTags.ShirtBraces))
        {
            return undershirt.Has(TorsoTags.ClosedCollar) ? 4 : 1;
        }

        if (undershirt.IsGroup(9))
        {
            return 1;
        }

        if (undershirt.HasAny(TorsoTags.ApartmentDraw(2), TorsoTags.ApartmentDraw(3)))
        {
            return 4;
        }

        return undershirt.IsDlc
            ? TorsoRequisite.ForcedDrawable(ped, undershirt.Hash, PedComponentSlots.Torso)
            : null;
    }

    private static int? MaleSweater(TorsoGarment undershirt)
    {
        if (undershirt.Has(TorsoTags.SweatVest))
        {
            return 6;
        }

        if (undershirt.IsAnyGroup(3, 7, 6))
        {
            return 11;
        }

        if (undershirt.Has(TorsoTags.LongSleeve))
        {
            return 12;
        }

        return undershirt.IsBaseDrawable(15) ? 15 : null;
    }

    private static int? Female(int ped, TorsoGarment top, TorsoGarment undershirt, TorsoGarment legs, int depth)
    {
        var undershirtGroup = FemaleUndershirtGroup(undershirt);

        if (top.Has(TorsoTags.SilkRobe))
        {
            return 0;
        }

        if (undershirt.HasHash(TorsoTags.UnnamedShirtTagHash))
        {
            return 3;
        }

        if (top.Has(TorsoTags.BikerVest) && !top.Has(TorsoTags.JacketOnly))
        {
            return FemaleBikerVest(ped, top, undershirt, legs, undershirtGroup, depth);
        }

        if (top.Has(TorsoTags.OpenShort))
        {
            return FemaleOpenShortSleeves(ped, undershirt, FemaleOpenShortTorso);
        }

        if (top.Has(TorsoTags.OpenShortTwo))
        {
            return FemaleOpenShortSleeves(ped, undershirt, FemaleBikerTorso0);
        }

        if (undershirt.Has(TorsoTags.OvercoatAccessory))
        {
            return 3;
        }

        if (undershirt.Has(TorsoTags.VestShirt)
            && !TorsoJackets.IsJacket(male: false, top)
            && !top.Has(TorsoTags.SilkRobe))
        {
            return undershirt.Has(TorsoTags.LongSleeve) ? 3 : 0;
        }

        if (undershirt.Has(TorsoTags.ApartmentDraw(0)))
        {
            return 7;
        }

        if (undershirt.HasAny(TorsoTags.ApartmentDraw(2), TorsoTags.ApartmentDraw(3)))
        {
            return 3;
        }

        if (top.Has(TorsoTags.LowriderDraw(1)))
        {
            return legs.Has(TorsoTags.HighWaist) ? 11 : 15;
        }

        if (FemaleIsTuckedTee(undershirt))
        {
            return FemaleTuckedTee(top);
        }

        if (top.HasAny(
                TorsoTags.LuxeDraw(0),
                TorsoTags.BikerDraw(6),
                TorsoTags.BikerDraw(9),
                TorsoTags.BikerDraw(10),
                TorsoTags.BikerDraw(12),
                TorsoTags.BikerDraw(33),
                TorsoTags.AirDraw3,
                TorsoTags.SmugglerDraw1))
        {
            return undershirt.HasAny(FemaleBusinessUndershirts) ? 1 : 6;
        }

        if (top.IsGroup(1))
        {
            if (undershirtGroup is 5 or 15)
            {
                return 5;
            }

            return undershirt.IsDlc
                ? TorsoRequisite.ForcedDrawable(ped, undershirt.Hash, PedComponentSlots.Torso)
                : null;
        }

        if (top.IsGroup(7) || top.Has(TorsoTags.LowriderOpenCheck))
        {
            return undershirt.HasAny(FemaleBusinessUndershirts) ? 3 : 6;
        }

        return null;
    }

    private static int? FemaleOpenShortSleeves(int ped, TorsoGarment undershirt, string fallbackTorso)
    {
        if (undershirt.HasAny(TorsoTags.LowriderDraw(4), TorsoTags.LowriderDraw(5), TorsoTags.LowriderDraw(6)))
        {
            return 9;
        }

        return Named(ped, fallbackTorso);
    }

    private static int FemaleUndershirtGroup(TorsoGarment undershirt)
    {
        if (TorsoItems.IsAnyNamed(undershirt.Hash, FemaleBeachBareChestUndershirts))
        {
            return 15;
        }

        return TorsoItems.IsAnyNamed(undershirt.Hash, FemaleValentineUndershirts) ? 13 : undershirt.Group;
    }

    private static int? FemaleBikerVest(
        int ped,
        TorsoGarment top,
        TorsoGarment undershirt,
        TorsoGarment legs,
        int undershirtGroup,
        int depth)
    {
        if (FemaleBikerVestTakesShirt(undershirt, undershirtGroup))
        {
            if (top.HasAny(TorsoTags.BikerDraw(0), TorsoTags.BikerDraw(3), TorsoTags.BikerDraw(13)))
            {
                return Named(ped, FemaleBikerTorso0);
            }

            if (top.Has(TorsoTags.BikerDraw(1)))
            {
                return Named(ped, FemaleBikerTorso1);
            }

            if (top.HasAny(TorsoTags.BikerDraw(2), TorsoTags.BikerDraw(5), TorsoTags.X17Draw6))
            {
                return Named(ped, FemaleBikerTorso2);
            }

            return null;
        }

        if (undershirt.HasAny(TorsoTags.BikerDraw(9), TorsoTags.BikerDraw(10), TorsoTags.BikerDraw(11)))
        {
            return 11;
        }

        if (depth >= MaxRecursion)
        {
            return 14;
        }

        var asTop = TorsoVariants.TopFromUndershirt(ped, male: false, undershirt);

        return Female(ped, asTop, TorsoGarment.None(PedComponentSlots.Undershirt), legs, depth + 1) ?? 14;
    }

    private static bool FemaleBikerVestTakesShirt(TorsoGarment undershirt, int undershirtGroup)
    {
        if (undershirtGroup is 4 or 5 or 11 or 12 or 13 or 15)
        {
            return true;
        }

        if (TorsoItems.IsAnyNamed(undershirt.Hash, FemaleBeachBikerVestUndershirts))
        {
            return true;
        }

        return undershirt.HasAny(
            TorsoTags.LowriderTwoDraw(0),
            TorsoTags.LowriderTwoDraw(1),
            TorsoTags.LowriderTwoDraw(2),
            TorsoTags.LowriderTwoDraw(3),
            TorsoTags.LowriderTwoDraw(4),
            TorsoTags.LowriderTwoDraw(5),
            TorsoTags.BikerDraw(12),
            TorsoTags.BikerDraw(13),
            TorsoTags.BikerDraw(14));
    }

    private static bool FemaleIsTuckedTee(TorsoGarment undershirt) =>
        undershirt.HasAny(
            TorsoTags.LowriderDraw(0),
            TorsoTags.LowriderDraw(4),
            TorsoTags.LowriderDraw(6),
            TorsoTags.LowriderTwoDraw(0),
            TorsoTags.LowriderTwoDraw(3),
            TorsoTags.BikerDraw(6),
            TorsoTags.BikerDraw(7),
            TorsoTags.BikerDraw(8),
            TorsoTags.BikerDraw(9),
            TorsoTags.BikerDraw(10),
            TorsoTags.BikerDraw(11),
            TorsoTags.BikerDraw(12),
            TorsoTags.BikerDraw(13),
            TorsoTags.BikerDraw(14));

    private static int? FemaleTuckedTee(TorsoGarment top)
    {
        if (top.IsAnyGroup(1, 6, 8))
        {
            return 1;
        }

        if (top.IsAnyGroup(7, 10)
            || top.HasAny(FemaleOpenJacketTops)
            || top.HasAny(
                TorsoTags.HeistDraw(6),
                TorsoTags.HeistDraw(16),
                TorsoTags.HeistDraw(17),
                TorsoTags.LuxeDraw(0),
                TorsoTags.LuxeDraw(1),
                TorsoTags.LuxeDraw(2),
                TorsoTags.LuxeTwoDraw(0),
                TorsoTags.LuxeTwoDraw(1),
                TorsoTags.ApartmentDraw(15),
                TorsoTags.ApartmentDraw(25),
                TorsoTags.StuntDraw(4),
                TorsoTags.StuntDraw(9),
                TorsoTags.BikerDraw(4),
                TorsoTags.BikerDraw(6),
                TorsoTags.BikerDraw(9),
                TorsoTags.BikerDraw(10),
                TorsoTags.BikerDraw(12),
                TorsoTags.BikerDraw(33)))
        {
            return 3;
        }

        return top.Has(TorsoTags.HeistDraw(7)) ? 9 : null;
    }

    private static int? Named(int ped, string torsoItemName) =>
        TorsoItems.DrawableOfNamed(ped, PedComponentSlots.Torso, torsoItemName);
}

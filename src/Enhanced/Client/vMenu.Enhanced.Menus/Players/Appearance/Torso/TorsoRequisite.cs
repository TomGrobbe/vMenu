using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoRequisite
{
    private const int NoTorso = -1;

    private const string MaleOpenShortTorso = "DLC_MP_H4_M_TORSO_0_0";

    private const int MaleTopGroupNeedingVestCheck = 11;

    private const int MaleTopGroupVestTorso = 11;

    private const int MaleUndershirtBareChest = 15;

    private const int MaleUndershirtTankTop = 5;

    private const int MaleUndershirtTankTopTorso = 6;

    private const uint MaleOpenShortTankTopTorso = 799429565;

    private const int MaleOpenShortTwoBareChestTorso = 0;

    private const int MaleTopLastLadderedDrawable = 6;

    private const int MaleTopLastLadderedTexture = 11;

    private const int MaleUndershirtBareChestTorso = 14;

    private static readonly int[] MaleTopToTorso =
        [0, 0, 2, 1, 1, 5, 12, 1, 8, 0, 1, NoTorso, 12, 11, 4, 15];

    private static readonly int[] MaleUndershirtToTorso =
        [1, 1, NoTorso, 12, 12, 6, 11, 11, 1, 1, 4, 12, 1, 1, NoTorso, NoTorso];

    private static readonly int[] FemaleTopToTorso =
        [0, 5, 2, 3, 4, 4, 5, 6, 5, 9, 7, 11, 12, 4, 14, 15];

    internal static int? Torso(
        int ped,
        bool male,
        int changedSlot,
        TorsoGarment top,
        TorsoGarment undershirt)
    {
        return changedSlot switch
        {
            PedComponentSlots.Tops => TorsoForTop(ped, male, top, undershirt),
            PedComponentSlots.Undershirt => TorsoForUndershirt(ped, male, top, undershirt),
            PedComponentSlots.Decals => ForcedDrawable(
                ped,
                TorsoGarment.Read(ped, PedComponentSlots.Decals, male).Hash,
                PedComponentSlots.Torso),
            _ => null,
        };
    }

    internal static int? ForcedDrawable(int ped, uint item, int slot)
    {
        if (!TorsoItems.IsRealItem(item))
        {
            return null;
        }

        var count = Native.GetShopPedApparelForcedComponentCount(item);

        for (var index = 0; index < count; index++)
        {
            Native.GetForcedComponent(item, index, out var hash, out var value, out var type);

            if (type != slot)
            {
                continue;
            }

            return TorsoItems.IsRealItem((uint)hash)
                ? TorsoItems.DrawableOf(ped, slot, (uint)hash)
                : Usable(value);
        }

        return null;
    }

    private static int? TorsoForTop(int ped, bool male, TorsoGarment top, TorsoGarment undershirt)
    {
        if (!male)
        {
            return top.IsDlc
                ? ForcedDrawable(ped, top.Hash, PedComponentSlots.Torso)
                : Usable(FemaleTopToTorso[top.Drawable]);
        }

        if (!top.IsDlc)
        {
            if (top.Drawable == MaleTopGroupNeedingVestCheck)
            {
                return VestAwareTorso(undershirt);
            }

            return Usable(MaleTopToTorso[MaleTopLadderDrawable(top)]);
        }

        if (top.IsGroup(MaleTopGroupNeedingVestCheck) && KeepsTorsoForVest(undershirt))
        {
            return null;
        }

        return ForcedDrawable(ped, top.Hash, PedComponentSlots.Torso);
    }

    private static int? TorsoForUndershirt(int ped, bool male, TorsoGarment top, TorsoGarment undershirt)
    {
        if (!male)
        {
            return null;
        }

        if (undershirt.IsDlc)
        {
            return ForcedDrawable(ped, undershirt.Hash, PedComponentSlots.Torso);
        }

        if (undershirt.Drawable == MaleUndershirtTankTop)
        {
            return top.IsDlc && top.HasAny(TorsoTags.OpenShort, TorsoTags.OpenShortTwo)
                ? TorsoItems.DrawableOf(ped, PedComponentSlots.Torso, MaleOpenShortTankTopTorso)
                : MaleUndershirtTankTopTorso;
        }

        if (undershirt.Drawable == MaleUndershirtBareChest)
        {
            if (top.IsDlc && top.Has(TorsoTags.OpenShort))
            {
                return TorsoItems.DrawableOfNamed(ped, PedComponentSlots.Torso, MaleOpenShortTorso);
            }

            return top.IsDlc && top.Has(TorsoTags.OpenShortTwo)
                ? MaleOpenShortTwoBareChestTorso
                : MaleUndershirtBareChestTorso;
        }

        return Usable(MaleUndershirtToTorso[undershirt.Drawable]);
    }

    internal static int MaleTopLadderDrawable(TorsoGarment top) =>
        top.Drawable == MaleTopLastLadderedDrawable && top.Texture > MaleTopLastLadderedTexture
            ? MaleTopLastLadderedDrawable + 1
            : top.Drawable;

    private static int? VestAwareTorso(TorsoGarment undershirt) =>
        KeepsTorsoForVest(undershirt) ? null : MaleTopGroupVestTorso;

    private static bool KeepsTorsoForVest(TorsoGarment undershirt) =>
        undershirt.IsAnyGroup(6, 7) || (undershirt.IsDlc && undershirt.Has(TorsoTags.VestShirt));

    private static int? Usable(int drawable) => drawable == NoTorso ? null : drawable;
}

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoVariants
{
    private const int NoTop = -1;

    private static readonly int[] MaleUndershirtToTop =
        [0, 1, 0, NoTop, NoTop, 5, NoTop, NoTop, 8, 9, NoTop, NoTop, 12, 13, 1, 15];

    private static readonly int[] FemaleUndershirtToTop =
        [0, 0, NoTop, NoTop, 4, 5, NoTop, NoTop, NoTop, NoTop, NoTop, 11, 12, 13, NoTop, 15];

    internal static TorsoGarment TopFromUndershirt(int ped, bool male, TorsoGarment undershirt)
    {
        if (!undershirt.IsWorn)
        {
            return TorsoGarment.None(PedComponentSlots.Tops);
        }

        if (undershirt.IsDlc)
        {
            return VariantTop(ped, undershirt.Hash, male);
        }

        var table = male ? MaleUndershirtToTop : FemaleUndershirtToTop;
        var drawable = table[undershirt.Drawable];

        return drawable == NoTop
            ? TorsoGarment.None(PedComponentSlots.Tops)
            : TorsoGarment.BaseGame(PedComponentSlots.Tops, drawable, male);
    }

    private static TorsoGarment VariantTop(int ped, uint undershirtItem, bool male)
    {
        var count = Native.GetShopPedApparelVariantComponentCount(undershirtItem);

        for (var index = 0; index < count; index++)
        {
            Native.GetVariantComponent(undershirtItem, index, out var hash, out var value, out var type);

            if (type != PedComponentSlots.Tops)
            {
                continue;
            }

            if (!TorsoItems.IsRealItem((uint)hash))
            {
                return value == NoTop
                    ? TorsoGarment.None(PedComponentSlots.Tops)
                    : TorsoGarment.BaseGame(PedComponentSlots.Tops, value, male);
            }

            return TorsoItems.DrawableOf(ped, PedComponentSlots.Tops, (uint)hash) is { } drawable
                ? TorsoGarment.Dlc(ped, PedComponentSlots.Tops, drawable, male)
                : TorsoGarment.None(PedComponentSlots.Tops);
        }

        return TorsoGarment.None(PedComponentSlots.Tops);
    }
}

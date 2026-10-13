using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal static class TorsoGloves
{
    internal const int NoGlove = -1;

    internal static bool Worn(int ped, out int gloveType, out int gloveTexture)
    {
        gloveType = NoGlove;
        gloveTexture = 0;

        var drawable = Native.GetPedDrawableVariation(ped, PedComponentSlots.Torso);
        var item = TorsoItems.HashOfFirstTexture(ped, PedComponentSlots.Torso, drawable);

        if (!TorsoTags.Has(item, TorsoTags.Gloves))
        {
            return false;
        }

        if (!TorsoGloveTable.Worn(ped, item, out _, out gloveType))
        {
            return false;
        }

        gloveTexture = Math.Max(0, Native.GetPedTextureVariation(ped, PedComponentSlots.Torso));

        return true;
    }

    internal static int? DrawableFor(int ped, bool male, int baseTorso, int gloveType)
    {
        if (gloveType == NoGlove || baseTorso < 0)
        {
            return null;
        }

        var item = TorsoGloveTable.GloveItemFor(ped, male, baseTorso, gloveType);

        return item == 0 ? null : TorsoItems.DrawableOf(ped, PedComponentSlots.Torso, item);
    }
}

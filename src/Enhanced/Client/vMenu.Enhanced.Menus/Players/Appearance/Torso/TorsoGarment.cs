using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

internal sealed class TorsoGarment
{
    internal const int NoGroup = -1;

    private const int ClothingBaseGameDrawables = 16;

    private const int MaleDecalBaseGameDrawables = 7;

    private const int FemaleDecalBaseGameDrawables = 6;

    private const int NoDrawable = -1;

    private TorsoGarment(int slot, int drawable, int texture, uint hash, bool male)
    {
        Slot = slot;
        Drawable = drawable;
        Texture = texture;
        IsDlc = drawable >= BaseGameDrawables(slot, male);
        Hash = IsDlc ? hash : 0;
        Group = drawable < 0 ? NoGroup : IsDlc ? TorsoTags.DrawGroup(Hash) : drawable;
    }

    internal static int BaseGameDrawables(int slot, bool male)
    {
        if (slot != PedComponentSlots.Decals)
        {
            return ClothingBaseGameDrawables;
        }

        return male ? MaleDecalBaseGameDrawables : FemaleDecalBaseGameDrawables;
    }

    internal int Slot { get; }

    internal int Drawable { get; }

    internal int Texture { get; }

    internal uint Hash { get; }

    internal bool IsDlc { get; }

    internal int Group { get; }

    internal bool IsWorn => Drawable >= 0;

    internal static TorsoGarment Read(int ped, int slot, bool male)
    {
        var drawable = Native.GetPedDrawableVariation(ped, slot);
        var texture = Math.Max(0, Native.GetPedTextureVariation(ped, slot));

        return new TorsoGarment(slot, drawable, texture, TorsoItems.HashOf(ped, slot, drawable, texture), male);
    }

    internal static TorsoGarment None(int slot) => new(slot, NoDrawable, 0, 0, true);

    internal static TorsoGarment BaseGame(int slot, int drawable, bool male) =>
        new(slot, drawable, 0, 0, male);

    internal static TorsoGarment Dlc(int ped, int slot, int drawable, bool male) =>
        new(slot, drawable, 0, TorsoItems.HashOfFirstTexture(ped, slot, drawable), male);

    internal bool Has(string tag) => TorsoTags.Has(Hash, tag);

    internal bool HasHash(uint tagHash) => TorsoTags.HasHash(Hash, tagHash);

    internal bool HasAny(params string[] tags) => TorsoTags.HasAny(Hash, tags);

    internal bool IsBaseDrawable(int drawable) => !IsDlc && Drawable == drawable;

    internal bool IsGroup(int group) => group != NoGroup && Group == group;

    internal bool IsAnyGroup(params int[] groups)
    {
        foreach (var group in groups)
        {
            if (IsGroup(group))
            {
                return true;
            }
        }

        return false;
    }
}

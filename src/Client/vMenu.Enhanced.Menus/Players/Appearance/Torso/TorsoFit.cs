using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus.Players.Appearance.Torso;

public static class TorsoFit
{
    private const int FirstTexture = 0;

    internal static bool IsEnabled => UserDefaults.CharacterCreatorFitTorso.Value;

    internal static void SetEnabled(bool enabled) =>
        UserDefaults.CharacterCreatorFitTorso.Value = enabled;

    internal static void Forget()
    {
        TorsoItems.Forget();
        TorsoTags.Forget();
        TorsoGloveTable.Forget();
    }

    internal static bool Triggers(int slot) =>
        slot is PedComponentSlots.Tops
            or PedComponentSlots.Undershirt
            or PedComponentSlots.Legs
            or PedComponentSlots.Decals;

    internal static TorsoSnapshot? Before(int ped, bool fitting)
    {
        if (!fitting || !IsEnabled || !PedSpawning.IsWearingFreemode())
        {
            return null;
        }

        var wearingGloves = TorsoGloves.Worn(ped, out var gloveType, out var gloveTexture);

        return new TorsoSnapshot
        {
            Ped = ped,
            IsMale = PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(ped)),
            GloveType = wearingGloves ? gloveType : TorsoGloves.NoGlove,
            GloveTexture = gloveTexture,
        };
    }

    internal static void FitWornOutfit(int ped) =>
        Apply(Before(ped, fitting: true), PedComponentSlots.Tops, null);

    internal static void Apply(TorsoSnapshot? before, int changedSlot, Action? redrawTorso)
    {
        if (before is null || !Triggers(changedSlot))
        {
            return;
        }

        var ped = before.Ped;

        if (ped != Native.PlayerPedId() || !PedSpawning.IsWearingFreemode())
        {
            return;
        }

        var male = before.IsMale;
        var top = TorsoGarment.Read(ped, PedComponentSlots.Tops, male);
        var undershirt = TorsoGarment.Read(ped, PedComponentSlots.Undershirt, male);
        var legs = TorsoGarment.Read(ped, PedComponentSlots.Legs, male);

        var requisite = TorsoRequisite.Torso(ped, male, changedSlot, top, undershirt);
        var combo = TorsoCombo.Torso(ped, male, top, undershirt, legs);
        var changed = false;

        if ((combo ?? requisite) is { } torso)
        {
            changed = Wear(ped, torso, FirstTexture);
        }

        changed |= RestoreGloves(ped, before);

        if (changed)
        {
            redrawTorso?.Invoke();
        }
    }

    private static bool RestoreGloves(int ped, TorsoSnapshot before)
    {
        if (before.GloveType == TorsoGloves.NoGlove)
        {
            return false;
        }

        var baseTorso = Native.GetPedDrawableVariation(ped, PedComponentSlots.Torso);
        var glove = TorsoGloves.DrawableFor(ped, before.IsMale, baseTorso, before.GloveType);

        return glove is { } drawable && Wear(ped, drawable, before.GloveTexture);
    }

    private static bool Wear(int ped, int drawable, int texture)
    {
        if (drawable < 0 || drawable >= Native.GetNumberOfPedDrawableVariations(ped, PedComponentSlots.Torso))
        {
            Log.Debug($"[Character] Torso {drawable} is not on this ped, so the arms were left alone.");

            return false;
        }

        var textures = Native.GetNumberOfPedTextureVariations(ped, PedComponentSlots.Torso, drawable);
        var wanted = texture >= 0 && texture < textures ? texture : FirstTexture;

        if (Native.GetPedDrawableVariation(ped, PedComponentSlots.Torso) == drawable
            && Math.Max(0, Native.GetPedTextureVariation(ped, PedComponentSlots.Torso)) == wanted)
        {
            return false;
        }

        Native.SetPedComponentVariation(
            ped,
            PedComponentSlots.Torso,
            drawable,
            wanted,
            PedVariationScope.CurrentPalette(ped, PedComponentSlots.Torso));

        return true;
    }
}

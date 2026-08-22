using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class FreemodeWriter
{
    private const int BlendTimeout = 1000;

    public static async Task ApplyAsync(int ped, MpCharacter character, MpCharacterStyle? style, MpCharacterOutfit? outfit)
    {
        await ApplyCoreAsync(ped, character.Core);

        if (style is not null)
        {
            ApplyStyle(ped, style);
        }

        if (outfit is not null)
        {
            PedAppearanceWriter.Apply(ped, outfit.Outfit);
        }

        if (style is not null)
        {
            ApplyHair(ped, style, outfit?.Outfit.ComponentAt(PedComponentSlots.Hair) is null);
        }

        ApplyDecorations(ped, character.Core.Tattoos, style);
        ApplyExpression(ped, character.FacialExpression);
    }

    public static async Task ApplyCoreAsync(int ped, MpCharacterCore core)
    {
        Native.SetPedHeadBlendData(
            ped,
            core.Blend.FirstShape,
            core.Blend.SecondShape,
            core.Blend.ThirdShape,
            core.Blend.FirstSkin,
            core.Blend.SecondSkin,
            core.Blend.ThirdSkin,
            core.Blend.ShapeMix,
            core.Blend.SkinMix,
            core.Blend.ThirdMix,
            core.Blend.IsParent);

        var started = Native.GetGameTimer();

        while (!Native.HasPedHeadBlendFinished(ped) && Native.GetGameTimer() - started < BlendTimeout)
        {
            await API.Delay(0);
        }

        ApplyFaceFeatures(ped, core.FaceFeatures);
        ApplyOverlays(ped, core.Overlays);

        Native.SetPedEyeColor(ped, core.EyeColour);
    }

    public static void ApplyFaceFeatures(int ped, List<float> features)
    {
        for (var feature = 0; feature < FreemodeReader.FaceFeatureCount; feature++)
        {
            Native.SetPedFaceFeature(ped, feature, feature < features.Count ? features[feature] : 0f);
        }
    }

    public static void ApplyOverlays(int ped, List<PedOverlayValue> overlays)
    {
        foreach (var overlay in overlays)
        {
            ApplyOverlay(ped, overlay);
        }
    }

    public static void ApplyOverlay(int ped, PedOverlayValue overlay)
    {
        Native.SetPedHeadOverlay(ped, overlay.Slot, overlay.Style, overlay.Opacity);

        var ramp = PedHeadOverlays.ColourType(overlay.Slot);

        if (ramp == PedHeadOverlays.NoColour)
        {
            return;
        }

        Native.SetPedHeadOverlayColor(ped, overlay.Slot, ramp, overlay.Colour, overlay.Colour);
    }

    public static void ApplyStyle(int ped, MpCharacterStyle style)
    {
        ApplyHair(ped, style);
        ApplyOverlays(ped, style.Overlays);
    }

    public static void ApplyHair(int ped, MpCharacterStyle style, bool drawable = true)
    {
        if (drawable)
        {
            var available = Native.GetNumberOfPedDrawableVariations(ped, PedComponentSlots.Hair);

            var hair = style.HairStyle >= 0 && style.HairStyle < available ? style.HairStyle : 0;

            Native.SetPedComponentVariation(ped, PedComponentSlots.Hair, hair, 0, 0);
        }

        Native.SetPedHairColor(ped, style.HairColour, style.HairHighlight);
    }

    public static void ApplyDecorations(int ped, PedTattooSet tattoos, MpCharacterStyle? style)
    {
        Native.ClearPedDecorations(ped);
        Native.ClearPedFacialDecorations(ped);

        foreach (var tattoo in tattoos.Everything())
        {
            Native.AddPedDecorationFromHashes(
                ped, TattooCatalogue.Hash(tattoo.Collection), TattooCatalogue.Hash(tattoo.Name));
        }

        if (style is null || style.HairDecorationName.Length == 0)
        {
            return;
        }

        Native.SetPedFacialDecoration(
            ped,
            TattooCatalogue.Hash(style.HairDecorationCollection),
            TattooCatalogue.Hash(style.HairDecorationName));
    }

    public static void ApplyExpression(int ped, string expression)
    {
        if (expression.Length == 0)
        {
            Native.ClearFacialIdleAnimOverride(ped);

            return;
        }

        NativeFixer.SetFacialIdleAnimOverride(ped, expression);
    }
}

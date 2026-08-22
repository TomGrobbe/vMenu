using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class FreemodeReader
{
    private const int BlendFrames = 10;

    public const int FaceFeatureCount = 20;

    public static async Task<MpCharacterCore?> ReadCoreAsync(int ped)
    {
        var model = (uint)Native.GetEntityModel(ped);

        if (!PedSpawning.IsFreemode(model))
        {
            return null;
        }

        return new MpCharacterCore
        {
            IsMale = PedSpawning.IsFreemodeMale(model),
            Blend = await ReadBlendAsync(ped),
            FaceFeatures = ReadFaceFeatures(ped),
            EyeColour = Native.GetPedEyeColor(ped),
            Overlays = ReadOverlays(ped, PedHeadOverlays.Core),
            Tattoos = ReadTattoos(ped),
        };
    }

    public static MpCharacterStyle ReadStyle(int ped)
    {
        var hairStyle = Native.GetPedDrawableVariation(ped, PedComponentSlots.Hair);
        var decoration = PedHairDecorations.For(hairStyle);

        return new MpCharacterStyle
        {
            HairStyle = hairStyle,
            HairColour = Native.GetPedHairColor(ped),
            HairHighlight = Native.GetPedHairHighlightColor(ped),

            HairDecorationCollection = decoration?.Collection ?? string.Empty,
            HairDecorationName = decoration?.Name ?? string.Empty,
            Overlays = ReadOverlays(ped, PedHeadOverlays.Style),
        };
    }

    public static PedOutfit ReadOutfit(int ped) => PedAppearanceReader.ReadOutfit(ped);

    private static async Task<PedHeadBlend> ReadBlendAsync(int ped)
    {
        var buffer = new PedHeadBlendBuffer();

        if (!Native.GetPedHeadBlendData(ped, buffer))
        {
            return new PedHeadBlend();
        }

        var asked = Native.GetFrameCount();

        for (var waited = 0; waited < BlendFrames && Native.GetFrameCount() == asked; waited++)
        {
            await API.Delay(0);
        }

        if (buffer.IsEmpty)
        {
            Log.Debug("[Character] The game reported no head blend for this ped, so a neutral one is used.");

            return new PedHeadBlend();
        }

        return new PedHeadBlend
        {
            FirstShape = buffer.FirstShape,
            SecondShape = buffer.SecondShape,
            ThirdShape = buffer.ThirdShape,
            FirstSkin = buffer.FirstSkin,
            SecondSkin = buffer.SecondSkin,
            ThirdSkin = buffer.ThirdSkin,

            ShapeMix = Clamp(buffer.ShapeMix),
            SkinMix = Clamp(buffer.SkinMix),
            ThirdMix = Clamp(buffer.ThirdMix),
            IsParent = buffer.IsParent,
        };
    }

    private static List<float> ReadFaceFeatures(int ped)
    {
        var features = new List<float>(FaceFeatureCount);

        for (var feature = 0; feature < FaceFeatureCount; feature++)
        {
            features.Add(Native.GetPedFaceFeature(ped, feature));
        }

        return features;
    }

    private static List<PedOverlayValue> ReadOverlays(int ped, int[] slots)
    {
        var overlays = new List<PedOverlayValue>(slots.Length);

        foreach (var slot in slots)
        {
            if (!Native.GetPedHeadOverlayData(ped, slot, out var style, out _, out var colour, out _, out var opacity))
            {
                continue;
            }

            overlays.Add(new PedOverlayValue
            {
                Slot = slot,

                Style = style == PedHeadOverlays.Unset ? 0 : style,
                Opacity = style == PedHeadOverlays.Unset ? 0f : Clamp(opacity),
                Colour = Math.Max(0, colour),
            });
        }

        return overlays;
    }

    private static PedTattooSet ReadTattoos(int ped)
    {
        var tattoos = new PedTattooSet();

        foreach (var (collection, overlay) in NativeFixer.GetPedDecorations(ped))
        {
            if (TattooCatalogue.Resolve(collection, overlay) is not { } known)
            {
                continue;
            }

            if (PedHairDecorations.IsScalpOverlay(known.Collection, known.Name))
            {
                continue;
            }

            List(tattoos, known.Zone).Add(new TattooRef
            {
                Collection = known.Collection,
                Name = known.Name,
            });
        }

        return tattoos;
    }

    public static List<TattooRef> List(PedTattooSet tattoos, TattooZone zone) => zone switch
    {
        TattooZone.Hair => tattoos.Hair,
        TattooZone.Head => tattoos.Head,
        TattooZone.Torso => tattoos.Torso,
        TattooZone.LeftArm => tattoos.LeftArm,
        TattooZone.RightArm => tattoos.RightArm,
        TattooZone.LeftLeg => tattoos.LeftLeg,
        TattooZone.RightLeg => tattoos.RightLeg,
        TattooZone.Badge => tattoos.Badges,
        _ => tattoos.Addons,
    };

    private static float Clamp(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
}

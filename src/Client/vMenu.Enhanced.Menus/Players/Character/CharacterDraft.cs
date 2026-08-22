using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Appearance;
using vMenu.Enhanced.Menus.Players.Appearance;

namespace vMenu.Enhanced.Menus.Players.Character;

public static class CharacterDraft
{
    public const string FirstVariantName = "Default";

    public const string DefaultExpression = "mood_Normal_1";

    public static readonly string[] Expressions =
    [
        "mood_Normal_1",
        "mood_Happy_1",
        "mood_Angry_1",
        "mood_Aiming_1",
        "mood_Injured_1",
        "mood_stressed_1",
        "mood_smug_1",
        "mood_sulk_1",
    ];

    public static MpCharacter New(bool male)
    {
        var character = new MpCharacter
        {
            FacialExpression = DefaultExpression,
            Core = new MpCharacterCore
            {
                IsMale = male,
                Blend = new PedHeadBlend { ShapeMix = 0.5f, SkinMix = 0.5f },
                FaceFeatures = NeutralFace(),
                Overlays = BlankOverlays(PedHeadOverlays.Core),
            },
        };

        character.Styles.Add(new MpCharacterStyle
        {
            Name = FirstVariantName,
            Overlays = BlankOverlays(PedHeadOverlays.Style),
        });

        character.Outfits.Add(new MpCharacterOutfit
        {
            Name = FirstVariantName,
            Outfit = StartingClothes(male),
        });

        character.LastStyle = FirstVariantName;
        character.LastOutfit = FirstVariantName;

        return character;
    }

    public static async Task<MpCharacter?> FromPlayerAsync(int ped)
    {
        if (await FreemodeReader.ReadCoreAsync(ped) is not { } core)
        {
            return null;
        }

        var style = FreemodeReader.ReadStyle(ped);

        style.Name = FirstVariantName;

        var character = new MpCharacter
        {
            Core = core,
            FacialExpression = DefaultExpression,
            LastStyle = FirstVariantName,
            LastOutfit = FirstVariantName,
        };

        character.Styles.Add(style);

        character.Outfits.Add(new MpCharacterOutfit
        {
            Name = FirstVariantName,
            Outfit = FreemodeReader.ReadOutfit(ped),
        });

        return character;
    }

    public static void Randomise(MpCharacter character, MpCharacterStyle style, int ped)
    {
        var core = character.Core;

        core.Blend = new PedHeadBlend
        {
            FirstShape = RandomParent(core.IsMale ? 0 : 1),
            SecondShape = RandomParent(core.IsMale ? 1 : 0),
            FirstSkin = RandomParent(core.IsMale ? 0 : 1),
            SecondSkin = RandomParent(core.IsMale ? 1 : 0),
            ShapeMix = Native.GetRandomIntInRange(0, 11) / 10f,
            SkinMix = Native.GetRandomIntInRange(0, 11) / 10f,
        };

        var features = new List<float>(FreemodeReader.FaceFeatureCount);

        for (var feature = 0; feature < FreemodeReader.FaceFeatureCount; feature++)
        {
            features.Add(Native.GetRandomIntInRange(-10, 11) / 10f);
        }

        core.FaceFeatures = features;
        core.EyeColour = Native.GetRandomIntInRange(0, EyeColourCount);

        foreach (var overlay in core.Overlays)
        {
            Roll(overlay);
        }

        foreach (var overlay in style.Overlays)
        {
            if (PedHeadOverlays.IsMaleOnly(overlay.Slot) && !core.IsMale)
            {
                overlay.Style = 0;
                overlay.Opacity = 0f;

                continue;
            }

            Roll(overlay);
        }

        var hairStyles = Native.GetNumberOfPedDrawableVariations(ped, PedComponentSlots.Hair);

        style.HairStyle = hairStyles > 0 ? Native.GetRandomIntInRange(0, hairStyles) : 0;
        style.HairColour = Native.GetRandomIntInRange(0, Math.Max(1, Native.GetNumHairColors()));
        style.HairHighlight = Native.GetRandomIntInRange(0, Math.Max(1, Native.GetNumHairColors()));

        var decoration = PedHairDecorations.For(style.HairStyle);

        style.HairDecorationCollection = decoration?.Collection ?? string.Empty;
        style.HairDecorationName = decoration?.Name ?? string.Empty;
    }

    public const int EyeColourCount = 32;

    public static List<float> NeutralFace()
    {
        var features = new List<float>(FreemodeReader.FaceFeatureCount);

        for (var feature = 0; feature < FreemodeReader.FaceFeatureCount; feature++)
        {
            features.Add(0f);
        }

        return features;
    }

    public static List<PedOverlayValue> BlankOverlays(int[] slots)
    {
        var overlays = new List<PedOverlayValue>(slots.Length);

        foreach (var slot in slots)
        {
            overlays.Add(new PedOverlayValue { Slot = slot, Style = 0, Opacity = 0f, Colour = 0 });
        }

        return overlays;
    }

    private static PedOutfit StartingClothes(bool male)
    {
        var outfit = new PedOutfit();

        Wear(outfit, PedComponentSlots.Torso, 15, 0);

        if (male)
        {
            Wear(outfit, PedComponentSlots.Undershirt, 15, 0);
            Wear(outfit, PedComponentSlots.Tops, 15, 0);
            Wear(outfit, PedComponentSlots.Legs, 61, Native.GetRandomIntInRange(0, 15));
            Wear(outfit, PedComponentSlots.Shoes, 34, 0);

            return outfit;
        }

        var trousers = Native.GetRandomIntInRange(0, 15);

        Wear(outfit, PedComponentSlots.Undershirt, 14, 0);
        Wear(outfit, PedComponentSlots.Legs, 17, trousers);
        Wear(outfit, PedComponentSlots.Tops, 18, trousers);
        Wear(outfit, PedComponentSlots.Shoes, 35, 0);

        return outfit;
    }

    private static void Wear(PedOutfit outfit, int slot, int drawable, int texture) =>
        outfit.Components.Add(new PedComponentValue
        {
            Slot = slot,
            Drawable = drawable,
            Texture = texture,
        });

    private static void Roll(PedOverlayValue overlay)
    {
        var styles = Native.GetNumHeadOverlayValues(overlay.Slot);

        overlay.Style = styles > 0 ? Native.GetRandomIntInRange(0, styles) : 0;
        overlay.Opacity = Native.GetRandomIntInRange(0, 11) / 10f;

        if (PedHeadOverlays.ColourType(overlay.Slot) != PedHeadOverlays.NoColour)
        {
            overlay.Colour = Native.GetRandomIntInRange(0, Math.Max(1, Native.GetNumHairColors()));
        }
    }

    private static int RandomParent(int list)
    {
        var count = Native.GetNumParentPedsOfType(list);

        return count > 0 ? Native.GetRandomIntInRange(0, count) : 0;
    }
}

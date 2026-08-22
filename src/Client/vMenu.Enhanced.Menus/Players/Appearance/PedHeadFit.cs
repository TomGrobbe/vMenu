using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Players.Character;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Players.Appearance;

public static class PedHeadFit
{
    private const int IntervalMs = 500;

    private const int SmallMaleHead = 0;

    private const int SmallFemaleHead = 21;

    private const int BaldHair = 0;

    private const int ComponentApparel = 0;

    private const int PropApparel = 1;

    private const int NoItem = 1849449579;

    private static int _mask = int.MinValue;

    private static int _maskTexture = int.MinValue;

    private static int _hat = int.MinValue;

    private static bool _shrunk;

    private static int? _hair;

    private static bool _recheck = true;

    public static void Initialize() =>
        TickRegistry.Register("Character.HeadFit", CheckAsync, TickRate.Every(IntervalMs));

    public static void Forget()
    {
        _mask = int.MinValue;
        _maskTexture = int.MinValue;
        _hat = int.MinValue;
        _shrunk = false;
        _hair = null;
        _recheck = true;
    }

    private static async Task CheckAsync()
    {
        var ped = Native.PlayerPedId();

        if (!PedSpawning.IsWearingFreemode())
        {
            if (_shrunk || _mask != int.MinValue)
            {
                Forget();
            }

            return;
        }

        await CheckHelmetAsync(ped);
        await CheckMaskAsync(ped);
        await CheckHairAsync(ped);
    }

    private static async Task CheckHairAsync(int ped)
    {
        if (!_recheck)
        {
            return;
        }

        _recheck = false;

        var wanted = await WantedHairAsync(ped);

        if (wanted == _hair)
        {
            return;
        }

        _hair = wanted;

        if (wanted is { } drawable)
        {
            Native.SetPedComponentVariation(ped, PedComponentSlots.Hair, drawable, 0, 0);

            Log.Debug($"[Character] Hair forced to {drawable}.");

            return;
        }

        if (MpCharacterState.Style is { } style)
        {
            FreemodeWriter.ApplyHair(ped, style);
        }

        Log.Debug("[Character] Hair restored.");
    }

    private static async Task<int?> WantedHairAsync(int ped)
    {
        foreach (var (item, apparel) in Worn(ped))
        {
            if (AnyBaldTag(item, apparel))
            {
                return BaldHair;
            }

            if (await ForcedHairAsync(item) is { } forced)
            {
                return forced;
            }
        }

        return null;
    }

    private static List<(uint Item, int Apparel)> Worn(int ped)
    {
        var worn = new List<(uint, int)>(2);

        var mask = Native.GetPedDrawableVariation(ped, PedComponentSlots.Mask);

        if (mask > 0)
        {
            var texture = Math.Max(0, Native.GetPedTextureVariation(ped, PedComponentSlots.Mask));

            worn.Add(((uint)Native.GetHashNameForComponent(ped, PedComponentSlots.Mask, mask, texture), ComponentApparel));
        }

        var hat = Native.GetPedPropIndex(ped, PedPropSlots.Hats, false);

        if (hat >= 0)
        {
            var texture = Math.Max(0, Native.GetPedPropTextureIndex(ped, PedPropSlots.Hats));

            worn.Add(((uint)Native.GetHashNameForProp(ped, PedPropSlots.Hats, hat, texture), PropApparel));
        }

        return worn;
    }

    private static async Task<int?> ForcedHairAsync(uint item)
    {
        var count = Native.GetShopPedApparelForcedComponentCount(item);

        for (var index = 0; index < count; index++)
        {
            Native.GetForcedComponent(item, index, out var hash, out var value, out var type);

            if (type != PedComponentSlots.Hair)
            {
                continue;
            }

            if (hash is 0 or NoItem)
            {
                Log.Debug($"[Character] Item {item} forces base game hair {value}, which cannot be resolved to a drawable.");

                continue;
            }

            var component = new ShopPedComponentBuffer();

            Native.GetShopPedComponent((uint)hash, component);

            var asked = Native.GetFrameCount();

            while (Native.GetFrameCount() == asked)
            {
                await API.Delay(0);
            }

            Log.Debug($"[Character] Item {item} forces hair {component.Drawable}.");

            return component.Drawable;
        }

        return null;
    }

    private static readonly string[] BaldTags = ["FORCE_BALD", "HAIR_SHRINK"];

    private static bool AnyBaldTag(uint item, int apparel)
    {
        foreach (var tag in BaldTags)
        {
            if (Native.DoesShopPedApparelHaveRestrictionTag(item, (uint)Native.GetHashKey(tag), apparel))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task CheckMaskAsync(int ped)
    {
        var drawable = Native.GetPedDrawableVariation(ped, PedComponentSlots.Mask);
        var texture = Math.Max(0, Native.GetPedTextureVariation(ped, PedComponentSlots.Mask));

        if (drawable == _mask && texture == _maskTexture)
        {
            return;
        }

        _mask = drawable;
        _maskTexture = texture;
        _recheck = true;

        Report(ped, "Mask changed");

        var shrink = ShouldShrink(ped, drawable, texture);

        if (shrink == _shrunk)
        {
            return;
        }

        if (!shrink)
        {
            Restore(ped);

            _shrunk = false;

            return;
        }

        await Remember(ped);

        _shrunk = true;

        if (MpCharacterState.Worn is null)
        {
            return;
        }

        Shrink(ped);
    }

    private static void Report(int ped, string what)
    {
        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        var shrink = (uint)Native.GetHashKey("SHRINK_HEAD");

        var mask = Native.GetPedDrawableVariation(ped, PedComponentSlots.Mask);
        var maskTexture = Math.Max(0, Native.GetPedTextureVariation(ped, PedComponentSlots.Mask));
        var maskItem = (uint)Native.GetHashNameForComponent(ped, PedComponentSlots.Mask, mask, maskTexture);

        var hat = Native.GetPedPropIndex(ped, PedPropSlots.Hats, false);
        var hatTexture = Math.Max(0, Native.GetPedPropTextureIndex(ped, PedPropSlots.Hats));

        var hatItem = hat < 0
            ? 0u
            : (uint)Native.GetHashNameForProp(ped, PedPropSlots.Hats, hat, hatTexture);

        Log.Debug(
            $"[Character] {what} on the freemode {(PedSpawning.IsFreemodeMale((uint)Native.GetEntityModel(ped)) ? "male" : "female")}: "
            + $"mask {mask}/{maskTexture} hash {maskItem} shrink {Native.DoesShopPedApparelHaveRestrictionTag(maskItem, shrink, ComponentApparel)}"
            + $"{Tags(maskItem, ComponentApparel)}{Forced(maskItem)}{Variants(maskItem)}, "
            + $"hat {hat}/{hatTexture} hash {hatItem}{Tags(hatItem, PropApparel)}{Forced(hatItem)}{Variants(hatItem)}");
    }

    private static string Tags(uint item, int apparel)
    {
        var carried = new List<string>();

        foreach (var tag in BaldTags)
        {
            if (item != 0 && Native.DoesShopPedApparelHaveRestrictionTag(item, (uint)Native.GetHashKey(tag), apparel))
            {
                carried.Add(tag);
            }
        }

        return carried.Count == 0 ? " no hair tags" : " " + string.Join(" ", carried);
    }

    private static string Forced(uint item)
    {
        if (item == 0)
        {
            return string.Empty;
        }

        var count = Native.GetShopPedApparelForcedComponentCount(item);
        var forced = new List<string>(count);

        for (var index = 0; index < count; index++)
        {
            Native.GetForcedComponent(item, index, out var hash, out var value, out var type);

            forced.Add($"slot {type} hash {hash} value {value}");
        }

        return forced.Count == 0 ? " forces nothing" : " forces [" + string.Join("; ", forced) + "]";
    }

    private static string Variants(uint item)
    {
        if (item == 0)
        {
            return string.Empty;
        }

        var count = Native.GetShopPedApparelVariantComponentCount(item);
        var variants = new List<string>(count);

        for (var index = 0; index < count; index++)
        {
            Native.GetVariantComponent(item, index, out var hash, out var value, out var type);

            variants.Add($"slot {type} hash {hash} value {value}");
        }

        return variants.Count == 0 ? " no variants" : " variants [" + string.Join("; ", variants) + "]";
    }

    private static bool ShouldShrink(int ped, int drawable, int texture)
    {
        if (drawable <= 0)
        {
            return false;
        }

        var item = (uint)Native.GetHashNameForComponent(ped, PedComponentSlots.Mask, drawable, texture);

        return Native.DoesShopPedApparelHaveRestrictionTag(
            item, (uint)Native.GetHashKey("SHRINK_HEAD"), ComponentApparel);
    }

    private static async Task Remember(int ped)
    {
        if (MpCharacterState.Worn is not null && MpCharacterState.MatchesPlayer())
        {
            return;
        }

        if (!await MpCharacterState.AdoptAsync(ped))
        {
            Log.Debug("[Character] A mask needs a smaller head but this ped's face could not be read first.");
        }
    }

    private static void Shrink(int ped)
    {
        if (MpCharacterState.Worn is not { } core)
        {
            return;
        }

        Native.SetPedHeadBlendData(
            ped,
            core.IsMale ? SmallMaleHead : SmallFemaleHead,
            0,
            0,
            core.Blend.FirstSkin,
            core.Blend.SecondSkin,
            core.Blend.ThirdSkin,
            0f,
            core.Blend.SkinMix,
            0f,
            false);

        for (var feature = 0; feature < FreemodeReader.FaceFeatureCount; feature++)
        {
            Native.SetPedFaceFeature(ped, feature, 0f);
        }
    }

    private static void Restore(int ped)
    {
        if (MpCharacterState.Worn is not { } core)
        {
            return;
        }

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

        FreemodeWriter.ApplyFaceFeatures(ped, core.FaceFeatures);
    }

    private static async Task CheckHelmetAsync(int ped)
    {
        var hat = Native.GetPedPropIndex(ped, PedPropSlots.Hats, false);

        if (hat == _hat)
        {
            return;
        }

        _hat = hat;
        _recheck = true;

        Report(ped, "Hat changed");

        if (hat < 0)
        {
            PedVisorHint.Reset();

            return;
        }

        PedVisorHint.ShowIfHelmet(ped, PedPropSlots.Hats);

        await Task.CompletedTask;
    }
}

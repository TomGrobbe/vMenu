using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterEdit
{
    internal static MpCharacter? Draft => MpCharacterState.Draft;

    internal static MpCharacterStyle? Style => MpCharacterState.Style;

    internal static int Ped => Native.PlayerPedId();

    internal static void ApplyBlend()
    {
        if (Draft?.Core is not { } core)
        {
            return;
        }

        Native.SetPedHeadBlendData(
            Ped,
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

        ApplyFace();
    }

    internal static void ApplyFace()
    {
        if (Draft?.Core is { } core)
        {
            FreemodeWriter.ApplyFaceFeatures(Ped, core.FaceFeatures);
        }
    }

    internal static void ApplyOverlay(PedOverlayValue overlay) => FreemodeWriter.ApplyOverlay(Ped, overlay);

    internal static void ApplyEyes()
    {
        if (Draft?.Core is { } core)
        {
            Native.SetPedEyeColor(Ped, core.EyeColour);
        }
    }

    internal static void ApplyHair()
    {
        if (Style is not { } style)
        {
            return;
        }

        var decoration = PedHairDecorations.For(style.HairStyle);

        style.HairDecorationCollection = decoration?.Collection ?? string.Empty;
        style.HairDecorationName = decoration?.Name ?? string.Empty;

        FreemodeWriter.ApplyHair(Ped, style);
        ApplyTattoos();
    }

    internal static void ApplyTattoos()
    {
        if (Draft?.Core is { } core)
        {
            FreemodeWriter.ApplyDecorations(Ped, core.Tattoos, Style);
        }
    }

    internal static void ApplyExpression()
    {
        if (Draft is not { } draft)
        {
            return;
        }

        _expressionAt = Native.GetGameTimer();

        FreemodeWriter.ApplyExpression(Ped, draft.FacialExpression);

        Log.Debug($"[Character] Facial expression set to '{draft.FacialExpression}'.");
    }

    private const int ExpressionRenewMs = 1000;

    private static int _expressionAt;

    internal static void KeepExpression()
    {
        if (Draft is null || Native.GetGameTimer() - _expressionAt < ExpressionRenewMs)
        {
            return;
        }

        ApplyExpression();
    }

    internal static PedOverlayValue? Overlay(int slot) =>
        Draft?.Core.OverlayAt(slot) ?? Style?.OverlayAt(slot);

    internal static bool IsMale => Draft?.Core.IsMale ?? true;

    internal static int Step(int current, int count, bool left)
    {
        if (count <= 0)
        {
            return 0;
        }

        var next = current + (left ? -1 : 1);

        return next < 0 ? count - 1 : next >= count ? 0 : next;
    }

    internal static string Position(int index) => (index + 1).ToString(CultureInfo.InvariantCulture);

    internal static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    internal static string Resolve(MenuText text) => text.Resolve(Localizer.Current);
}

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players.Appearance;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterAppearanceRows
{
    private const int OpacitySteps = 10;

    private static readonly int[] Order =
    [
        PedHeadOverlays.Blemishes,
        PedHeadOverlays.Beard,
        PedHeadOverlays.Eyebrows,
        PedHeadOverlays.Ageing,
        PedHeadOverlays.Makeup,
        PedHeadOverlays.Blush,
        PedHeadOverlays.Complexion,
        PedHeadOverlays.SunDamage,
        PedHeadOverlays.Lipstick,
        PedHeadOverlays.MolesFreckles,
        PedHeadOverlays.ChestHair,
        PedHeadOverlays.BodyBlemishes,
    ];

    internal static void Attach(MenuBuilder menu)
    {
        CharacterCamera.AddButtons(menu);

        menu.Entries.Add(HairStyleRow());
        menu.Entries.Add(HairColourRow(Loc.CharacterCreator.HairColour, highlight: false));
        menu.Entries.Add(HairColourRow(Loc.CharacterCreator.HairHighlight, highlight: true));

        foreach (var slot in Order)
        {
            menu.Entries.Add(StyleRow(slot));
            menu.Entries.Add(OpacityRow(slot));

            if (PedHeadOverlays.ColourType(slot) != PedHeadOverlays.NoColour)
            {
                menu.Entries.Add(ColourRow(slot));
            }
        }

        menu.Entries.Add(EyeColourRow());

        menu.OnOpened = _ =>
        {
            CharacterCamera.Page = CameraFocus.Head;

            MenuRegistry.Refresh(menu.Menu);
        };
    }

    private static string NameKey(int slot) => slot switch
    {
        PedHeadOverlays.Blemishes => Loc.CharacterCreator.OverlayBlemishes,
        PedHeadOverlays.Beard => Loc.CharacterCreator.OverlayBeard,
        PedHeadOverlays.Eyebrows => Loc.CharacterCreator.OverlayEyebrows,
        PedHeadOverlays.Ageing => Loc.CharacterCreator.OverlayAgeing,
        PedHeadOverlays.Makeup => Loc.CharacterCreator.OverlayMakeup,
        PedHeadOverlays.Blush => Loc.CharacterCreator.OverlayBlush,
        PedHeadOverlays.Complexion => Loc.CharacterCreator.OverlayComplexion,
        PedHeadOverlays.SunDamage => Loc.CharacterCreator.OverlaySunDamage,
        PedHeadOverlays.Lipstick => Loc.CharacterCreator.OverlayLipstick,
        PedHeadOverlays.MolesFreckles => Loc.CharacterCreator.OverlayMolesFreckles,
        PedHeadOverlays.ChestHair => Loc.CharacterCreator.OverlayChestHair,
        _ => Loc.CharacterCreator.OverlayBodyBlemishes,
    };

    private static bool Available(int slot) => !PedHeadOverlays.IsMaleOnly(slot) || CharacterEdit.IsMale;

    private static CameraFocus Framing(int slot) =>
        slot is PedHeadOverlays.ChestHair or PedHeadOverlays.BodyBlemishes
            ? CameraFocus.UpperBody
            : CameraFocus.Head;

    private static MenuGate Editable(int slot) =>
        MenuGate.Permission(CharacterCreatorPermissions.Create) & MenuGate.When(() => Available(slot));

    #region Rows

    private static DynamicListEntry StyleRow(int slot)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(NameKey(slot)),
            Description = MenuText.Key(Loc.CharacterCreator.StyleRowDescription, ("name", MenuText.Key(NameKey(slot)))),
            LockedDescription = MenuText.Key(Loc.CharacterCreator.MaleOnly),
            Gate = Editable(slot),
            Behaviour = GateBehaviour.Lock,
            Configure = item => item.ItemData = Framing(slot),
            ReadValue = () => StyleValue(slot),

            Change = changing =>
            {
                if (CharacterEdit.Overlay(slot) is { } overlay)
                {
                    overlay.Style = CharacterEdit.Step(
                        overlay.Style, Native.GetNumHeadOverlayValues(slot), changing.Left);

                    CharacterEdit.ApplyOverlay(overlay);
                }

                return StyleValue(slot);
            },
        };
    }

    private static ListEntry OpacityRow(int slot)
    {
        return new ListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.OpacityRow, ("name", MenuText.Key(NameKey(slot)))),
            Description = MenuText.Key(Loc.CharacterCreator.OpacityRowDescription, ("name", MenuText.Key(NameKey(slot)))),
            LockedDescription = MenuText.Key(Loc.CharacterCreator.MaleOnly),
            Options = OpacityOptions(),
            Gate = Editable(slot),
            Behaviour = GateBehaviour.Lock,
            Configure = item =>
            {
                item.ItemData = Framing(slot);
                item.ShowOpacityPanel = true;
            },

            ReadSelectedIndex = () => CharacterEdit.Overlay(slot) is { } overlay
                ? Math.Clamp((int)Math.Round(overlay.Opacity * OpacitySteps), 0, OpacitySteps)
                : 0,

            OnIndexChanged = changed =>
            {
                if (CharacterEdit.Overlay(slot) is { } overlay)
                {
                    overlay.Opacity = changed.NewIndex / (float)OpacitySteps;

                    CharacterEdit.ApplyOverlay(overlay);
                }
            },
        };
    }

    private static ListEntry ColourRow(int slot)
    {
        var makeup = PedHeadOverlays.ColourType(slot) == PedHeadOverlays.MakeupPalette;

        return new ListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.ColourRow, ("name", MenuText.Key(NameKey(slot)))),
            Description = MenuText.Key(Loc.CharacterCreator.ColourRowDescription, ("name", MenuText.Key(NameKey(slot)))),
            LockedDescription = MenuText.Key(Loc.CharacterCreator.MaleOnly),
            Options = ColourOptions(makeup),
            Gate = Editable(slot),
            Behaviour = GateBehaviour.Lock,
            Configure = item =>
            {
                item.ItemData = Framing(slot);
                item.ShowColorPanel = true;
                item.ColorPanelColorType = Panel(makeup);
            },

            ReadSelectedIndex = () => CharacterEdit.Overlay(slot)?.Colour ?? 0,

            OnIndexChanged = changed =>
            {
                if (CharacterEdit.Overlay(slot) is { } overlay)
                {
                    overlay.Colour = changed.NewIndex;

                    CharacterEdit.ApplyOverlay(overlay);
                }
            },
        };
    }

    private static DynamicListEntry HairStyleRow()
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.HairStyle),
            Description = MenuText.Key(Loc.CharacterCreator.HairRowDescription),
            Gate = CharacterCreatorPermissions.Create,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadValue = HairValue,

            Change = changing =>
            {
                if (CharacterEdit.Style is { } style)
                {
                    style.HairStyle = CharacterEdit.Step(style.HairStyle, HairCount(), changing.Left);

                    CharacterEdit.ApplyHair();
                }

                return HairValue();
            },
        };
    }

    private static ListEntry HairColourRow(string name, bool highlight)
    {
        return new ListEntry
        {
            Text = MenuText.Key(name),
            Description = MenuText.Key(Loc.CharacterCreator.ColourRowDescription, ("name", MenuText.Key(name))),
            Options = ColourOptions(makeup: false),
            Gate = CharacterCreatorPermissions.Create,
            Configure = item =>
            {
                item.ItemData = CameraFocus.Head;
                item.ShowColorPanel = true;
                item.ColorPanelColorType = Panel(makeup: false);
            },

            ReadSelectedIndex = () => CharacterEdit.Style is { } style
                ? highlight ? style.HairHighlight : style.HairColour
                : 0,

            OnIndexChanged = changed =>
            {
                if (CharacterEdit.Style is not { } style)
                {
                    return;
                }

                if (highlight)
                {
                    style.HairHighlight = changed.NewIndex;
                }
                else
                {
                    style.HairColour = changed.NewIndex;
                }

                Native.SetPedHairColor(CharacterEdit.Ped, style.HairColour, style.HairHighlight);
            },
        };
    }

    private static DynamicListEntry EyeColourRow()
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.EyeColour),
            Description = MenuText.Key(Loc.CharacterCreator.EyeColourRowDescription),
            Gate = CharacterCreatorPermissions.Create,
            Configure = item => item.ItemData = CameraFocus.Head,
            ReadValue = EyeValue,

            Change = changing =>
            {
                if (CharacterEdit.Draft?.Core is { } core)
                {
                    core.EyeColour = CharacterEdit.Step(
                        core.EyeColour, CharacterDraft.EyeColourCount, changing.Left);

                    CharacterEdit.ApplyEyes();
                }

                return EyeValue();
            },
        };
    }

    #endregion

    #region Values

    private static string StyleValue(int slot) =>
        CharacterEdit.Overlay(slot) is { } overlay
            ? Option(Loc.CharacterCreator.StyleOption, overlay.Style)
            : CharacterEdit.Resolve(MenuText.Key(Loc.CharacterCreator.StyleNone));

    private static string HairValue() =>
        CharacterEdit.Style is { } style ? Option(Loc.CharacterCreator.StyleOption, style.HairStyle) : string.Empty;

    private static string EyeValue() =>
        CharacterEdit.Draft?.Core is { } core
            ? Option(Loc.CharacterCreator.EyeColourOption, core.EyeColour)
            : string.Empty;

    private static string Option(string key, int index) =>
        CharacterEdit.Resolve(MenuText.Key(key, ("number", MenuText.Literal(CharacterEdit.Position(index)))));

    private static int HairCount() =>
        Math.Max(1, Native.GetNumberOfPedDrawableVariations(CharacterEdit.Ped, PedComponentSlots.Hair));

    private static List<MenuText> ColourOptions(bool makeup)
    {
        var count = makeup ? Native.GetNumMakeupColors() : Native.GetNumHairColors();

        if (count <= 0)
        {
            Log.Warning(
                $"[Character] The game reported no {(makeup ? "makeup" : "hair")} colours, so that "
                + "row has nothing to offer.");

            count = 1;
        }

        var options = new List<MenuText>(count);

        for (var colour = 0; colour < count; colour++)
        {
            options.Add(MenuText.Key(
                Loc.CharacterCreator.ColourOption,
                ("number", MenuText.Literal(CharacterEdit.Position(colour)))));
        }

        return options;
    }

    private static List<MenuText> OpacityOptions()
    {
        var options = new List<MenuText>(OpacitySteps + 1);

        for (var step = 0; step <= OpacitySteps; step++)
        {
            options.Add(MenuText.Key(
                Loc.CharacterCreator.OpacityOption,
                ("percent", MenuText.Literal(CharacterEdit.Number(step * 100 / OpacitySteps)))));
        }

        return options;
    }

    private static MenuListItem.ColorPanelType Panel(bool makeup) =>
        makeup ? MenuListItem.ColorPanelType.Makeup : MenuListItem.ColorPanelType.Hair;

    #endregion
}

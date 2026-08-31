using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using vMenu.Enhanced.Menus.Players.Appearance.Torso;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;
using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Everything a player sees counts from one, because counting from zero on screen reads as an off by
// one mistake, and the game counts from zero underneath. The jump-to-a-number prompt asks in the
// rows' counting, so a player types back the number they are looking at.
public static class PedCustomizationRows
{
    [Flags]
    internal enum PedRows
    {
        Components = 1,

        Props = 2,

        Everything = Components | Props,
    }

    // Refilled on open rather than built once, because the player can change ped model in another menu
    // entirely and come back to rows describing a wardrobe that is no longer theirs. The scope is asked
    // for again each time because the collections menu points every one of its rows at this same child.
    internal static void Attach(
        MenuBuilder menu,
        Func<PedVariationScope> scope,
        PedRows show = PedRows.Everything,
        IReadOnlyList<int>? components = null,
        bool fitTorso = false)
    {
        menu.Keys.Add(new MenuKey
        {
            Name = "exactid",
            Description = MenuText.Key(Loc.PlayerAppearance.ExactIdBinding),
            DefaultKey = "LCONTROL",
            DefaultButton = "L1_INDEX",
            ShadowedControl = Control.Duck,
            Text = MenuText.Key(Loc.PlayerAppearance.ExactIdButton),
            Handler = (_, _) => _ = ExactIdForHighlightedAsync(menu, scope),
        });

        menu.AddRange(Rows(scope(), show, components, fitTorso));

        menu.OnOpened = _ => Refill(menu, Rows(scope(), show, components, fitTorso));
    }

    internal static IReadOnlyList<MenuEntry> Rows(
        PedVariationScope scope,
        PedRows show = PedRows.Everything,
        IReadOnlyList<int>? components = null,
        bool fitTorso = false)
    {
        var ped = Native.PlayerPedId();
        var rows = new List<MenuEntry>();

        DynamicListEntry? torsoRow = null;
        var redrawTorso = fitTorso ? () => RedrawTorso(scope, torsoRow) : (Action?)null;

        if (show.HasFlag(PedRows.Components))
        {
            if (fitTorso && !scope.IsCollection)
            {
                rows.Add(FitTorsoRow());
            }

            foreach (var slot in components ?? PedComponentSlots.All)
            {
                if (scope.DrawableCount(ped, slot) > 0)
                {
                    var row = ComponentRow(scope, slot, fitTorso, redrawTorso);

                    if (slot == PedComponentSlots.Torso)
                    {
                        torsoRow = row;
                    }

                    rows.Add(row);
                }
            }
        }

        var props = new List<MenuEntry>();

        if (show.HasFlag(PedRows.Props))
        {
            foreach (var slot in PedPropSlots.All)
            {
                if (scope.PropCount(ped, slot) > 0)
                {
                    props.Add(PropRow(scope, slot));
                }
            }
        }

        // The slot ids start counting again at zero for props, and two rows both labelled [0] with nothing
        // between them read as a bug.
        if (props.Count > 0)
        {
            if (rows.Count > 0)
            {
                rows.Add(Notice(Loc.PlayerAppearance.PropsHeading, Loc.PlayerAppearance.PropsHeadingDescription));
            }

            rows.AddRange(props);
        }

        // MenuAPI will not let a player back out of a menu with nothing in it, so an empty wardrobe needs a
        // row of its own rather than no rows at all.
        return rows.Count > 0
            ? rows
            : new List<MenuEntry> { Notice(Loc.PlayerAppearance.Empty, Loc.PlayerAppearance.EmptyDescription) };
    }

    private static ButtonEntry Notice(string text, string description) => new()
    {
        Text = MenuText.Key(text),
        Description = MenuText.Key(description),
    };

    // The handler fires for the menu rather than for a row, so the row it landed on has to be able to
    // say which slot it edits.
    internal sealed class SlotReference(int slot, bool component, bool fitTorso = false, Action? redrawTorso = null)
    {
        internal int Slot { get; } = slot;

        internal bool IsComponent { get; } = component;

        internal bool FitTorso { get; } = fitTorso;

        internal Action? RedrawTorso { get; } = redrawTorso;
    }

    private static async Task ExactIdForHighlightedAsync(MenuBuilder menu, Func<PedVariationScope> scope)
    {
        if (menu.Menu.GetCurrentMenuItem() is not MenuDynamicListItem item
            || !item.Enabled
            || item.ItemData is not SlotReference reference)
        {
            return;
        }

        await AskForExactIdAsync(
            scope(),
            reference.Slot,
            item,
            reference.IsComponent,
            reference.FitTorso,
            reference.RedrawTorso);
    }

    #region Component rows

    private static DynamicListEntry ComponentRow(
        PedVariationScope scope,
        int slot,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var fits = fitTorso && TorsoFit.Triggers(slot);

        return new DynamicListEntry
        {
            Text = MenuText.Key(PedComponentSlots.NameKey(slot)),
            Description = MenuText.From(() => ComponentDescription(scope, slot)),
            Gate = PlayerAppearancePermissions.Customize,
            Configure = item => item.ItemData = new SlotReference(slot, component: true, fits, redrawTorso),
            ReadValue = () => ComponentValue(scope, slot),

            // Applied as it scrolls, which is the only way to see what you are choosing.
            Change = changing =>
            {
                Shift(scope, slot, changing.Left, component: true, fits, redrawTorso);

                // A description is only rewritten on a refresh pass, and scrolling is not one, so the numbers
                // underneath would sit a step behind what the row is showing.
                changing.Item.Description = ComponentDescription(scope, slot);

                return ComponentValue(scope, slot);
            },

            OnSelected = selected => ComponentSelected(scope, slot, selected.Item, fits, redrawTorso),
        };
    }

    private static string ComponentValue(PedVariationScope scope, int slot)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentDrawable(ped, slot) is not { } drawable)
        {
            return Resolve(MenuText.Key(Loc.PlayerAppearance.ValueNone));
        }

        var texture = PedVariationScope.CurrentTexture(ped, slot);

        return Resolve(MenuText.Key(
            scope.IsUsable(ped, slot, drawable, texture)
                ? Loc.PlayerAppearance.Value
                : Loc.PlayerAppearance.ValueUnavailable,
            ("position", Position(drawable)),
            ("drawables", Number(scope.DrawableCount(ped, slot))),
            ("texturePosition", Position(texture))));
    }

    private static string ComponentDescription(PedVariationScope scope, int slot)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentDrawable(ped, slot) is not { } drawable)
        {
            return Elsewhere(PedComponentSlots.NameKey(slot), scope);
        }

        return Describe(
            PedComponentSlots.NameKey(slot),
            scope,
            drawable,
            scope.DrawableCount(ped, slot),
            PedVariationScope.CurrentTexture(ped, slot),
            scope.TextureCount(ped, slot, drawable));
    }

    private static void ComponentSelected(
        PedVariationScope scope,
        int slot,
        MenuDynamicListItem item,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentDrawable(ped, slot) is not { } drawable)
        {
            return;
        }

        var texture = NextTexture(
            PedVariationScope.CurrentTexture(ped, slot),
            scope.TextureCount(ped, slot, drawable));

        var before = TorsoFit.Before(ped, fitTorso);

        scope.SetComponent(ped, slot, drawable, texture, PedVariationScope.CurrentPalette(ped, slot));

        TorsoFit.Apply(before, slot, redrawTorso);

        // Selecting is not a refresh, so nothing re-reads the row on its own and the texture number would
        // sit on whatever it said before while the ped already wore the new one.
        Redraw(item, ComponentValue(scope, slot), ComponentDescription(scope, slot));
    }

    #endregion

    #region Prop rows

    private static DynamicListEntry PropRow(PedVariationScope scope, int slot)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(PedPropSlots.NameKey(slot)),
            Description = MenuText.From(() => PropDescription(scope, slot)),
            Gate = PlayerAppearancePermissions.Customize,
            Configure = item => item.ItemData = new SlotReference(slot, component: false),
            ReadValue = () => PropValue(scope, slot),

            Change = changing =>
            {
                Shift(scope, slot, changing.Left, component: false);

                changing.Item.Description = PropDescription(scope, slot);

                // Scrolling is how a helmet usually goes on, so this is where the hint is most likely to be needed.
                PedVisorHint.ShowIfHelmet(Native.PlayerPedId(), slot);

                return PropValue(scope, slot);
            },

            OnSelected = selected => PropSelected(scope, slot, selected.Item),
        };
    }

    private static string PropValue(PedVariationScope scope, int slot)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentProp(ped, slot) is not { } drawable)
        {
            return Resolve(MenuText.Key(Loc.PlayerAppearance.ValueNone));
        }

        return Resolve(MenuText.Key(
            Loc.PlayerAppearance.Value,
            ("position", Position(drawable)),
            ("drawables", Number(scope.PropCount(ped, slot))),
            ("texturePosition", Position(PedVariationScope.CurrentPropTexture(ped, slot)))));
    }

    private static string PropDescription(PedVariationScope scope, int slot)
    {
        var ped = Native.PlayerPedId();
        var name = PedPropSlots.NameKey(slot);

        if (scope.CurrentProp(ped, slot) is not { } drawable)
        {
            // Nothing worn and worn from elsewhere look the same on the row but are not the same thing, and a
            // player who cannot tell them apart will think the menu is broken.
            if (scope.IsCollection && PedVariationScope.GlobalProp(ped, slot) >= 0)
            {
                return Elsewhere(name, scope);
            }

            return Resolve(MenuText.Key(
                Loc.PlayerAppearance.PropRowDescriptionEmpty,
                ("slot", MenuText.Key(name)),
                ("drawables", Number(scope.PropCount(ped, slot)))));
        }

        return Describe(
            name,
            scope,
            drawable,
            scope.PropCount(ped, slot),
            PedVariationScope.CurrentPropTexture(ped, slot),
            scope.PropTextureCount(ped, slot, drawable));
    }

    private static void PropSelected(PedVariationScope scope, int slot, MenuDynamicListItem item)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentProp(ped, slot) is not { } drawable)
        {
            return;
        }

        var texture = NextTexture(
            PedVariationScope.CurrentPropTexture(ped, slot),
            scope.PropTextureCount(ped, slot, drawable));

        scope.SetProp(ped, slot, drawable, texture);

        Redraw(item, PropValue(scope, slot), PropDescription(scope, slot));

        PedVisorHint.ShowIfHelmet(ped, slot);
    }

    #endregion

    #region Shared behaviour

    // The slot name carries its own id in brackets, so the description does not repeat it.
    private static string Describe(
        string nameKey,
        PedVariationScope scope,
        int drawable,
        int drawables,
        int texture,
        int textures)
    {
        var name = MenuText.Key(nameKey);

        return Resolve(scope.IsCollection
            ? MenuText.Key(
                Loc.PlayerAppearance.RowDescriptionInCollection,
                ("slot", name),
                ("position", Position(drawable)),
                ("drawables", Number(drawables)),
                ("texturePosition", Position(texture)),
                ("textures", Number(textures)),
                ("collection", CollectionName(scope)))
            : MenuText.Key(
                Loc.PlayerAppearance.RowDescription,
                ("slot", name),
                ("position", Position(drawable)),
                ("drawables", Number(drawables)),
                ("texturePosition", Position(texture)),
                ("textures", Number(textures))));
    }

    private static string Elsewhere(string nameKey, PedVariationScope scope) =>
        Resolve(MenuText.Key(
            Loc.PlayerAppearance.RowDescriptionElsewhere,
            ("slot", MenuText.Key(nameKey)),
            ("collection", CollectionName(scope))));

    // One helper rather than legacy's four inline copies, two of which disagreed about whether the count
    // they compared against had already had one taken off it.
    private static int NextTexture(int current, int count) =>
        count <= 0 ? 0 : (Math.Max(current, 0) + 1) % count;

    // The current position is read off the ped every time rather than tracked alongside it, so another
    // resource changing the player's clothes cannot leave this row counting from a place the ped has not
    // been for a while. Props have one extra position at the front for wearing nothing, so they run from -1.
    private static void Shift(
        PedVariationScope scope,
        int slot,
        bool left,
        bool component,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = Native.PlayerPedId();
        var count = component ? scope.DrawableCount(ped, slot) : scope.PropCount(ped, slot);

        if (count <= 0)
        {
            return;
        }

        var first = component ? 0 : -1;
        var current = (component ? scope.CurrentDrawable(ped, slot) : scope.CurrentProp(ped, slot)) ?? first;
        var next = current + (left ? -1 : 1);

        // Wrapped by hand rather than with a modulo, because the range does not start at zero for a prop and
        // C# remainder keeps the sign of the left operand.
        if (next < first)
        {
            next = count - 1;
        }
        else if (next >= count)
        {
            next = first;
        }

        if (component)
        {
            var before = TorsoFit.Before(ped, fitTorso);

            scope.SetComponent(ped, slot, next, 0, PedVariationScope.CurrentPalette(ped, slot));

            TorsoFit.Apply(before, slot, redrawTorso);

            return;
        }

        if (next < 0)
        {
            PedVariationScope.ClearProp(ped, slot);

            return;
        }

        scope.SetProp(ped, slot, next, 0);
    }

    // Asked for as a position counting from one, matching what the row shows. Legacy asked for a position
    // in its own menu list, which for a prop had a "none" row at the front, so every prop number a player
    // typed landed one place away from the one they meant.
    private static async Task AskForExactIdAsync(
        PedVariationScope scope,
        int slot,
        MenuDynamicListItem item,
        bool component,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = Native.PlayerPedId();
        var count = component ? scope.DrawableCount(ped, slot) : scope.PropCount(ped, slot);

        if (count <= 0)
        {
            return;
        }

        var name = MenuText.Key(component ? PedComponentSlots.NameKey(slot) : PedPropSlots.NameKey(slot));

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(
                Loc.PlayerAppearance.ExactIdPrompt,
                ("slot", name),
                ("max", Number(count))),
            maxLength: 5);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        if (!int.TryParse(typed.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var position)
            || position < 1
            || position > count)
        {
            Notifications.Error(MenuText.Key(Loc.PlayerAppearance.ExactIdInvalid, ("max", Number(count))));

            return;
        }

        // Read again rather than reusing the handle from before the prompt: turning into another ped
        // mid-prompt is a slow enough thing to do that somebody will manage it.
        ped = Native.PlayerPedId();

        var drawable = position - 1;

        if (component)
        {
            var before = TorsoFit.Before(ped, fitTorso);

            scope.SetComponent(ped, slot, drawable, 0, PedVariationScope.CurrentPalette(ped, slot));

            TorsoFit.Apply(before, slot, redrawTorso);

            Redraw(item, ComponentValue(scope, slot), ComponentDescription(scope, slot));

            return;
        }

        scope.SetProp(ped, slot, drawable, 0);

        Redraw(item, PropValue(scope, slot), PropDescription(scope, slot));

        PedVisorHint.ShowIfHelmet(ped, slot);
    }

    private static void Redraw(MenuDynamicListItem item, string value, string description)
    {
        item.CurrentItem = value;
        item.Description = description;
    }

    private static void RedrawTorso(PedVariationScope scope, DynamicListEntry? torsoRow)
    {
        if (torsoRow?.Typed is not { } item)
        {
            return;
        }

        Redraw(
            item,
            ComponentValue(scope, PedComponentSlots.Torso),
            ComponentDescription(scope, PedComponentSlots.Torso));
    }

    private static CheckboxEntry FitTorsoRow() => new()
    {
        Text = MenuText.Key(Loc.CharacterCreator.FitTorso),
        Description = MenuText.Key(Loc.CharacterCreator.FitTorsoDescription),
        Gate = CharacterCreatorPermissions.Create,
        ReadState = () => TorsoFit.IsEnabled,
        OnChanged = changed => TorsoFit.SetEnabled(changed.Checked),
    };

    private static MenuText CollectionName(PedVariationScope scope) => scope.CollectionName.Length == 0
        ? MenuText.Key(Loc.PlayerAppearance.BaseCollection)
        : MenuText.Literal(scope.CollectionName);

    private static MenuText Position(int index) => Number(index + 1);

    private static MenuText Number(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));

    private static string Resolve(MenuText text) => text.Resolve(Localizer.Current);

    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves the
    // player's selection out from under them.
    private static void Refill(MenuBuilder builder, IReadOnlyList<MenuEntry> rows)
    {
        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

    #endregion
}

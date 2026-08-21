using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

namespace vMenu.Enhanced.Menus.Players.Appearance;

/// <summary>
/// The rows that dress a ped, built once and pointed at whichever slice of the wardrobe is wanted.
/// </summary>
/// <remarks>
/// Everything a player sees counts from one, because counting from zero on screen reads as an off by
/// one mistake. The game counts from zero underneath, so the two differ by one everywhere, and the
/// only place that matters is the jump-to-a-number prompt, which asks in the same counting the rows
/// use so a player types back the number they are looking at.
///
/// <para>
/// A body slot and a prop say the same thing about themselves, so they share one set of description
/// strings rather than keeping two copies that drift apart every time the wording changes.
/// </para>
/// </remarks>
public static class PedCustomizationRows
{
    /// <summary>Fills a menu and keeps it honest when the player comes back to it.</summary>
    /// <param name="scope">
    /// Asked again on every opening rather than taken once, because the collections menu points every
    /// one of its rows at this same child and only decides which collection it is on the way in.
    /// </param>
    // Refilled on open rather than built once, because the player can change ped model in another
    // menu entirely and come back to rows describing a wardrobe that is no longer theirs.
    internal static void Attach(MenuBuilder menu, Func<PedVariationScope> scope)
    {
        menu.InstructionalButtons.Add((Control.Duck, MenuText.Key(Loc.PlayerAppearance.ExactIdButton)));

        // Through MenuAPI rather than asking the control ourselves. It disables every control while a
        // menu is on screen, so IsControlPressed answers false for all of them and a check of our own
        // could never fire. The last argument disables it here too, so the player does not crouch.
        menu.Menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(
            Control.Duck,
            Menu.ControlPressCheckType.JUST_PRESSED,
            (_, _) => _ = ExactIdForHighlightedAsync(menu, scope),
            true));

        menu.AddRange(Rows(scope()));

        menu.OnOpened = _ => Refill(menu, Rows(scope()));
    }

    internal static IReadOnlyList<MenuEntry> Rows(PedVariationScope scope)
    {
        var ped = Native.PlayerPedId();

        // Half of what a freemode ped is lives in the character creator, which is not ported yet, so
        // dressing one here would leave it with a default grey face and no way to fix it.
        if (PedSpawning.IsWearingFreemode())
        {
            return [Notice(Loc.PlayerAppearance.Freemode, Loc.PlayerAppearance.FreemodeDescription)];
        }

        var rows = new List<MenuEntry>();

        foreach (var slot in PedComponentSlots.All)
        {
            if (scope.DrawableCount(ped, slot) > 0)
            {
                rows.Add(ComponentRow(scope, slot));
            }
        }

        var props = new List<MenuEntry>();

        foreach (var slot in PedPropSlots.All)
        {
            if (scope.PropCount(ped, slot) > 0)
            {
                props.Add(PropRow(scope, slot));
            }
        }

        // A heading between the two halves, because the slot ids start counting again at zero for
        // props and two rows both labelled [0] with nothing between them read as a bug.
        if (props.Count > 0)
        {
            if (rows.Count > 0)
            {
                rows.Add(Notice(Loc.PlayerAppearance.PropsHeading, Loc.PlayerAppearance.PropsHeadingDescription));
            }

            rows.AddRange(props);
        }

        // MenuAPI will not let a player back out of a menu with nothing in it, so an empty wardrobe
        // needs a row of its own rather than no rows at all.
        return rows.Count > 0
            ? rows
            : new List<MenuEntry> { Notice(Loc.PlayerAppearance.Empty, Loc.PlayerAppearance.EmptyDescription) };
    }

    private static ButtonEntry Notice(string text, string description) => new()
    {
        Text = MenuText.Key(text),
        Description = MenuText.Key(description),
    };

    /// <summary>Which slot a row edits, kept on the item so a control handler can find it.</summary>
    // The handler fires for the menu rather than for a row, so the row it landed on has to be able to
    // say what it is. ItemData is the escape hatch the framework keeps for exactly this.
    private sealed class SlotReference(int slot, bool component)
    {
        internal int Slot { get; } = slot;

        internal bool IsComponent { get; } = component;
    }

    private static async Task ExactIdForHighlightedAsync(MenuBuilder menu, Func<PedVariationScope> scope)
    {
        if (menu.Menu.GetCurrentMenuItem() is not MenuDynamicListItem item
            || !item.Enabled
            || item.ItemData is not SlotReference reference)
        {
            return;
        }

        await AskForExactIdAsync(scope(), reference.Slot, item, reference.IsComponent);
    }

    #region Component rows

    private static DynamicListEntry ComponentRow(PedVariationScope scope, int slot)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(PedComponentSlots.NameKey(slot)),
            Description = MenuText.From(() => ComponentDescription(scope, slot)),
            Gate = PlayerAppearancePermissions.Customize,
            Configure = item => item.ItemData = new SlotReference(slot, component: true),
            ReadValue = () => ComponentValue(scope, slot),

            // Applied as it scrolls, which is the only way to see what you are choosing.
            Change = changing =>
            {
                Shift(scope, slot, changing.Left, component: true);

                // A description is only rewritten on a refresh pass, and scrolling is not one, so the
                // numbers underneath would sit a step behind what the row is showing.
                changing.Item.Description = ComponentDescription(scope, slot);

                return ComponentValue(scope, slot);
            },

            OnSelected = selected => ComponentSelected(scope, slot, selected.Item),
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

    private static void ComponentSelected(PedVariationScope scope, int slot, MenuDynamicListItem item)
    {
        var ped = Native.PlayerPedId();

        if (scope.CurrentDrawable(ped, slot) is not { } drawable)
        {
            return;
        }

        var texture = NextTexture(
            PedVariationScope.CurrentTexture(ped, slot),
            scope.TextureCount(ped, slot, drawable));

        scope.SetComponent(ped, slot, drawable, texture, PedVariationScope.CurrentPalette(ped, slot));

        // Selecting is not a refresh, so nothing re-reads the row on its own and the texture number
        // would sit on whatever it said before while the ped already wore the new one.
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

                // Scrolling is how a helmet usually goes on, so this is the place the hint is most
                // likely to be needed.
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
            // Nothing worn and worn-from-elsewhere look the same on the row but are not the same
            // thing, and a player who cannot tell them apart will think the menu is broken.
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

    /// <summary>
    /// What a row says about itself. The same for a body slot and for a prop, because from the
    /// player's side they are the same thing: a list of variations, each with a list of textures.
    /// </summary>
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

    /// <summary>Said when the ped is wearing something this collection had no part in.</summary>
    private static string Elsewhere(string nameKey, PedVariationScope scope) =>
        Resolve(MenuText.Key(
            Loc.PlayerAppearance.RowDescriptionElsewhere,
            ("slot", MenuText.Key(nameKey)),
            ("collection", CollectionName(scope))));

    /// <summary>The next texture, wrapping. A slot with no textures rests on zero.</summary>
    // One helper rather than legacy's four inline copies, two of which disagreed about whether the
    // count they compared against had already had one taken off it.
    private static int NextTexture(int current, int count) =>
        count <= 0 ? 0 : (Math.Max(current, 0) + 1) % count;

    /// <summary>
    /// Steps a slot one place and puts the result on the ped.
    /// </summary>
    /// <remarks>
    /// The current position is read off the ped every time rather than tracked alongside it, so
    /// another resource changing the player's clothes cannot leave this row counting from a place
    /// the ped has not been for a while.
    ///
    /// <para>
    /// Props have one extra position at the front for wearing nothing, so their range runs from -1.
    /// Components have no such thing: every one of them is always showing something.
    /// </para>
    /// </remarks>
    private static void Shift(PedVariationScope scope, int slot, bool left, bool component)
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

        // Wrapped by hand rather than with a modulo, because the range does not start at zero for a
        // prop and C# remainder keeps the sign of the left operand.
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
            scope.SetComponent(ped, slot, next, 0, PedVariationScope.CurrentPalette(ped, slot));

            return;
        }

        if (next < 0)
        {
            PedVariationScope.ClearProp(ped, slot);

            return;
        }

        scope.SetProp(ped, slot, next, 0);
    }

    /// <summary>
    /// Asks which item to jump to, which is the only workable way through a slot with a thousand of
    /// them.
    /// </summary>
    /// <remarks>
    /// Asked for as a position counting from one, matching what the row shows, so a player types back
    /// the number they are looking at. Legacy asked for a position in its own menu list, which for a
    /// prop had a "none" row at the front, so every prop number a player typed landed one place away
    /// from the one they meant.
    /// </remarks>
    private static async Task AskForExactIdAsync(
        PedVariationScope scope,
        int slot,
        MenuDynamicListItem item,
        bool component)
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
            scope.SetComponent(ped, slot, drawable, 0, PedVariationScope.CurrentPalette(ped, slot));

            Redraw(item, ComponentValue(scope, slot), ComponentDescription(scope, slot));

            return;
        }

        scope.SetProp(ped, slot, drawable, 0);

        Redraw(item, PropValue(scope, slot), PropDescription(scope, slot));

        PedVisorHint.ShowIfHelmet(ped, slot);
    }

    /// <summary>Writes what changed straight onto the live row, since this is not a refresh.</summary>
    private static void Redraw(MenuDynamicListItem item, string value, string description)
    {
        item.CurrentItem = value;
        item.Description = description;
    }

    private static MenuText CollectionName(PedVariationScope scope) => scope.CollectionName.Length == 0
        ? MenuText.Key(Loc.PlayerAppearance.BaseCollection)
        : MenuText.Literal(scope.CollectionName);

    /// <summary>A place in the list, counting from one the way a person counts.</summary>
    private static MenuText Position(int index) => Number(index + 1);

    /// <summary>A number exactly as the game gives it, counting from zero.</summary>
    private static MenuText Number(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));

    private static string Resolve(MenuText text) => text.Resolve(Localizer.Current);

    // Rebuilding drops every item and MenuAPI puts the highlight back on the first one, which moves
    // the player's selection out from under them.
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

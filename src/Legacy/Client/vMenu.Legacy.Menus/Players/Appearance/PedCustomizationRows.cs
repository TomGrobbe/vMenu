using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Logging;

using vMenu.Enhanced.Menus.Players.Appearance.Torso;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;
using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

using vMenu.Legacy.Hosting;

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

        menu.OnOpened = _ =>
        {
            ClothingDiagnostics.Opened(scope(), menu.Menu.MenuTitle + " / " + menu.Menu.MenuSubtitle);

            menu.ReplaceEntries(Rows(scope(), show, components, fitTorso));
        };
    }

    internal static List<MenuEntry> Rows(
        PedVariationScope scope,
        PedRows show = PedRows.Everything,
        IReadOnlyList<int>? components = null,
        bool fitTorso = false)
    {
        var ped = API.PlayerPedId();
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
                rows.Add(new ButtonEntry(Loc.PlayerAppearance.PropsHeading));
            }

            rows.AddRange(props);
        }

        // MenuAPI will not let a player back out of a menu with nothing in it, so an empty wardrobe needs a
        // row of its own rather than no rows at all.
        return rows.Count > 0
            ? rows
            : [new ButtonEntry(Loc.PlayerAppearance.Empty)];
    }

    // The handler fires for the menu rather than for a row, so the row it landed on has to be able to
    // say which slot it edits.
    internal sealed class SlotReference(
        int slot,
        bool component,
        ShiftMemory memory,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        internal int Slot { get; } = slot;

        internal bool IsComponent { get; } = component;

        internal ShiftMemory Memory { get; } = memory;

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
            reference.Memory,
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
        var memory = new ShiftMemory();

        return new DynamicListEntry
        {
            Text = MenuText.Key(PedComponentSlots.NameKey(slot)),
            Description = MenuText.From(() => ComponentDescription(scope, slot, memory)),
            Gate = PlayerAppearancePermissions.Customize,
            Configure = item => item.ItemData = new SlotReference(slot, component: true, memory, fits, redrawTorso),
            ReadValue = () => ComponentValue(scope, slot, memory),

            // Applied as it scrolls, which is the only way to see what you are choosing.
            Change = changing =>
            {
                Shift(
                    scope,
                    slot,
                    changing.Left,
                    component: true,
                    memory,
                    fits,
                    redrawTorso);

                // A description is only rewritten on a refresh pass, and scrolling is not one, so the numbers
                // underneath would sit a step behind what the row is showing.
                changing.Item.Description = ComponentDescription(scope, slot, memory);

                return ComponentValue(scope, slot, memory);
            },

            OnSelected = selected => ComponentSelected(scope, slot, selected.Item, memory, fits, redrawTorso),
        };
    }

    private static string ComponentValue(PedVariationScope scope, int slot, ShiftMemory memory)
    {
        var ped = API.PlayerPedId();

        if (memory.Pending(Worn(scope, ped, slot, component: true)) is int asked)
        {
            return PendingValue(asked, scope.DrawableCount(ped, slot));
        }

        if (scope.CurrentDrawable(ped, slot) is not int drawable)
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

    private static string ComponentDescription(PedVariationScope scope, int slot, ShiftMemory memory)
    {
        var ped = API.PlayerPedId();
        var worn = Worn(scope, ped, slot, component: true);

        if (memory.Pending(worn) is int asked)
        {
            return Describe(
                PedComponentSlots.NameKey(slot),
                scope,
                asked,
                scope.DrawableCount(ped, slot),
                0,
                scope.TextureCount(ped, slot, asked));
        }

        if (scope.CurrentDrawable(ped, slot) is not int drawable)
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
        ShiftMemory memory,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = API.PlayerPedId();

        if (memory.Pending(Worn(scope, ped, slot, component: true)) is not null)
        {
            return;
        }

        if (scope.CurrentDrawable(ped, slot) is not int drawable)
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
        Redraw(item, ComponentValue(scope, slot, memory), ComponentDescription(scope, slot, memory));
    }

    #endregion

    #region Prop rows

    private static DynamicListEntry PropRow(PedVariationScope scope, int slot)
    {
        var memory = new ShiftMemory();

        return new DynamicListEntry
        {
            Text = MenuText.Key(PedPropSlots.NameKey(slot)),
            Description = MenuText.From(() => PropDescription(scope, slot, memory)),
            Gate = PlayerAppearancePermissions.Customize,
            Configure = item => item.ItemData = new SlotReference(slot, component: false, memory),
            ReadValue = () => PropValue(scope, slot, memory),

            Change = changing =>
            {
                Shift(
                    scope,
                    slot,
                    changing.Left,
                    component: false,
                    memory);

                changing.Item.Description = PropDescription(scope, slot, memory);

                // Scrolling is how a helmet usually goes on, so this is where the hint is most likely to be needed.
                PedVisorHint.ShowIfHelmet(API.PlayerPedId(), slot);

                return PropValue(scope, slot, memory);
            },

            OnSelected = selected => PropSelected(scope, slot, selected.Item, memory),
        };
    }

    private static string PropValue(PedVariationScope scope, int slot, ShiftMemory memory)
    {
        var ped = API.PlayerPedId();

        if (memory.Pending(Worn(scope, ped, slot, component: false)) is int asked)
        {
            return PendingValue(asked, scope.PropCount(ped, slot));
        }

        if (scope.CurrentProp(ped, slot) is not int drawable)
        {
            return Resolve(MenuText.Key(Loc.PlayerAppearance.ValueNone));
        }

        return Resolve(MenuText.Key(
            Loc.PlayerAppearance.Value,
            ("position", Position(drawable)),
            ("drawables", Number(scope.PropCount(ped, slot))),
            ("texturePosition", Position(PedVariationScope.CurrentPropTexture(ped, slot)))));
    }

    private static string PropDescription(PedVariationScope scope, int slot, ShiftMemory memory)
    {
        var ped = API.PlayerPedId();
        var name = PedPropSlots.NameKey(slot);
        var worn = Worn(scope, ped, slot, component: false);

        if (memory.Pending(worn) is int asked)
        {
            return Describe(
                name,
                scope,
                asked,
                scope.PropCount(ped, slot),
                0,
                scope.PropTextureCount(ped, slot, asked));
        }

        if (scope.CurrentProp(ped, slot) is not int drawable)
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

    private static void PropSelected(PedVariationScope scope, int slot, MenuDynamicListItem item, ShiftMemory memory)
    {
        var ped = API.PlayerPedId();

        if (memory.Pending(Worn(scope, ped, slot, component: false)) is not null)
        {
            return;
        }

        if (scope.CurrentProp(ped, slot) is not int drawable)
        {
            return;
        }

        var texture = NextTexture(
            PedVariationScope.CurrentPropTexture(ped, slot),
            scope.PropTextureCount(ped, slot, drawable));

        scope.SetProp(ped, slot, drawable, texture);

        Redraw(item, PropValue(scope, slot, memory), PropDescription(scope, slot, memory));

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

    // Props have one extra position at the front for wearing nothing, so they run from -1.
    private static void Shift(
        PedVariationScope scope,
        int slot,
        bool left,
        bool component,
        ShiftMemory memory,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = API.PlayerPedId();
        var count = component ? scope.DrawableCount(ped, slot) : scope.PropCount(ped, slot);

        if (count <= 0)
        {
            return;
        }

        var first = component ? 0 : -1;
        var worn = Worn(scope, ped, slot, component);

        // The game refuses some valid pieces, stepping from what is worn would ask for them forever.
        var current = memory.Pending(worn) is int asked && asked >= first && asked < count
            ? asked
            : worn;

        var next = Step(current, left, first, count);
        var skipped = 0;

        while (component && skipped < count && scope.IsGen9Exclusive(ped, slot, next))
        {
            next = Step(next, left, first, count);
            skipped++;
        }

        if (skipped >= count)
        {
            return;
        }

        if (component)
        {
            var before = TorsoFit.Before(ped, fitTorso);

            scope.SetComponent(ped, slot, next, 0, PedVariationScope.CurrentPalette(ped, slot));

            TorsoFit.Apply(before, slot, redrawTorso);
        }
        else if (next < 0)
        {
            PedVariationScope.ClearProp(ped, slot);
        }
        else
        {
            scope.SetProp(ped, slot, next, 0);
        }

        Remember(scope, ped, slot, component, memory, worn, next);

        ClothingDiagnostics.Pressed(scope, slot, component, left, count, worn, current, next, skipped);
    }

    // Wrapped by hand rather than with a modulo, because the range does not start at zero for a prop and
    // C# remainder keeps the sign of the left operand.
    private static int Step(int current, bool left, int first, int count)
    {
        var next = current + (left ? -1 : 1);

        if (next < first)
        {
            return count - 1;
        }

        return next >= count ? first : next;
    }

    private static int Worn(PedVariationScope scope, int ped, int slot, bool component) => component
        ? scope.CurrentDrawable(ped, slot) ?? 0
        : scope.CurrentProp(ped, slot) ?? -1;

    private static void Remember(
        PedVariationScope scope,
        int ped,
        int slot,
        bool component,
        ShiftMemory memory,
        int worn,
        int asked)
    {
        if (Worn(scope, ped, slot, component) == asked)
        {
            memory.Worn = null;

            return;
        }

        memory.Worn = worn;
        memory.Asked = asked;
    }

    private static string PendingValue(int asked, int drawables) =>
        Resolve(MenuText.Key(
            Loc.PlayerAppearance.Value,
            ("position", Position(asked)),
            ("drawables", Number(drawables)),
            ("texturePosition", Position(0))));

    // Asked for as a position counting from one, matching what the row shows. Legacy asked for a position
    // in its own menu list, which for a prop had a "none" row at the front, so every prop number a player
    // typed landed one place away from the one they meant.
    private static async Task AskForExactIdAsync(
        PedVariationScope scope,
        int slot,
        MenuDynamicListItem item,
        bool component,
        ShiftMemory memory,
        bool fitTorso = false,
        Action? redrawTorso = null)
    {
        var ped = API.PlayerPedId();
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
        ped = API.PlayerPedId();

        var drawable = position - 1;
        var worn = Worn(scope, ped, slot, component);

        if (component)
        {
            if (scope.IsGen9Exclusive(ped, slot, drawable))
            {
                Notifications.Error(MenuText.Key(Loc.PlayerAppearance.ExactIdGen9, ("id", Number(position))));

                return;
            }

            var before = TorsoFit.Before(ped, fitTorso);

            scope.SetComponent(ped, slot, drawable, 0, PedVariationScope.CurrentPalette(ped, slot));

            TorsoFit.Apply(before, slot, redrawTorso);

            Remember(
                scope,
                ped,
                slot,
                component,
                memory,
                worn,
                drawable);

            Redraw(item, ComponentValue(scope, slot, memory), ComponentDescription(scope, slot, memory));

            return;
        }

        scope.SetProp(ped, slot, drawable, 0);

        Remember(
            scope,
            ped,
            slot,
            component,
            memory,
            worn,
            drawable);

        Redraw(item, PropValue(scope, slot, memory), PropDescription(scope, slot, memory));

        PedVisorHint.ShowIfHelmet(ped, slot);
    }

    internal sealed class ShiftMemory
    {
        internal int? Worn { get; set; }

        internal int Asked { get; set; }

        internal int? Pending(int worn)
        {
            // A piece that loaded late is no longer pending, even if the old one comes back later.
            if (worn == Asked)
            {
                Worn = null;
            }

            return Worn == worn && Asked != worn ? Asked : null;
        }
    }

    private static void Redraw(MenuDynamicListItem item, string value, string description)
    {
        item.CurrentItem = value;
        item.Description = description;
    }

    private static void RedrawTorso(PedVariationScope scope, DynamicListEntry? torsoRow)
    {
        if (torsoRow?.Typed is not { ItemData: SlotReference reference } item)
        {
            return;
        }

        Redraw(
            item,
            ComponentValue(scope, PedComponentSlots.Torso, reference.Memory),
            ComponentDescription(scope, PedComponentSlots.Torso, reference.Memory));
    }

    private static CheckboxEntry FitTorsoRow() => new(Loc.CharacterCreator.FitTorso)
    {
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

    #endregion
}

using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Menus.Players.Appearance;

using PlayerAppearancePermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerAppearance;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Changes the clothes and props on the ped the player is already wearing.
/// </summary>
/// <remarks>
/// Two ways into the same wardrobe. Ped Customization walks everything the ped owns, which is what
/// you want when you are looking for the next hat. Ped Collections groups the same clothes by the
/// game update that added them, which is what you want when you know roughly when something came
/// out. Both are built by <see cref="PedCustomizationRows"/> from one row builder.
///
/// <para>
/// The collections share one child menu rather than having one each. There can be dozens of them,
/// they are runtime data, and MenuAPI cannot take a menu back out once it has been added.
/// </para>
/// </remarks>
[VMenu(
    TitleKey = Loc.PlayerAppearance.Title,
    SubtitleKey = Loc.PlayerAppearance.Subtitle,
    DescriptionKey = Loc.PlayerAppearance.LinkDescription,
    Permission = PlayerAppearancePermissions.Menu)]
public sealed class PlayerAppearanceMenu : MenuDefinition
{
    /// <summary>In the order <see cref="ClothingGlow"/> declares, so the index is the value.</summary>
    // An array, not a List. A collection expression that fills a List compiles down to
    // CollectionsMarshal.SetCount, which the client sandbox refuses, and it throws while the type is
    // being initialised rather than where it is used, so the stack trace points nowhere useful.
    private static readonly MenuText[] GlowOptions =
    [
        MenuText.Key(Loc.PlayerAppearance.GlowOff),
        MenuText.Key(Loc.PlayerAppearance.GlowSolid),
        MenuText.Key(Loc.PlayerAppearance.GlowFade),
        MenuText.Key(Loc.PlayerAppearance.GlowFlash),
    ];

    private DetachedMenu? _collectionMenu;

    private PedCollection? _collection;

    /// <summary>
    /// The walking styles have to be here before the rows are built, because this menu owns child
    /// menus and MenuAPI cannot take one of those back out, so it cannot be rebuilt later.
    /// </summary>
    public override async Task PrepareAsync() => await WalkingStyleSync.WaitForFirstAsync();

    protected override void Build(MenuBuilder menu)
    {
        _collectionMenu = menu.AddDetachedMenu(
            MenuText.From(CollectionTitle),
            MenuText.Key(Loc.PlayerAppearance.CollectionSubtitle),
            child => PedCustomizationRows.Attach(child, () => PedVariationScope.ForCollection(CollectionName())),
            PlayerAppearancePermissions.Customize);

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.Customize),
            Description = MenuText.Key(Loc.PlayerAppearance.CustomizeDescription),
            MenuSubtitle = MenuText.Key(Loc.PlayerAppearance.CustomizeSubtitle),
            Gate = PlayerAppearancePermissions.Customize,
            Build = child => PedCustomizationRows.Attach(child, () => PedVariationScope.Global),
        });

        // Locked rather than hidden while the ped has only the base game's clothes, which is nearly
        // every ped that is not one of the online character models. Grouping one collection by the
        // update it came from just repeats the page above, and a row that quietly disappears leaves
        // a player wondering whether they imagined it.
        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.Collections),
            Description = MenuText.Key(Loc.PlayerAppearance.CollectionsDescription),
            LockedDescription = MenuText.Key(Loc.PlayerAppearance.CollectionsLocked),
            MenuSubtitle = MenuText.Key(Loc.PlayerAppearance.CollectionsSubtitle),
            Gate = MenuGate.Permission(PlayerAppearancePermissions.Customize) & MenuGate.When(HasSeveralCollections),
            Behaviour = GateBehaviour.Lock,
            Build = BuildCollectionList,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.Random),
            Description = MenuText.Key(Loc.PlayerAppearance.RandomDescription),
            Gate = PlayerAppearancePermissions.Customize,
            OnSelected = _ => Randomise(),
        });

        menu.Entries.Add(WalkingStyleRow());

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.Glow),
            Description = MenuText.Key(Loc.PlayerAppearance.GlowDescription),
            Options = GlowOptions,
            Gate = PlayerAppearancePermissions.IlluminatedClothing,
            ReadSelectedIndex = () => (int)PedIlluminatedClothing.Style,

            // On scroll, unlike the walking style, because nothing has to load and seeing it change
            // as you go is the whole point of a choice about how something looks.
            OnIndexChanged = changed => PedIlluminatedClothing.Style = (ClothingGlow)changed.NewIndex,
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.Reset),
            Description = MenuText.Key(Loc.PlayerAppearance.ResetDescription),
            ConfirmationDescription = MenuText.Key(Loc.PlayerAppearance.ResetConfirm),
            Gate = PlayerAppearancePermissions.Customize,
            OnConfirmed = _ => Reset(),
        });

        menu.OnOpened = _ =>
        {
            // The hint is worth saying once per visit, not once per session, so a player who forgot
            // it half an hour ago is told again the next time they put a helmet on.
            PedVisorHint.Reset();

            // Whether the collections row is open depends on the ped, and the player changes that in
            // another menu entirely. Gates are only re-evaluated on a refresh, and nothing else would
            // ask for one, so the row would still be locked from whoever they used to be.
            MenuRegistry.RefreshAll();
        };
    }

    /// <summary>
    /// The collections this ped has clothes in, read on every opening.
    /// </summary>
    // Read on open rather than once, because which collections exist depends entirely on which ped
    // the player is wearing, and they can change that from another menu between visits.
    private void BuildCollectionList(MenuBuilder menu)
    {
        menu.AddRange(CollectionRows());

        menu.OnOpened = _ => Refill(menu, CollectionRows());
    }

    private IReadOnlyList<MenuEntry> CollectionRows()
    {
        var rows = new List<MenuEntry>();

        foreach (var collection in PedCollections.All(Native.PlayerPedId()))
        {
            var current = collection;

            rows.Add(new ButtonEntry
            {
                Text = Name(current),
                Label = MenuText.Literal($"#{current.Index.ToString(CultureInfo.InvariantCulture)}"),
                Description = MenuText.Key(
                    Loc.PlayerAppearance.CollectionRowDescription,
                    ("name", Name(current))),
                Gate = PlayerAppearancePermissions.Customize,
                OnSelected = _ =>
                {
                    _collection = current;

                    _collectionMenu?.Open();
                },
            });
        }

        return rows.Count > 0
            ? rows
            : new List<MenuEntry>
            {
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.PlayerAppearance.Empty),
                    Description = MenuText.Key(Loc.PlayerAppearance.EmptyDescription),
                },
            };
    }

    /// <summary>
    /// How the player walks, from whatever the server owner put in <c>config/walking-styles.json</c>.
    /// </summary>
    /// <remarks>
    /// A plain list rather than a dynamic one, because there are only ever a handful of these and the
    /// row reads better with the names spelled out. Applied on enter rather than on scroll: each one
    /// has to load its animations first, and stepping past five of them would ask for five loads.
    /// </remarks>
    private static ListEntry WalkingStyleRow()
    {
        // Read once, which is why PrepareAsync waits for the list first.
        var styles = WalkingStyleSync.Styles;

        // The first choice is always the ped's own walk, so a player can always get back to it even
        // when the owner offers nothing else.
        var options = new List<MenuText>(styles.Count + 1) { MenuText.Key(Loc.PlayerAppearance.WalkingStyleNormal) };

        var clipsets = new List<string>(styles.Count + 1) { string.Empty };

        foreach (var style in styles)
        {
            options.Add(MenuText.Literal(style.Label));
            clipsets.Add(style.Clipset);
        }

        return new ListEntry
        {
            Text = MenuText.Key(Loc.PlayerAppearance.WalkingStyle),
            Description = MenuText.Key(Loc.PlayerAppearance.WalkingStyleDescription),
            Options = options,
            Gate = PlayerAppearancePermissions.WalkingStyle,
            ReadSelectedIndex = () => Math.Max(0, IndexOf(clipsets, PedWalkingStyle.Current)),
            OnSelectedAsync = selected => ApplyWalkAsync(options, clipsets, selected.SelectedIndex),
        };
    }

    private static async Task ApplyWalkAsync(List<MenuText> options, List<string> clipsets, int index)
    {
        if (index < 0 || index >= clipsets.Count)
        {
            return;
        }

        var label = options[index];

        if (await PedWalkingStyle.ApplyAsync(clipsets[index]))
        {
            Notifications.Success(MenuText.Key(Loc.PlayerAppearance.WalkingStyleApplied, ("style", label)));

            return;
        }

        // Nothing loaded under that name for this ped, which is a real thing that happens: a walk
        // animated for a person has nothing to give a coyote.
        Notifications.Warning(MenuText.Key(Loc.PlayerAppearance.WalkingStyleUnavailable, ("style", label)));
    }

    // By hand rather than List.IndexOf, which reaches for EqualityComparer<string>.Default and the
    // client sandbox refuses to load it.
    private static int IndexOf(List<string> values, string value)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Whether grouping this ped's clothes by update would tell the player anything.
    /// </summary>
    // A collection is the content pack a piece of clothing came from, not a preset outfit. Nearly
    // every ped only has the base game's, because DLC clothing was almost all made for the online
    // character models, and for those peds this page would just repeat Ped Customization.
    private static bool HasSeveralCollections() => PedCollections.All(Native.PlayerPedId()).Count > 1;

    /// <summary>
    /// Hands the ped over to the game's own idea of an outfit.
    /// </summary>
    // The game holds, per ped, which combinations of pieces belong together, and this is the only way
    // to reach that. Picking twelve slots at random ourselves would put a police hat on a hiker.
    private static void Randomise()
    {
        var ped = Native.PlayerPedId();

        // The second argument asks the game to keep the combination coherent rather than picking each
        // slot on its own.
        Native.SetPedRandomComponentVariation(ped, 0);
        Native.SetPedRandomProps(ped);

        Notifications.Success(MenuText.Key(Loc.PlayerAppearance.RandomDone));
    }

    private static void Reset()
    {
        var ped = Native.PlayerPedId();

        Native.SetPedDefaultComponentVariation(ped);
        Native.ClearAllPedProps(ped, false);

        Notifications.Success(MenuText.Key(Loc.PlayerAppearance.ResetDone));
    }

    private static MenuText Name(PedCollection collection) => collection.IsBaseGame
        ? MenuText.Key(Loc.PlayerAppearance.BaseCollection)
        : MenuText.Literal(collection.Name);

    private string CollectionName() => _collection?.Name ?? string.Empty;

    private string CollectionTitle() => _collection is { IsBaseGame: false } collection
        ? collection.Name
        : Localizer.Current.Get(Loc.PlayerAppearance.BaseCollection);

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
}

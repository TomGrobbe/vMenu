using System.Globalization;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using CharacterCreatorPermissions = vMenu.Enhanced.Data.Permissions.Menus.CharacterCreator;

namespace vMenu.Enhanced.Menus.Players.Character;

internal static class CharacterTattooRows
{
    private static readonly (TattooZone Zone, string Name)[] Zones =
    [
        (TattooZone.Hair, Loc.CharacterCreator.TattooHair),
        (TattooZone.Head, Loc.CharacterCreator.TattooHead),
        (TattooZone.Torso, Loc.CharacterCreator.TattooTorso),
        (TattooZone.LeftArm, Loc.CharacterCreator.TattooLeftArm),
        (TattooZone.RightArm, Loc.CharacterCreator.TattooRightArm),
        (TattooZone.LeftLeg, Loc.CharacterCreator.TattooLeftLeg),
        (TattooZone.RightLeg, Loc.CharacterCreator.TattooRightLeg),
        (TattooZone.Badge, Loc.CharacterCreator.TattooBadges),
        (TattooZone.Addon, Loc.CharacterCreator.TattooAddons),
    ];

    internal static void Attach(MenuBuilder menu)
    {

        menu.Keys.Add(new MenuKey
        {
            Name = "exactid",
            Description = MenuText.Key(Loc.PlayerAppearance.ExactIdBinding),
            DefaultKey = "LCONTROL",
            DefaultButton = "L1_INDEX",
            ShadowedControl = Control.Duck,
            Text = MenuText.Key(Loc.CharacterCreator.TattooExactIdButton),
            Handler = (_, _) => _ = JumpToNumberAsync(menu),
        });

        foreach (var (zone, name) in Zones)
        {
            if (zone == TattooZone.Addon && !TattooCatalogue.HasAddons)
            {
                continue;
            }

            menu.Entries.Add(Row(zone, name));
        }

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.CharacterCreator.TattooRemoveAll),
            Description = MenuText.Key(Loc.CharacterCreator.TattooRemoveAllDescription),
            ConfirmationDescription = MenuText.Key(Loc.CharacterCreator.TattooRemoveAllConfirm),
            Gate = CharacterCreatorPermissions.Create,
            OnConfirmed = _ => RemoveAll(),
        });

        menu.OnOpened = _ =>
        {
            CharacterCamera.Page = CameraFocus.FullBody;

            CharacterEdit.ApplyTattoos();
        };

        menu.OnClosed = _ => CharacterEdit.ApplyTattoos();
    }

    private static DynamicListEntry Row(TattooZone zone, string name)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(name),
            Description = MenuText.Key(
                zone == TattooZone.Badge
                    ? Loc.CharacterCreator.BadgeListDescription
                    : Loc.CharacterCreator.TattooListDescription),
            Gate = CharacterCreatorPermissions.Create,
            Configure = item => item.ItemData = new ZoneReference(zone),
            ReadValue = () => Value(zone, Cursor(zone)),

            Change = changing =>
            {
                var available = Available(zone);

                if (available.Count == 0)
                {
                    return Value(zone, 0);
                }

                var next = CharacterEdit.Step(Cursor(zone), available.Count, changing.Left);

                SetCursor(zone, next);
                Preview(zone, available[next]);

                return Value(zone, next);
            },

            OnSelected = selected => Toggle(zone, selected.Item),
        };
    }

    private static readonly Dictionary<int, int> Cursors = [];

    private static int Cursor(TattooZone zone) => Cursors.TryGetValue((int)zone, out var index) ? index : 0;

    private static void SetCursor(TattooZone zone, int index) => Cursors[(int)zone] = index;

    private static List<Tattoo> Available(TattooZone zone) => TattooCatalogue.Zone(zone, CharacterEdit.IsMale);

    private static void Preview(TattooZone zone, Tattoo tattoo)
    {
        CharacterEdit.ApplyTattoos();

        if (!IsWorn(zone, tattoo))
        {
            Native.AddPedDecorationFromHashes(CharacterEdit.Ped, tattoo.CollectionHash, tattoo.NameHash);
        }
    }

    private static void Toggle(TattooZone zone, MenuDynamicListItem item)
    {
        var available = Available(zone);

        if (available.Count == 0 || CharacterEdit.Draft?.Core is not { } core)
        {
            return;
        }

        var index = Math.Clamp(Cursor(zone), 0, available.Count - 1);
        var tattoo = available[index];
        var worn = FreemodeReader.List(core.Tattoos, zone);

        var already = Remove(worn, tattoo);

        if (!already)
        {
            worn.Add(new TattooRef { Collection = tattoo.Collection, Name = tattoo.Name });
        }

        CharacterEdit.ApplyTattoos();

        item.CurrentItem = Value(zone, index);

        Notifications.Success(
            MenuText.Key(
                already ? Loc.CharacterCreator.TattooRemoved : Loc.CharacterCreator.TattooAdded,
                ("name", MenuText.Literal(Value(zone, index)))),
            Notifications.SpawnDurationMs);
    }

    private static void RemoveAll()
    {
        if (CharacterEdit.Draft?.Core is not { } core)
        {
            return;
        }

        core.Tattoos.Clear();

        CharacterEdit.ApplyTattoos();

        Notifications.Success(
            MenuText.Key(Loc.CharacterCreator.TattooRemoveAllDone),
            Notifications.SpawnDurationMs);
    }

    private static async Task JumpToNumberAsync(MenuBuilder menu)
    {
        if (menu.Menu.GetCurrentMenuItem() is not MenuDynamicListItem item
            || !item.Enabled
            || item.ItemData is not ZoneReference reference)
        {
            return;
        }

        var available = Available(reference.Zone);

        if (available.Count == 0)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(
                Loc.CharacterCreator.TattooExactIdPrompt,
                ("max", MenuText.Literal(CharacterEdit.Number(available.Count)))),
            maxLength: 5);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        if (!int.TryParse(typed.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var position)
            || position < 1
            || position > available.Count)
        {
            Notifications.Error(MenuText.Key(
                Loc.CharacterCreator.TattooExactIdInvalid,
                ("max", MenuText.Literal(CharacterEdit.Number(available.Count)))));

            return;
        }

        var index = position - 1;

        SetCursor(reference.Zone, index);
        Preview(reference.Zone, available[index]);

        item.CurrentItem = Value(reference.Zone, index);
    }

    private static string Value(TattooZone zone, int index)
    {
        var available = Available(zone);

        if (available.Count == 0)
        {
            return CharacterEdit.Resolve(MenuText.Key(
                TattooCatalogue.HasLoaded && TattooCatalogue.Zone(zone).Count > 0
                    ? Loc.CharacterCreator.TattooZoneEmpty
                    : Loc.CharacterCreator.TattooFileEmpty));
        }

        index = Math.Clamp(index, 0, available.Count - 1);

        var tattoo = available[index];
        var number = MenuText.Literal(CharacterEdit.Position(index));
        var total = MenuText.Literal(CharacterEdit.Number(available.Count));

        var text = tattoo.Label.Length > 0
            ? MenuText.Key(
                Loc.CharacterCreator.TattooOptionNamed,
                ("name", MenuText.Literal(tattoo.Label)),
                ("number", number),
                ("total", total))
            : MenuText.Key(NumberedKey(zone), ("number", number), ("total", total));

        var value = CharacterEdit.Resolve(text);

        return IsWorn(zone, tattoo)
            ? value + " " + CharacterEdit.Resolve(MenuText.Key(Loc.CharacterCreator.TattooWorn))
            : value;
    }

    private static string NumberedKey(TattooZone zone) => zone switch
    {
        TattooZone.Badge => Loc.CharacterCreator.BadgeOption,
        TattooZone.Addon => Loc.CharacterCreator.AddonOption,
        _ => Loc.CharacterCreator.TattooOption,
    };

    private static bool IsWorn(TattooZone zone, Tattoo tattoo)
    {
        if (CharacterEdit.Draft?.Core is not { } core)
        {
            return false;
        }

        foreach (var worn in FreemodeReader.List(core.Tattoos, zone))
        {
            if (Matches(worn, tattoo))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Remove(List<TattooRef> worn, Tattoo tattoo)
    {
        for (var index = 0; index < worn.Count; index++)
        {
            if (Matches(worn[index], tattoo))
            {
                worn.RemoveAt(index);

                return true;
            }
        }

        return false;
    }

    private static bool Matches(TattooRef worn, Tattoo tattoo) =>
        string.Equals(worn.Name, tattoo.Name, StringComparison.OrdinalIgnoreCase)
        && string.Equals(worn.Collection, tattoo.Collection, StringComparison.OrdinalIgnoreCase);

    private sealed class ZoneReference(TattooZone zone) : ICameraFraming
    {
        internal TattooZone Zone { get; } = zone;

        public CameraFocus Framing => Zone switch
        {
            TattooZone.Hair or TattooZone.Head => CameraFocus.Head,
            TattooZone.LeftArm or TattooZone.RightArm => CameraFocus.FullArms,
            TattooZone.LeftLeg or TattooZone.RightLeg => CameraFocus.LowerBody,
            TattooZone.Torso or TattooZone.Badge => CameraFocus.UpperBody,
            _ => CameraFocus.FullBody,
        };
    }
}

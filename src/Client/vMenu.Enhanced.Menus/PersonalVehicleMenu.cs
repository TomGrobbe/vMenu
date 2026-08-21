using MenuAPI;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.Personal;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PersonalVehicle.Title,
    SubtitleKey = Loc.PersonalVehicle.Subtitle,
    DescriptionKey = Loc.PersonalVehicle.LinkDescription,
    Permission = PersonalVehiclePermissions.Menu)]
public sealed class PersonalVehicleMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.SetCurrent),
            Description = MenuText.Key(Loc.PersonalVehicle.SetCurrentDescription),
            OnSelectedAsync = _ => PersonalVehicle.MarkCurrentAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Status),
            Description = MenuText.From(Status),
            Label = MenuText.From(StatusLabel),
            ReadEnabled = static () => false,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.ShowBlip),
            Description = MenuText.Key(Loc.PersonalVehicle.ShowBlipDescription),
            Gate = PersonalVehiclePermissions.Blip,
            ReadState = static () => PersonalVehicle.BlipWanted,
            OnChanged = changed => PersonalVehicle.SetBlipEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Waypoint),
            Description = MenuText.Key(Loc.PersonalVehicle.WaypointDescription),
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelected = _ => PersonalVehicle.SetWaypoint(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Kick),
            Description = MenuText.Key(Loc.PersonalVehicle.KickDescription),
            Gate = PersonalVehiclePermissions.Kick,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.KickOccupantsAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Delete),
            Description = MenuText.Key(Loc.PersonalVehicle.DeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.PersonalVehicle.DeleteConfirm),
            Gate = PersonalVehiclePermissions.Delete,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnConfirmedAsync = _ => PersonalVehicle.DeleteAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Forget),
            Description = MenuText.Key(Loc.PersonalVehicle.ForgetDescription),
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.ForgetAsync(),
        });

        Follow(menu);
    }

    private static void Follow(MenuBuilder builder)
    {
        Menu? live = null;

        void OnChanged()
        {
            if (live is { } menu)
            {
                MenuRegistry.Refresh(menu);
            }
        }

        void OnVehicleChanged(VehicleChanged _) => OnChanged();

        builder.OnOpened = opened =>
        {
            live = opened.Menu;

            PersonalVehicle.Changed -= OnChanged;
            PersonalVehicle.Changed += OnChanged;

            LocalVehicleTicks.VehicleChanged -= OnVehicleChanged;
            LocalVehicleTicks.VehicleChanged += OnVehicleChanged;

            MenuRegistry.Refresh(opened.Menu);
        };

        builder.OnClosed = _ =>
        {
            live = null;

            PersonalVehicle.Changed -= OnChanged;
            LocalVehicleTicks.VehicleChanged -= OnVehicleChanged;
        };
    }

    private static string StatusLabel() =>
        PersonalVehicle.IsMarked
            ? VehicleSpawning.DisplayName(PersonalVehicle.Model)
            : Localizer.Current.Get(Loc.PersonalVehicle.StatusNone);

    private static string Status()
    {
        if (!PersonalVehicle.IsMarked)
        {
            return Localizer.Current.Get(Loc.PersonalVehicle.StatusNoneDescription);
        }

        if (!PersonalVehicle.InRange)
        {
            return Localizer.Current.Get(Loc.PersonalVehicle.StatusOtherWorld);
        }

        var occupants = PersonalVehicle.Occupants;

        if (occupants.Count == 0)
        {
            return Localizer.Current.Get(Loc.PersonalVehicle.StatusEmpty);
        }

        return Localizer.Current.Get(Loc.PersonalVehicle.StatusOccupied)
            .Replace("{players}", string.Join(", ", occupants));
    }
}

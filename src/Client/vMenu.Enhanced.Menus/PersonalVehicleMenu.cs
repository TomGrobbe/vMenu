using MenuAPI;

using vMenu.Enhanced.Data.VehicleData;
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

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Lock),
            Description = MenuText.Key(Loc.PersonalVehicle.LockDescription),
            Gate = PersonalVehiclePermissions.Lock,
            ReadState = static () => PersonalVehicle.IsLocked,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnChangedAsync = changed => PersonalVehicle.SetLockedAsync(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Engine),
            Description = MenuText.Key(Loc.PersonalVehicle.EngineDescription),
            Gate = PersonalVehiclePermissions.Engine,
            ReadState = static () => PersonalVehicle.IsEngineRunning,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnChangedAsync = changed => PersonalVehicle.SetEngineAsync(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Lights),
            Description = MenuText.Key(Loc.PersonalVehicle.LightsDescription),
            Gate = PersonalVehiclePermissions.Lights,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            Options =
            [
                MenuText.Key(Loc.PersonalVehicle.LightsAutomatic),
                MenuText.Key(Loc.PersonalVehicle.LightsOff),
                MenuText.Key(Loc.PersonalVehicle.LightsOn),
            ],
            OnSelectedAsync = selected => PersonalVehicle.SetLightsAsync(LightState(selected.SelectedIndex)),
        });

        menu.Entries.Add(SubmenuEntry.For(new PersonalVehicleDoorsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new PersonalVehicleWindowsMenu()));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Horn),
            Description = MenuText.Key(Loc.PersonalVehicle.HornDescription),
            Gate = PersonalVehiclePermissions.Horn,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.PlayHornTuneAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Explode),
            Description = MenuText.Key(Loc.PersonalVehicle.ExplodeDescription),
            ConfirmationDescription = MenuText.Key(Loc.PersonalVehicle.ExplodeConfirm),
            Gate = PersonalVehiclePermissions.Explode,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnConfirmedAsync = _ => PersonalVehicle.ExplodeAsync(),
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

    private static int LightState(int index) => index switch
    {
        1 => RemoteVehicleAction.LightsOff,
        2 => RemoteVehicleAction.LightsOn,
        _ => RemoteVehicleAction.LightsAutomatic,
    };

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

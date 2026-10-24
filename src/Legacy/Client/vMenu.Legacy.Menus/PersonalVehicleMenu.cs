using MenuAPI;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.Personal;

using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;
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
        menu.Entries.Add(new ButtonEntry(Loc.PersonalVehicle.SetCurrent)
        {
            OnSelectedAsync = _ => PersonalVehicle.MarkCurrentAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.Status),
            Description = MenuText.From(Status),
            Label = MenuText.From(StatusLabel),
            ReadEnabled = static () => false,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PersonalVehicle.ShowBlip)
        {
            Gate = PersonalVehiclePermissions.Blip,
            ReadState = static () => PersonalVehicle.BlipWanted,
            OnChanged = changed => PersonalVehicle.SetBlipEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PersonalVehicle.Waypoint)
        {
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelected = _ => PersonalVehicle.SetWaypoint(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PersonalVehicle.Kick)
        {
            Gate = PersonalVehiclePermissions.Kick,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.KickOccupantsAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.PersonalVehicle.Delete)
        {
            ConfirmationDescription = MenuText.Key(Loc.PersonalVehicle.DeleteConfirm),
            Gate = PersonalVehiclePermissions.Delete,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnConfirmedAsync = _ => PersonalVehicle.DeleteAsync(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.PersonalVehicle.Forget)
        {
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.ForgetAsync(),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PersonalVehicle.Lock)
        {
            Gate = PersonalVehiclePermissions.Lock,
            ReadState = static () => PersonalVehicle.IsLocked,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnChangedAsync = changed => PersonalVehicle.SetLockedAsync(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PersonalVehicle.Electrocute)
        {
            Gate = MenuGate.Setting(BullySettings.Enabled) & MenuGate.Permission(PersonalVehiclePermissions.Electrocute),
            Behaviour = GateBehaviour.Hide,
            ReadState = static () => PersonalVehicle.IsElectrified,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnChangedAsync = changed => PersonalVehicle.SetElectrifiedAsync(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.PersonalVehicle.Engine)
        {
            Gate = PersonalVehiclePermissions.Engine,
            ReadState = static () => PersonalVehicle.IsEngineRunning,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnChangedAsync = changed => PersonalVehicle.SetEngineAsync(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(
            Loc.PersonalVehicle.Lights,
            [
                MenuText.Key(Loc.PersonalVehicle.LightsAutomatic),
                MenuText.Key(Loc.PersonalVehicle.LightsOff),
                MenuText.Key(Loc.PersonalVehicle.LightsOn),
            ])
        {
            Gate = PersonalVehiclePermissions.Lights,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = selected => PersonalVehicle.SetLightsAsync(LightState(selected.SelectedIndex)),
        });

        menu.Entries.Add(SubmenuEntry.For(new PersonalVehicleDoorsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new PersonalVehicleWindowsMenu()));

        menu.Entries.Add(new ButtonEntry(Loc.PersonalVehicle.Horn)
        {
            Gate = PersonalVehiclePermissions.Horn,
            ReadEnabled = static () => PersonalVehicle.IsMarked,
            OnSelectedAsync = _ => PersonalVehicle.PlayHornTuneAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.PersonalVehicle.Explode)
        {
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

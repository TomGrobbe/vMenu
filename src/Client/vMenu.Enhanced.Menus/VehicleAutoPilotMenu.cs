using System.Globalization;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.AutoPilot;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.AutoPilot.Title,
    SubtitleKey = Loc.AutoPilot.Subtitle,
    DescriptionKey = Loc.AutoPilot.LinkDescription,
    Permission = PlayerOptionsPermissions.AutoPilot)]
public sealed class VehicleAutoPilotMenu : MenuDefinition
{
    private const int MaxCruiseSpeed = 150;

    private const int CruiseStep = 5;

    private DetachedMenu? _pointPicker;

    private DetachedMenu? _pathPicker;

    protected override void Build(MenuBuilder menu)
    {
        _pointPicker = menu.AddDetachedMenu(
            MenuText.Key(Loc.AutoPilot.DriveToPoint),
            MenuText.Key(Loc.AutoPilot.DriveToPointSubtitle),
            _ => { });

        _pointPicker.Builder.OnOpened = _ => Refill(_pointPicker.Builder, PointRows());

        _pathPicker = menu.AddDetachedMenu(
            MenuText.Key(Loc.AutoPilot.ReplayPath),
            MenuText.Key(Loc.AutoPilot.ReplayPathSubtitle),
            _ => { });

        _pathPicker.Builder.OnOpened = _ => Refill(_pathPicker.Builder, PathRows());

        VehicleAutoPilot.Changed += () => MenuRegistry.Refresh(menu.Menu);

        menu.Entries.Add(Group(Loc.AutoPilot.GroupStatus, Loc.AutoPilot.GroupStatusDescription));

        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.From(Status),
            Description = MenuText.Key(Loc.AutoPilot.GroupStatusDescription),
            ShowArrows = false,
        });

        menu.Entries.Add(Group(Loc.AutoPilot.GroupStart, Loc.AutoPilot.GroupStartDescription));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.DriveToWaypoint),
            Description = MenuText.Key(Loc.AutoPilot.DriveToWaypointDescription),
            OnSelected = _ =>
            {
                if (VehicleAutoPilot.DriveToWaypoint())
                {
                    Notifications.Info(MenuText.Key(Loc.AutoPilot.StartedWaypoint));
                }
            },
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.DriveWander),
            Description = MenuText.Key(Loc.AutoPilot.DriveWanderDescription),
            OnSelected = _ =>
            {
                if (VehicleAutoPilot.Wander())
                {
                    Notifications.Info(MenuText.Key(Loc.AutoPilot.StartedWander));
                }
            },
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.DriveToPoint),
            Description = MenuText.Key(Loc.AutoPilot.DriveToPointDescription),
            Label = MenuText.Literal("→"),
            OnSelected = _ => _pointPicker?.Open(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.ReplayPath),
            Description = MenuText.Key(Loc.AutoPilot.ReplayPathDescription),
            Label = MenuText.Literal("→"),
            OnSelected = _ => _pathPicker?.Open(),
        });

        menu.Entries.Add(Group(Loc.AutoPilot.GroupControl, Loc.AutoPilot.GroupControlDescription));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.From(() => Localizer.Current.Get(
                VehicleAutoPilot.IsPaused ? Loc.AutoPilot.Resume : Loc.AutoPilot.Pause)),
            Description = MenuText.From(() => Localizer.Current.Get(
                VehicleAutoPilot.IsPaused ? Loc.AutoPilot.ResumeDescription : Loc.AutoPilot.PauseDescription)),
            ReadEnabled = () => VehicleAutoPilot.HasTask,
            OnSelected = _ =>
            {
                if (VehicleAutoPilot.IsPaused)
                {
                    VehicleAutoPilot.Resume();

                    Notifications.Info(MenuText.Key(Loc.AutoPilot.Resumed));

                    return;
                }

                VehicleAutoPilot.Pause();

                Notifications.Info(MenuText.Key(Loc.AutoPilot.Paused));
            },
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.Stop),
            Description = MenuText.Key(Loc.AutoPilot.StopDescription),
            ReadEnabled = () => VehicleAutoPilot.HasTask,
            OnSelected = _ =>
            {
                VehicleAutoPilot.Stop();

                Notifications.Info(MenuText.Key(Loc.AutoPilot.Stopped));
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.StopAction),
            Description = MenuText.Key(Loc.AutoPilot.StopActionDescription),
            Options =
            [
                MenuText.Key(Loc.AutoPilot.StopActionPark),
                MenuText.Key(Loc.AutoPilot.StopActionBrake),
                MenuText.Key(Loc.AutoPilot.StopActionCoast),
            ],
            ReadSelectedIndex = () => Math.Clamp(UserDefaults.AutoPilotStopAction.Value, 0, 2),
            OnIndexChanged = changed => UserDefaults.AutoPilotStopAction.Value = changed.NewIndex,
        });

        menu.Entries.Add(new DynamicListEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.CruiseSpeed),
            Description = MenuText.Key(Loc.AutoPilot.CruiseSpeedDescription),
            ReadValue = CruiseSpeed,
            Change = changing =>
            {
                var next = UserDefaults.AutoPilotCruiseSpeed.Value + (changing.Left ? -CruiseStep : CruiseStep);

                UserDefaults.AutoPilotCruiseSpeed.Value = Math.Clamp(next, 0, MaxCruiseSpeed);

                return CruiseSpeed();
            },
        });

        menu.Entries.Add(Group(Loc.AutoPilot.GroupProfiles, Loc.AutoPilot.GroupProfilesDescription));

        menu.Entries.Add(ProfileRow(
            AutoPilotCategory.Vehicle, Loc.AutoPilot.ProfileVehicle, Loc.AutoPilot.ProfileVehicleDescription));

        menu.Entries.Add(ProfileRow(
            AutoPilotCategory.Plane, Loc.AutoPilot.ProfilePlane, Loc.AutoPilot.ProfilePlaneDescription));

        menu.Entries.Add(ProfileRow(
            AutoPilotCategory.Boat, Loc.AutoPilot.ProfileBoat, Loc.AutoPilot.ProfileBoatDescription));

        menu.Entries.Add(ProfileRow(
            AutoPilotCategory.Helicopter, Loc.AutoPilot.ProfileHeli, Loc.AutoPilot.ProfileHeliDescription));

        menu.Entries.Add(Group(Loc.AutoPilot.GroupManage, Loc.AutoPilot.GroupManageDescription));

        menu.Entries.Add(SubmenuEntry.For(new AutoPilotProfilesMenu()));

        menu.Entries.Add(SubmenuEntry.For(new AutoPilotRoutesMenu()));
    }

    private static DynamicListEntry ProfileRow(AutoPilotCategory category, string textKey, string descriptionKey)
    {
        return new DynamicListEntry
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            ReadValue = () => AutoPilotDefaults.Resolve(category).Name,
            Change = changing => Shift(category, changing.Left),
        };
    }

    private static string Shift(AutoPilotCategory category, bool left)
    {
        var choices = AutoPilotDefaults.Choices(category);

        if (choices.Count == 0)
        {
            return string.Empty;
        }

        var current = AutoPilotDefaults.Resolve(category).Name;
        var at = 0;

        for (var index = 0; index < choices.Count; index++)
        {
            if (string.Equals(choices[index].Name, current, StringComparison.Ordinal))
            {
                at = index;

                break;
            }
        }

        var next = (at + (left ? -1 : 1) + choices.Count) % choices.Count;

        AutoPilotDefaults.Select(category, choices[next].Name);

        return choices[next].Name;
    }

    private List<MenuEntry> PointRows()
    {
        var rows = new List<MenuEntry>();
        var points = AutoPilotPointStore.All();

        if (points.Count == 0)
        {
            rows.Add(Placeholder(Loc.AutoPilot.PointsEmpty, Loc.AutoPilot.PointsEmptyDescription));

            return rows;
        }

        foreach (var entry in points)
        {
            var point = entry.Point;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(point.Name),
                Description = MenuText.Key(
                    Loc.AutoPilot.PointRowDescription,
                    ("description", MenuText.Literal(point.Description))),
                OnSelected = _ =>
                {
                    if (!VehicleAutoPilot.DriveToPoint(point))
                    {
                        return;
                    }

                    Notifications.Info(MenuText.Key(Loc.AutoPilot.Started, ("name", point.Name)));

                    _pointPicker?.Menu.GoBack();
                },
            });
        }

        return rows;
    }

    private List<MenuEntry> PathRows()
    {
        var rows = new List<MenuEntry>();
        var paths = AutoPilotPathStore.All();

        if (paths.Count == 0)
        {
            rows.Add(Placeholder(Loc.AutoPilot.PathsEmpty, Loc.AutoPilot.PathsEmptyDescription));

            return rows;
        }

        foreach (var entry in paths)
        {
            var path = entry.Path;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(path.Name),
                Description = MenuText.Literal(path.Description),
                Label = MenuText.Literal(path.Points.Count.ToString(CultureInfo.InvariantCulture)),
                OnSelected = _ =>
                {
                    if (!VehicleAutoPilot.ReplayPath(path))
                    {
                        return;
                    }

                    Notifications.Info(MenuText.Key(
                        Loc.AutoPilot.StartedPath,
                        ("name", path.Name),
                        ("count", MenuText.Literal(path.Points.Count.ToString(CultureInfo.InvariantCulture)))));

                    _pathPicker?.Menu.GoBack();
                },
            });
        }

        return rows;
    }

    private static string CruiseSpeed()
    {
        var speed = UserDefaults.AutoPilotCruiseSpeed.Value;

        return speed <= 0
            ? Localizer.Current.Get(Loc.AutoPilot.CruiseSpeedAuto)
            : speed.ToString(CultureInfo.InvariantCulture);
    }

    private static string Status()
    {
        var doing = VehicleAutoPilot.Mode switch
        {
            AutoPilotMode.Waypoint => MenuText.Key(Loc.AutoPilot.StatusWaypoint),
            AutoPilotMode.Point => MenuText.Key(
                Loc.AutoPilot.StatusPoint,
                ("name", MenuText.Literal(VehicleAutoPilot.TargetName))),
            AutoPilotMode.Wander => MenuText.Key(Loc.AutoPilot.StatusWander),
            AutoPilotMode.Path => MenuText.Key(
                Loc.AutoPilot.StatusPath,
                ("name", MenuText.Literal(VehicleAutoPilot.TargetName)),
                ("index", MenuText.Literal((VehicleAutoPilot.PathIndex + 1).ToString(CultureInfo.InvariantCulture))),
                ("count", MenuText.Literal(VehicleAutoPilot.PathCount.ToString(CultureInfo.InvariantCulture)))),
            _ => MenuText.Key(Loc.AutoPilot.StatusIdle),
        };

        if (!VehicleAutoPilot.IsPaused)
        {
            return doing.Resolve(Localizer.Current);
        }

        return MenuText.Key(Loc.AutoPilot.StatusPaused, ("what", doing)).Resolve(Localizer.Current);
    }

    private static SeparatorEntry Group(string textKey, string descriptionKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
    };

    private static ButtonEntry Placeholder(string textKey, string descriptionKey) => new()
    {
        Text = MenuText.Key(textKey),
        Description = MenuText.Key(descriptionKey),
        ReadEnabled = () => false,
    };

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

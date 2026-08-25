using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Saved;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.AutoPilot;
using vMenu.Enhanced.Storage;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.AutoPilot.RoutesTitle,
    SubtitleKey = Loc.AutoPilot.RoutesSubtitle,
    DescriptionKey = Loc.AutoPilot.RoutesLinkDescription,
    Permission = PlayerOptionsPermissions.AutoPilot)]
public sealed class AutoPilotRoutesMenu : MenuDefinition
{
    private const int NameLength = 40;

    private const int DescriptionLength = 100;

    private const int MinSpacing = 5;

    private const int MaxSpacing = 200;

    private const int SpacingStep = 5;

    private DetachedMenu? _pointMenu;

    private DetachedMenu? _pathMenu;

    private DetachedMenu? _pathPointsMenu;

    private DetachedMenu? _recorderMenu;

    private SavedAutoPilotPointEntry? _point;

    private SavedAutoPilotPathEntry? _path;

    protected override void Build(MenuBuilder menu)
    {
        _pointMenu = menu.AddDetachedMenu(
            MenuText.From(() => _point?.Point.Name ?? string.Empty),
            MenuText.Key(Loc.AutoPilot.GroupPoints),
            _ => { },
            MenuGate.When(() => _point is not null));

        _pointMenu.Builder.OnOpened = _ => Refill(_pointMenu, PointRows());

        _pathPointsMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.AutoPilot.PathPointsListSubtitle),
            MenuText.From(() => _path?.Path.Name ?? string.Empty),
            _ => { },
            MenuGate.When(() => _path is not null));

        _pathPointsMenu.Builder.OnOpened = _ => Refill(_pathPointsMenu, PathPointRows());

        _pathMenu = menu.AddDetachedMenu(
            MenuText.From(() => _path?.Path.Name ?? string.Empty),
            MenuText.Key(Loc.AutoPilot.GroupPaths),
            _ => { },
            MenuGate.When(() => _path is not null));

        _pathMenu.Builder.OnOpened = _ => Refill(_pathMenu, PathRows());

        _recorderMenu = menu.AddDetachedMenu(
            MenuText.Key(Loc.AutoPilot.RecordPath),
            MenuText.Key(Loc.AutoPilot.RecordPathSubtitle),
            _ => { });

        _recorderMenu.Builder.OnOpened = _ => Refill(_recorderMenu, RecorderRows());

        PathRecorder.Changed += () =>
        {
            if (_recorderMenu is { } recorder)
            {
                MenuRegistry.Refresh(recorder.Menu);
            }
        };

        menu.AddRange(RootRows());

        menu.OnOpened = _ => Refill(menu, RootRows());
    }

    private List<MenuEntry> RootRows()
    {
        var rows = new List<MenuEntry>
        {
            Group(Loc.AutoPilot.GroupPoints, Loc.AutoPilot.GroupPointsDescription),

            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.SavePosition),
                Description = MenuText.Key(Loc.AutoPilot.SavePositionDescription),
                OnSelectedAsync = _ => SavePointAsync(Here()),
            },

            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.SaveWaypoint),
                Description = MenuText.Key(Loc.AutoPilot.SaveWaypointDescription),
                ReadEnabled = Native.IsWaypointActive,
                OnSelectedAsync = _ => SaveWaypointAsync(),
            },
        };

        var points = AutoPilotPointStore.All();

        if (points.Count == 0)
        {
            rows.Add(Placeholder(Loc.AutoPilot.PointsEmpty, Loc.AutoPilot.PointsEmptyDescription));
        }

        foreach (var entry in points)
        {
            var picked = entry;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(entry.Point.Name),
                Description = MenuText.Key(
                    Loc.AutoPilot.PointRowDescription,
                    ("description", MenuText.Literal(entry.Point.Description))),
                Label = MenuText.Literal("→"),
                OnSelected = _ =>
                {
                    _point = picked;

                    _pointMenu?.Open();
                },
            });
        }

        rows.Add(Group(Loc.AutoPilot.GroupPaths, Loc.AutoPilot.GroupPathsDescription));

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.RecordPath),
            Description = MenuText.Key(Loc.AutoPilot.RecordPathDescription),
            Label = MenuText.Literal("→"),
            OnSelected = _ => _recorderMenu?.Open(),
        });

        var paths = AutoPilotPathStore.All();

        if (paths.Count == 0)
        {
            rows.Add(Placeholder(Loc.AutoPilot.PathsEmpty, Loc.AutoPilot.PathsEmptyDescription));

            return rows;
        }

        foreach (var entry in paths)
        {
            var picked = entry;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Literal(entry.Path.Name),
                Description = MenuText.Key(
                    Loc.AutoPilot.PathRowDescription,
                    ("description", MenuText.Literal(entry.Path.Description)),
                    ("count", Num(entry.Path.Points.Count)),
                    ("length", Num((int)Length(entry.Path)))),
                Label = MenuText.Literal("→"),
                OnSelected = _ =>
                {
                    _path = picked;

                    _pathMenu?.Open();
                },
            });
        }

        return rows;
    }

    #region Points

    private List<MenuEntry> PointRows()
    {
        var rows = new List<MenuEntry>();

        if (_point is not { } entry)
        {
            return rows;
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PointDriveHere),
            Description = MenuText.Key(Loc.AutoPilot.PointDriveHereDescription),
            OnSelected = _ =>
            {
                if (VehicleAutoPilot.DriveToPoint(entry.Point))
                {
                    Notifications.Info(MenuText.Key(Loc.AutoPilot.Started, ("name", entry.Point.Name)));
                }
            },
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PointRename),
            Description = MenuText.Key(Loc.AutoPilot.PointRenameDescription),
            ReadEnabled = () => !entry.IsFromNewerBuild,
            OnSelectedAsync = _ => RenamePointAsync(entry),
        });

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PointDelete),
            Description = MenuText.Key(Loc.AutoPilot.PointDeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.AutoPilot.PointDeleteConfirm, ("name", entry.Point.Name)),
            OnConfirmed = _ =>
            {
                var name = entry.Point.Name;

                AutoPilotPointStore.Delete(name);

                _point = null;

                Notifications.Success(MenuText.Key(Loc.AutoPilot.PointDeleted, ("name", name)));

                Rebuild();

                _pointMenu?.Menu.GoBack();
            },
        });

        return rows;
    }

    private async Task SaveWaypointAsync()
    {
        if (VehicleAutoPilot.WaypointPosition() is not { } target)
        {
            return;
        }

        await SavePointAsync(target);
    }

    private async Task SavePointAsync(Vector3 position)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.PointNamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.PointDescriptionPrompt), DescriptionLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        var outcome = AutoPilotPointStore.Save(
            new SavedAutoPilotPoint
            {
                Name = name,
                Description = answers[1].Trim(),
                X = position.X,
                Y = position.Y,
                Z = position.Z,
            },
            replacing: false);

        switch (outcome)
        {
            case SaveOutcome.Saved:
                Notifications.Success(MenuText.Key(Loc.AutoPilot.PointSaved, ("name", name)));

                Rebuild();

                break;

            case SaveOutcome.NameTaken:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.PointNameTaken, ("name", name)));

                break;

            default:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.PointRefused));

                break;
        }
    }

    private async Task RenamePointAsync(SavedAutoPilotPointEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.PointNamePrompt), NameLength, entry.Point.Name),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.PointDescriptionPrompt), DescriptionLength, entry.Point.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!AutoPilotPointStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.PointNameTaken, ("name", name)));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.AutoPilot.PointRenamed, ("name", name)));

        Rebuild();
    }

    #endregion

    #region Paths

    private List<MenuEntry> PathRows()
    {
        var rows = new List<MenuEntry>();

        if (_path is not { } entry)
        {
            return rows;
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PathReplay),
            Description = MenuText.Key(Loc.AutoPilot.PathReplayDescription),
            OnSelected = _ =>
            {
                if (VehicleAutoPilot.ReplayPath(entry.Path))
                {
                    Notifications.Info(MenuText.Key(
                        Loc.AutoPilot.StartedPath,
                        ("name", entry.Path.Name),
                        ("count", Num(entry.Path.Points.Count))));
                }
            },
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PathPointsList),
            Description = MenuText.Key(Loc.AutoPilot.PathPointsListDescription),
            Label = MenuText.Literal(entry.Path.Points.Count.ToString(CultureInfo.InvariantCulture)),
            OnSelected = _ => _pathPointsMenu?.Open(),
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PathRename),
            Description = MenuText.Key(Loc.AutoPilot.PathRenameDescription),
            ReadEnabled = () => !entry.IsFromNewerBuild,
            OnSelectedAsync = _ => RenamePathAsync(entry),
        });

        rows.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.AutoPilot.PathDelete),
            Description = MenuText.Key(Loc.AutoPilot.PathDeleteDescription),
            ConfirmationDescription = MenuText.Key(Loc.AutoPilot.PathDeleteConfirm, ("name", entry.Path.Name)),
            OnConfirmed = _ =>
            {
                var name = entry.Path.Name;

                AutoPilotPathStore.Delete(name);

                _path = null;

                Notifications.Success(MenuText.Key(Loc.AutoPilot.PathDeleted, ("name", name)));

                Rebuild();

                _pathMenu?.Menu.GoBack();
            },
        });

        return rows;
    }

    private List<MenuEntry> PathPointRows()
    {
        var rows = new List<MenuEntry>();

        if (_path is not { } entry)
        {
            return rows;
        }

        for (var index = 0; index < entry.Path.Points.Count; index++)
        {
            var at = index;
            var point = entry.Path.Points[index];
            var number = Num(index + 1);

            rows.Add(new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.PathPointRow, ("index", number)),
                Description = MenuText.Key(
                    Loc.AutoPilot.PathPointRowDescription,
                    ("x", Coord(point.X)),
                    ("y", Coord(point.Y)),
                    ("z", Coord(point.Z))),
                ConfirmationDescription = MenuText.Key(Loc.AutoPilot.PathPointDeleteConfirm, ("index", number)),
                ReadEnabled = () => !entry.IsFromNewerBuild,
                OnConfirmed = _ => RemovePoint(entry, at),
            });
        }

        return rows;
    }

    private void RemovePoint(SavedAutoPilotPathEntry entry, int index)
    {
        if (entry.Path.Points.Count <= 1)
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.PathPointLast));

            return;
        }

        if (!AutoPilotPathStore.RemovePoint(entry, index))
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderRefused));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.AutoPilot.PathPointRemoved));

        Rebuild();
    }

    private async Task RenamePathAsync(SavedAutoPilotPathEntry entry)
    {
        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.RecorderNamePrompt), NameLength, entry.Path.Name),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.RecorderDescriptionPrompt), DescriptionLength, entry.Path.Description)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        if (!AutoPilotPathStore.Edit(entry, name, answers[1].Trim()))
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderNameTaken, ("name", name)));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.AutoPilot.PathRenamed, ("name", name)));

        Rebuild();
    }

    #endregion

    #region Recorder

    private List<MenuEntry> RecorderRows()
    {
        return
        [
            new SeparatorEntry
            {
                Text = MenuText.From(RecorderStatus),
                Description = MenuText.Key(Loc.AutoPilot.RecordPathDescription),
                ShowArrows = false,
            },

            new ButtonEntry
            {
                Text = MenuText.From(() => Localizer.Current.Get(
                    PathRecorder.IsRecording ? Loc.AutoPilot.RecorderStop : Loc.AutoPilot.RecorderStart)),
                Description = MenuText.From(() => Localizer.Current.Get(
                    PathRecorder.IsRecording ? Loc.AutoPilot.RecorderStopDescription : Loc.AutoPilot.RecorderStartDescription)),
                OnSelected = _ =>
                {
                    if (PathRecorder.IsRecording)
                    {
                        PathRecorder.Stop();
                    }
                    else
                    {
                        PathRecorder.Start();
                    }

                    Rebuild();
                },
            },

            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderDrop),
                Description = MenuText.Key(Loc.AutoPilot.RecorderDropDescription),
                ReadEnabled = () => !PathRecorder.IsFull,
                OnSelected = _ =>
                {
                    if (PathRecorder.Drop())
                    {
                        Notifications.Success(MenuText.Key(Loc.AutoPilot.RecorderDropped, ("count", Num(PathRecorder.Count))));
                    }
                    else
                    {
                        Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderFull, ("count", Num(PathRecorder.MaxPoints))));
                    }

                    Rebuild();
                },
            },

            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderUndo),
                Description = MenuText.Key(Loc.AutoPilot.RecorderUndoDescription),
                ReadEnabled = () => PathRecorder.Count > 0,
                OnSelected = _ =>
                {
                    if (PathRecorder.Undo())
                    {
                        Notifications.Success(MenuText.Key(Loc.AutoPilot.RecorderUndone, ("count", Num(PathRecorder.Count))));
                    }

                    Rebuild();
                },
            },

            new CheckboxEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderAuto),
                Description = MenuText.Key(Loc.AutoPilot.RecorderAutoDescription),
                ReadState = () => UserDefaults.AutoPilotAutoRecord.Value,
                OnChanged = changed => UserDefaults.AutoPilotAutoRecord.Value = changed.Checked,
            },

            new SliderEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderSpacing),
                Description = MenuText.Key(Loc.AutoPilot.RecorderSpacingDescription),
                Min = MinSpacing / SpacingStep,
                Max = MaxSpacing / SpacingStep,
                ReadPosition = () => Math.Clamp(
                    UserDefaults.AutoPilotPathSpacing.Value / SpacingStep,
                    MinSpacing / SpacingStep,
                    MaxSpacing / SpacingStep),
                OnMoved = moved => UserDefaults.AutoPilotPathSpacing.Value = moved.NewPosition * SpacingStep,
            },

            new ButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderSave),
                Description = MenuText.Key(Loc.AutoPilot.RecorderSaveDescription),
                ReadEnabled = () => PathRecorder.Count > 0,
                OnSelectedAsync = _ => SavePathAsync(),
            },

            new ConfirmButtonEntry
            {
                Text = MenuText.Key(Loc.AutoPilot.RecorderDiscard),
                Description = MenuText.Key(Loc.AutoPilot.RecorderDiscardDescription),
                ConfirmationDescription = MenuText.Key(
                    Loc.AutoPilot.RecorderDiscardConfirm,
                    ("count", MenuText.From(() => PathRecorder.Count.ToString(CultureInfo.InvariantCulture)))),
                ReadEnabled = () => PathRecorder.Count > 0,
                OnConfirmed = _ =>
                {
                    PathRecorder.Discard();

                    Notifications.Success(MenuText.Key(Loc.AutoPilot.RecorderDiscarded));

                    Rebuild();
                },
            },
        ];
    }

    private async Task SavePathAsync()
    {
        if (PathRecorder.Count == 0)
        {
            Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderEmpty));

            return;
        }

        if (await UserInput.GetTextAsync(
            new InputPrompt(MenuText.Key(Loc.AutoPilot.RecorderNamePrompt), NameLength),
            new InputPrompt(MenuText.Key(Loc.AutoPilot.RecorderDescriptionPrompt), DescriptionLength)) is not { } answers)
        {
            return;
        }

        var name = answers[0].Trim();

        if (name.Length == 0)
        {
            return;
        }

        var built = PathRecorder.Build(name, answers[1].Trim());
        var outcome = AutoPilotPathStore.Save(built, replacing: false);

        switch (outcome)
        {
            case SaveOutcome.Saved:
                Notifications.Success(MenuText.Key(
                    Loc.AutoPilot.RecorderSaved,
                    ("name", name),
                    ("count", Num(built.Points.Count))));

                PathRecorder.Discard();

                Rebuild();

                break;

            case SaveOutcome.NameTaken:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderNameTaken, ("name", name)));

                break;

            default:
                Notifications.Error(MenuText.Key(Loc.AutoPilot.RecorderRefused));

                break;
        }
    }

    private static string RecorderStatus()
    {
        if (!PathRecorder.IsRecording && PathRecorder.Count == 0)
        {
            return Localizer.Current.Get(Loc.AutoPilot.RecorderIdle);
        }

        return MenuText.Key(
            PathRecorder.IsRecording ? Loc.AutoPilot.RecorderRunning : Loc.AutoPilot.RecorderHeld,
            ("count", Num(PathRecorder.Count)),
            ("length", Num((int)PathRecorder.Length()))).Resolve(Localizer.Current);
    }

    #endregion

    private void Rebuild()
    {
        if (_pointMenu is { } point)
        {
            Refill(point, PointRows());
        }

        if (_pathPointsMenu is { } pathPoints)
        {
            Refill(pathPoints, PathPointRows());
        }

        if (_pathMenu is { } path)
        {
            Refill(path, PathRows());
        }

        if (_recorderMenu is { } recorder)
        {
            MenuRegistry.Refresh(recorder.Menu);
        }
    }

    private static Vector3 Here()
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle != 0)
        {
            return Native.GetEntityCoords(vehicle, false);
        }

        return API.Players.Local.Ped is { } ped
            ? Native.GetEntityCoords(ped.Handle, false)
            : Vector3.Zero;
    }

    private static float Length(SavedAutoPilotPath path)
    {
        var total = 0f;

        for (var index = 1; index < path.Points.Count; index++)
        {
            var previous = path.Points[index - 1];
            var current = path.Points[index];

            total += Vector3.Distance(
                new Vector3(previous.X, previous.Y, previous.Z),
                new Vector3(current.X, current.Y, current.Z));
        }

        return total;
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

    private static MenuText Num(int value) =>
        MenuText.Literal(value.ToString(CultureInfo.InvariantCulture));

    private static MenuText Coord(float value) =>
        MenuText.Literal(value.ToString("0.#", CultureInfo.InvariantCulture));

    private static void Refill(DetachedMenu menu, IReadOnlyList<MenuEntry> rows)
    {
        var builder = menu.Builder;

        var was = builder.Menu.CurrentIndex;
        var offset = builder.Menu.ViewIndexOffset;

        builder.ClearEntries();
        builder.AddRange(rows);

        var keep = was < builder.Menu.GetMenuItems().Count;

        builder.Menu.RefreshIndex(keep ? was : 0, keep ? offset : 0);
    }

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

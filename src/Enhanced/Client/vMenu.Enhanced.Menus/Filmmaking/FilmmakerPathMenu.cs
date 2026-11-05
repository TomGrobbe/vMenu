using vMenu.Enhanced.Freecam;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.MenuFramework;

using FilmmakerPermissions = vMenu.Enhanced.Data.Permissions.Menus.Filmmaker;
using FreecamCamera = vMenu.Enhanced.Freecam.Freecam;

namespace vMenu.Enhanced.Menus.Filmmaking;

[VMenu(
    TitleKey = Loc.Filmmaker.PathTitle,
    SubtitleKey = Loc.Filmmaker.PathSubtitle,
    DescriptionKey = Loc.Filmmaker.PathLinkDescription,
    Permission = FilmmakerPermissions.Paths)]
public sealed class FilmmakerPathMenu : MenuDefinition
{
    private const float DurationStep = 0.25f;

    private const float MaxDuration = 120f;

    private static readonly float[] Speeds = [0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f, 4f];

    private static readonly float[] Delays = [0f, 1f, 2f, 3f, 5f];

    private MenuBuilder? _root;

    private MenuBuilder? _keyframes;

    private MenuBuilder? _saved;

    private DetachedMenu? _keyframeMenu;

    private DetachedMenu? _savedMenu;

    private int _keyframe = -1;

    private FilmEntry<FilmPath>? _savedEntry;

    protected override void Build(MenuBuilder menu)
    {
        _root = menu;

        _keyframeMenu = menu.AddDetachedMenu(
            MenuText.From(() => MenuText.Key(Loc.Filmmaker.KeyframeRow, ("index", FilmmakerText.Number(_keyframe + 1))).Resolve(Localizer.Current)),
            MenuText.Key(Loc.Filmmaker.PathTitle),
            _ => { },
            MenuGate.When(() => Frame() is not null));

        _keyframeMenu.Builder.OnOpened = _ => _keyframeMenu.Builder.ReplaceEntries(KeyframeDetailRows());

        _savedMenu = menu.AddDetachedMenu(
            MenuText.From(() => _savedEntry?.Item.Name ?? string.Empty),
            MenuText.Key(Loc.Filmmaker.PathSaved),
            _ => { },
            MenuGate.When(() => _savedEntry is not null));

        _savedMenu.Builder.OnOpened = _ => _savedMenu.Builder.ReplaceEntries(SavedDetailRows());

        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.From(Status),
            ShowArrows = false,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.From(() => Localizer.Current.Get(PathPlayer.IsActive ? Loc.Filmmaker.PathStop : Loc.Filmmaker.PathPlay)),
            Description = MenuText.Key(Loc.Filmmaker.PathPlayDescription),
            OnSelected = _ =>
            {
                if (PathPlayer.IsActive)
                {
                    PathPlayer.Stop();

                    return;
                }

                FreecamCamera.Report(Filmmaker.Play());
            },
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.From(() => Localizer.Current.Get(PathPlayer.State == PlaybackState.Paused ? Loc.Filmmaker.PathResume : Loc.Filmmaker.PathPause)),
            ReadEnabled = () => PathPlayer.IsActive,
            OnSelected = _ => PathPlayer.TogglePause(),
        });

        menu.Entries.Add(new SliderEntry(Loc.Filmmaker.PathScrub, 0, 100)
        {
            ReadEnabled = HasKeyframes,
            ReadPosition = ScrubPosition,
            OnMoved = moved => FreecamCamera.Report(Filmmaker.Preview(moved.NewPosition / 100f)),
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.PathAddKeyframe)
        {
            ReadEnabled = () => FreecamCamera.IsActive,
            OnSelected = _ => FreecamCamera.Report(Filmmaker.AddKeyframe()),
        });

        menu.Entries.Add(new DynamicListEntry(
            Loc.Filmmaker.PathGoTo,
            GoToText,
            change =>
            {
                FreecamCamera.Report(Filmmaker.StepKeyframe(change.Left ? -1 : 1));

                return GoToText();
            })
        {
            ReadEnabled = HasKeyframes,
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.PathFirst)
        {
            ReadEnabled = HasKeyframes,
            OnSelected = _ => FreecamCamera.Report(Filmmaker.JumpToKeyframe(0)),
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.PathLast)
        {
            ReadEnabled = HasKeyframes,
            OnSelected = _ => FreecamCamera.Report(Filmmaker.JumpToKeyframe(Filmmaker.ActivePath.Keyframes.Count - 1)),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Filmmaker.PlaybackStay)
        {
            ReadState = () => PathPlayer.StayOnLastFrame,
            OnChanged = changed => PathPlayer.StayOnLastFrame = changed.Checked,
        });

        menu.Entries.Add(new SubmenuEntry(Loc.Filmmaker.PathKeyframes)
        {
            Build = child =>
            {
                _keyframes = child;

                child.OnOpened = _ => child.ReplaceEntries(KeyframeRows());
            },
        });

        menu.Entries.Add(new SubmenuEntry(Loc.Filmmaker.PathPlayback)
        {
            Build = PlaybackRows,
        });

        menu.Entries.Add(new SeparatorEntry(Loc.Filmmaker.PathGroupManage));

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.PathSaveAs)
        {
            OnSelectedAsync = _ => SaveAsAsync(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.Filmmaker.Rename)
        {
            ReadEnabled = () => Filmmaker.IsSaved,
            OnSelectedAsync = _ => RenameAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Filmmaker.PathNew)
        {
            OnConfirmed = _ => Filmmaker.New(),
        });

        menu.Entries.Add(new SubmenuEntry(Loc.Filmmaker.PathSaved)
        {
            Build = child =>
            {
                _saved = child;

                child.OnOpened = _ => child.ReplaceEntries(SavedRows());
            },
        });

        Filmmaker.PathChanged += Rebuild;
        PathPlayer.Changed += menu.Refresh;
        FreecamCamera.StateChanged += menu.Refresh;
    }

    #region Keyframes

    private FilmKeyframe? Frame()
    {
        var frames = Filmmaker.ActivePath.Keyframes;

        return (uint)_keyframe < (uint)frames.Count ? frames[_keyframe] : null;
    }

    private List<MenuEntry> KeyframeRows()
    {
        var frames = Filmmaker.ActivePath.Keyframes;

        if (frames.Count == 0)
        {
            return [FilmmakerText.Placeholder(Loc.Filmmaker.PathEmptyRow)];
        }

        var rows = new List<MenuEntry>(frames.Count);

        for (var index = 0; index < frames.Count; index++)
        {
            var at = index;
            var frame = frames[index];
            var last = index == frames.Count - 1 && !PathPlayer.Closed;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(Loc.Filmmaker.KeyframeRow, ("index", FilmmakerText.Number(index + 1))),
                Label = last
                    ? MenuText.Literal("→")
                    : MenuText.Literal($"{FilmmakerText.Format(frame.Duration)} s →"),
                OnSelected = _ =>
                {
                    _keyframe = at;

                    _keyframeMenu?.Open();
                },
            });
        }

        return rows;
    }

    private List<MenuEntry> KeyframeDetailRows()
    {
        if (Frame() is null)
        {
            return [];
        }

        return
        [
            new ButtonEntry(Loc.Filmmaker.KeyframeJump)
            {
                ReadEnabled = () => FreecamCamera.IsActive,
                OnSelected = _ => FreecamCamera.Report(Filmmaker.JumpToKeyframe(_keyframe)),
            },
            new ButtonEntry(Loc.Filmmaker.KeyframeReplace)
            {
                ReadEnabled = () => FreecamCamera.IsActive,
                OnSelected = _ => FreecamCamera.Report(Filmmaker.ReplaceKeyframe(_keyframe)),
            },
            new ButtonEntry(Loc.Filmmaker.KeyframeInsert)
            {
                ReadEnabled = () => FreecamCamera.IsActive,
                OnSelected = _ =>
                {
                    var outcome = Filmmaker.InsertKeyframe(_keyframe);

                    if (outcome == KeyframeOutcome.Done)
                    {
                        _keyframe++;
                    }

                    FreecamCamera.Report(outcome);
                },
            },
            new DynamicListEntry(
                Loc.Filmmaker.KeyframeDuration,
                DurationText,
                change =>
                {
                    if (Frame() is { } frame)
                    {
                        frame.Duration = Math.Clamp(frame.Duration + (change.Left ? -DurationStep : DurationStep), FilmPath.MinKeyframeDuration, MaxDuration);

                        Filmmaker.Changed();
                    }

                    return DurationText();
                }),
            new ListEntry(
                Loc.Filmmaker.KeyframeBlend,
                FilmmakerText.Keys(Loc.Filmmaker.BlendCut, Loc.Filmmaker.BlendLinear, Loc.Filmmaker.BlendSmooth))
            {
                ReadSelectedIndex = () => (int)(Frame()?.Blend ?? KeyframeBlend.Smooth),
                OnIndexChanged = changed => Edit(frame => frame.Blend = (KeyframeBlend)changed.NewIndex),
            },
            new ListEntry(
                Loc.Filmmaker.KeyframeEase,
                FilmmakerText.Keys(Loc.Filmmaker.EaseNone, Loc.Filmmaker.EaseIn, Loc.Filmmaker.EaseOut, Loc.Filmmaker.EaseInOut))
            {
                ReadSelectedIndex = () => (int)(Frame()?.Ease ?? KeyframeEase.None),
                OnIndexChanged = changed => Edit(frame => frame.Ease = (KeyframeEase)changed.NewIndex),
            },
            new ButtonEntry(Loc.Filmmaker.KeyframeMoveUp)
            {
                ReadEnabled = () => _keyframe > 0,
                OnSelected = _ => Move(-1),
            },
            new ButtonEntry(Loc.Filmmaker.KeyframeMoveDown)
            {
                ReadEnabled = () => _keyframe < Filmmaker.ActivePath.Keyframes.Count - 1,
                OnSelected = _ => Move(1),
            },
            new ConfirmButtonEntry(Loc.Filmmaker.Delete)
            {
                OnConfirmed = _ =>
                {
                    Filmmaker.RemoveKeyframe(_keyframe);

                    _keyframe = -1;

                    _keyframeMenu?.Menu.GoBack();
                },
            },
        ];
    }

    private string DurationText() => FilmmakerText.Format(Frame()?.Duration ?? 0f) + " s";

    private void Edit(Action<FilmKeyframe> change)
    {
        if (Frame() is not { } frame)
        {
            return;
        }

        change(frame);

        Filmmaker.Changed();
    }

    private void Move(int offset)
    {
        var target = _keyframe + offset;

        if (target < 0 || target >= Filmmaker.ActivePath.Keyframes.Count)
        {
            return;
        }

        Filmmaker.MoveKeyframe(_keyframe, offset);

        _keyframe = target;

        _keyframeMenu?.Refresh();
    }

    #endregion

    #region Playback

    private static void PlaybackRows(MenuBuilder child)
    {
        child.Entries.Add(new ListEntry(
            Loc.Filmmaker.PlaybackLoop,
            FilmmakerText.Keys(Loc.Filmmaker.LoopOnce, Loc.Filmmaker.LoopRepeat, Loc.Filmmaker.LoopPingPong, Loc.Filmmaker.LoopCircuit))
        {
            ReadSelectedIndex = () => (int)PathPlayer.Loop,
            OnIndexChanged = changed => PathPlayer.Loop = (LoopMode)changed.NewIndex,
        });

        child.Entries.Add(new ListEntry(
            Loc.Filmmaker.PlaybackSpeed,
            Speeds.Select(speed => MenuText.Literal(FilmmakerText.Format(speed) + "x")).ToList())
        {
            ReadSelectedIndex = () => IndexOf(Speeds, PathPlayer.Speed),
            OnIndexChanged = changed => PathPlayer.Speed = Speeds[changed.NewIndex],
        });

        child.Entries.Add(new ListEntry(
            Loc.Filmmaker.PlaybackDelay,
            Delays.Select(delay => MenuText.Literal(FilmmakerText.Format(delay) + " s")).ToList())
        {
            ReadSelectedIndex = () => IndexOf(Delays, PathPlayer.StartDelay),
            OnIndexChanged = changed => PathPlayer.StartDelay = Delays[changed.NewIndex],
        });

        child.Entries.Add(new CheckboxEntry(Loc.Filmmaker.PlaybackHideHud)
        {
            ReadState = () => PathPlayer.HideHud,
            OnChanged = changed => PathPlayer.HideHud = changed.Checked,
        });

        child.Entries.Add(new CheckboxEntry(Loc.Filmmaker.PlaybackLetterbox)
        {
            ReadState = () => PathPlayer.Letterbox,
            OnChanged = changed => PathPlayer.Letterbox = changed.Checked,
        });
    }

    #endregion

    #region Saving

    private List<MenuEntry> SavedRows()
    {
        var paths = FilmStores.Paths.All();

        if (paths.Count == 0)
        {
            return [FilmmakerText.Placeholder(Loc.Filmmaker.PathsEmpty)];
        }

        return paths.Select(entry => (MenuEntry)new ButtonEntry
        {
            Text = MenuText.Literal(entry.Item.Name),
            Label = MenuText.Literal($"{FilmmakerText.Format(entry.Item.Keyframes.Count)} →"),
            OnSelected = _ =>
            {
                _savedEntry = entry;

                _savedMenu?.Open();
            },
        }).ToList();
    }

    private List<MenuEntry> SavedDetailRows()
    {
        if (_savedEntry is not { } entry)
        {
            return [];
        }

        return
        [
            new ButtonEntry(Loc.Filmmaker.PathLoad)
            {
                ReadEnabled = () => !entry.IsFromNewerBuild,
                OnSelected = _ =>
                {
                    Filmmaker.Load(entry.Item);

                    Notifications.Success(MenuText.Key(Loc.Filmmaker.PathLoaded, ("name", MenuText.Literal(entry.Item.Name))));
                },
            },
            new ConfirmButtonEntry(Loc.Filmmaker.Delete)
            {
                OnConfirmed = _ =>
                {
                    var name = entry.Item.Name;

                    FilmStores.Paths.Delete(name);

                    if (ReferenceEquals(Filmmaker.ActivePath, entry.Item)
                        || string.Equals(Filmmaker.ActivePath.Name, name, StringComparison.Ordinal))
                    {
                        Filmmaker.ActivePath.Name = string.Empty;
                    }

                    Notifications.Success(MenuText.Key(Loc.Filmmaker.Deleted, ("name", MenuText.Literal(name))));

                    _savedEntry = null;

                    Rebuild();

                    _savedMenu?.Menu.GoBack();
                },
            },
        ];
    }

    private async Task SaveAsAsync()
    {
        var path = Filmmaker.ActivePath;

        if (path.Keyframes.Count == 0)
        {
            Notifications.Error(MenuText.Key(Loc.Freecam.PathEmpty));

            return;
        }

        if (await FilmmakerText.PromptNameAsync(path.Name) is not { } name)
        {
            return;
        }

        var copy = path.Copy(name);

        if (FilmmakerText.Report(FilmStores.Paths.Save(copy, replacing: false), Loc.Filmmaker.PathSavedAs, name))
        {
            Filmmaker.Load(copy);
        }
    }

    private async Task RenameAsync()
    {
        var path = Filmmaker.ActivePath;

        if (await FilmmakerText.PromptNameAsync(path.Name) is { } name
            && FilmmakerText.Report(FilmStores.Paths.Rename(path, name), Loc.Filmmaker.Renamed, name))
        {
            Rebuild();
        }
    }

    #endregion

    private static string Status()
    {
        var path = Filmmaker.ActivePath;
        var name = path.Name.Length > 0 ? path.Name : Localizer.Current.Get(Loc.Filmmaker.PathUnsaved);

        return MenuText.Key(
            path.Space == PathSpace.Target ? Loc.Filmmaker.PathStatusTarget : Loc.Filmmaker.PathStatus,
            ("name", MenuText.Literal(name)),
            ("count", FilmmakerText.Number(path.Keyframes.Count)),
            ("seconds", FilmmakerText.Number(PathPlayer.Length(path), "0.#"))).Resolve(Localizer.Current);
    }

    private static bool HasKeyframes() => FreecamCamera.IsActive && Filmmaker.ActivePath.Keyframes.Count > 0;

    private static string GoToText()
    {
        var count = Filmmaker.ActivePath.Keyframes.Count;
        var current = Filmmaker.CurrentKeyframe;

        return (uint)current < (uint)count
            ? $"{FilmmakerText.Format(current + 1)} / {FilmmakerText.Format(count)}"
            : $"0 / {FilmmakerText.Format(count)}";
    }

    private static int IndexOf(float[] values, float value)
    {
        for (var index = 0; index < values.Length; index++)
        {
            if (MathF.Abs(values[index] - value) < 0.001f)
            {
                return index;
            }
        }

        return 0;
    }

    private static int ScrubPosition()
    {
        var length = PathPlayer.Length(Filmmaker.ActivePath);

        return length > 0f ? (int)MathF.Round(PathPlayer.Time / length * 100f) : 0;
    }

    private void Rebuild()
    {
        _root?.Refresh();
        _keyframes?.ReplaceEntries(KeyframeRows());
        _saved?.ReplaceEntries(SavedRows());
        _keyframeMenu?.Refresh();
        _savedMenu?.Refresh();
    }
}

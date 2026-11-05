namespace vMenu.Enhanced.Freecam;

public enum KeyframeOutcome
{
    Done,
    NotInFreecam,
    Full,
    SpaceMismatch,
    Empty,
}

public enum PlayOutcome
{
    Started,
    NotInFreecam,
    TooFewKeyframes,
    NeedsTarget,
}

public static class Filmmaker
{
    public static FilmPath ActivePath { get; private set; } = new();

    public static bool IsSaved => ActivePath.Name.Length > 0;

    public static int CurrentKeyframe { get; private set; } = -1;

    public static event Action? PathChanged;

    public static void Load(FilmPath path)
    {
        PathPlayer.Stop();

        ActivePath = path;
        CurrentKeyframe = -1;

        PathChanged?.Invoke();
    }

    public static void New() => Load(new FilmPath());

    public static KeyframeOutcome AddKeyframe() => InsertKeyframe(ActivePath.Keyframes.Count);

    public static KeyframeOutcome InsertKeyframe(int index)
    {
        if (!Freecam.IsActive)
        {
            return KeyframeOutcome.NotInFreecam;
        }

        var path = ActivePath;

        if (path.Keyframes.Count >= FilmPath.MaxKeyframes)
        {
            return KeyframeOutcome.Full;
        }

        if (path.Keyframes.Count == 0)
        {
            path.Space = TargetLock.IsFollowing ? PathSpace.Target : PathSpace.World;
            path.Attach = TargetLock.Mode;
        }
        else if (!SpaceMatches(path))
        {
            return KeyframeOutcome.SpaceMismatch;
        }

        var frame = FilmKeyframe.From(Freecam.PoseIn(path.Space, path.Attach));

        if (path.Keyframes.Count > 0)
        {
            var previous = path.Keyframes[Math.Clamp(index - 1, 0, path.Keyframes.Count - 1)];

            frame.Duration = previous.Duration;
            frame.Blend = previous.Blend;
            frame.Ease = previous.Ease;
        }

        CurrentKeyframe = Math.Clamp(index, 0, path.Keyframes.Count);

        path.Keyframes.Insert(CurrentKeyframe, frame);

        Changed();

        return KeyframeOutcome.Done;
    }

    public static KeyframeOutcome ReplaceKeyframe(int index)
    {
        if (!Freecam.IsActive)
        {
            return KeyframeOutcome.NotInFreecam;
        }

        var path = ActivePath;

        if ((uint)index >= (uint)path.Keyframes.Count)
        {
            return KeyframeOutcome.Empty;
        }

        if (!SpaceMatches(path))
        {
            return KeyframeOutcome.SpaceMismatch;
        }

        path.Keyframes[index].CopyPose(Freecam.PoseIn(path.Space, path.Attach));

        Changed();

        return KeyframeOutcome.Done;
    }

    public static KeyframeOutcome RemoveLastKeyframe() => RemoveKeyframe(ActivePath.Keyframes.Count - 1);

    public static KeyframeOutcome RemoveKeyframe(int index)
    {
        var frames = ActivePath.Keyframes;

        if ((uint)index >= (uint)frames.Count)
        {
            return KeyframeOutcome.Empty;
        }

        PathPlayer.Stop();

        frames.RemoveAt(index);

        CurrentKeyframe = Math.Min(CurrentKeyframe, frames.Count - 1);

        Changed();

        return KeyframeOutcome.Done;
    }

    public static void MoveKeyframe(int index, int offset)
    {
        var frames = ActivePath.Keyframes;
        var target = index + offset;

        if ((uint)index >= (uint)frames.Count || (uint)target >= (uint)frames.Count)
        {
            return;
        }

        (frames[index], frames[target]) = (frames[target], frames[index]);

        Changed();
    }

    public static void Changed()
    {
        if (IsSaved)
        {
            FilmStores.Paths.Save(ActivePath, replacing: true);
        }

        PathChanged?.Invoke();
    }

    public static PlayOutcome Play()
    {
        var outcome = Playable(2);

        if (outcome == PlayOutcome.Started)
        {
            PathPlayer.Start();
        }

        return outcome;
    }

    public static PlayOutcome Preview(float fraction)
    {
        var outcome = Playable(1);

        if (outcome == PlayOutcome.Started)
        {
            PathPlayer.Seek(Math.Clamp(fraction, 0f, 1f) * PathPlayer.Length(ActivePath));
        }

        return outcome;
    }

    public static PlayOutcome JumpToKeyframe(int index)
    {
        var outcome = Playable(1);

        if (outcome != PlayOutcome.Started)
        {
            return outcome;
        }

        var path = ActivePath;

        CurrentKeyframe = Math.Clamp(index, 0, path.Keyframes.Count - 1);

        Freecam.SetPose(Freecam.ToWorld(path.Keyframes[CurrentKeyframe].ToPose(), path.Space, path.Attach));

        PathChanged?.Invoke();

        return PlayOutcome.Started;
    }

    public static PlayOutcome StepKeyframe(int offset)
    {
        var count = ActivePath.Keyframes.Count;

        if (CurrentKeyframe < 0 || CurrentKeyframe >= count)
        {
            return JumpToKeyframe(offset < 0 ? count - 1 : 0);
        }

        return JumpToKeyframe((CurrentKeyframe + offset + count) % count);
    }

    private static bool SpaceMatches(FilmPath path) =>
        (path.Space == PathSpace.Target) == TargetLock.IsFollowing
        && (!TargetLock.IsFollowing || path.Attach == TargetLock.Mode);

    private static PlayOutcome Playable(int minimumKeyframes)
    {
        if (!Freecam.IsActive)
        {
            return PlayOutcome.NotInFreecam;
        }

        if (ActivePath.Keyframes.Count < minimumKeyframes)
        {
            return PlayOutcome.TooFewKeyframes;
        }

        return ActivePath.Space == PathSpace.Target && !TargetLock.IsValid
            ? PlayOutcome.NeedsTarget
            : PlayOutcome.Started;
    }
}

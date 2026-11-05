using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Freecam;

public enum LoopMode
{
    Once,
    Loop,
    PingPong,
    Circuit,
}

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused,
}

public static class PathPlayer
{
    public static PlaybackState State { get; private set; }

    public static LoopMode Loop
    {
        get => (LoopMode)Math.Clamp(UserDefaults.FreecamLoop.Value, 0, (int)LoopMode.Circuit);
        set => UserDefaults.FreecamLoop.Value = (int)value;
    }

    public static float Speed
    {
        get => UserDefaults.FreecamPlaybackSpeed.Value;
        set => UserDefaults.FreecamPlaybackSpeed.Value = value;
    }

    public static float StartDelay
    {
        get => UserDefaults.FreecamStartDelay.Value;
        set => UserDefaults.FreecamStartDelay.Value = value;
    }

    public static bool StayOnLastFrame
    {
        get => UserDefaults.FreecamStayOnLastFrame.Value;
        set => UserDefaults.FreecamStayOnLastFrame.Value = value;
    }

    public static bool HideHud
    {
        get => UserDefaults.FreecamPlaybackHideHud.Value;
        set => UserDefaults.FreecamPlaybackHideHud.Value = value;
    }

    public static bool Letterbox
    {
        get => UserDefaults.FreecamPlaybackLetterbox.Value;
        set => UserDefaults.FreecamPlaybackLetterbox.Value = value;
    }

    public static float Time { get; private set; }

    public static bool IsActive => State != PlaybackState.Stopped;

    public static event Action? Changed;

    public static bool Closed => Loop == LoopMode.Circuit;

    private static int _direction = 1;

    private static float _delay;

    internal static void Start()
    {
        Time = 0f;
        _direction = 1;
        _delay = StartDelay;

        SetState(PlaybackState.Playing);
    }

    public static void Stop() => SetState(PlaybackState.Stopped);

    public static void TogglePause()
    {
        if (State == PlaybackState.Playing)
        {
            SetState(PlaybackState.Paused);
        }
        else if (State == PlaybackState.Paused)
        {
            _delay = 0f;

            SetState(PlaybackState.Playing);
        }
    }

    internal static void Seek(float time)
    {
        Time = MathF.Max(0f, time);
        _delay = 0f;

        SetState(PlaybackState.Paused);
    }

    internal static CameraPose Advance(FilmPath path, float delta, out bool finished)
    {
        finished = false;

        var length = Length(path);

        if (State == PlaybackState.Playing)
        {
            if (_delay > 0f)
            {
                _delay -= delta;
            }
            else
            {
                Time += delta * Speed * _direction;

                if (Time >= length || Time <= 0f)
                {
                    finished = Bounce(length);
                }
            }
        }

        Time = Math.Clamp(Time, 0f, length);

        return PathMath.Sample(path, Time, Closed);
    }

    public static float Length(FilmPath path) => path.Duration(Closed);

    private static bool Bounce(float length)
    {
        switch (Loop)
        {
            case LoopMode.Loop:
            case LoopMode.Circuit:
                Time = length > 0f ? Time % length : 0f;

                return false;

            case LoopMode.PingPong:
                _direction = Time >= length ? -1 : 1;

                return false;

            default:
                return Time >= length;
        }
    }

    private static void SetState(PlaybackState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;

        Changed?.Invoke();
    }
}

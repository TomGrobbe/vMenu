using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Freecam;

public enum FocusMode
{
    Manual,
    Target,
}

public enum ShakeType
{
    None,
    Handheld,
    Drunk,
    RoadVibration,
    AirTurbulence,
    Vibrate,
}

public static class CameraEffects
{
    private const float MinFocus = 0.5f;

    private const float MaxFocus = 1000f;

    private const float LetterboxBar = 0.12f;

    public static bool DofEnabled
    {
        get => UserDefaults.FreecamDof.Value;
        set => UserDefaults.FreecamDof.Value = value;
    }

    public static FocusMode Focus
    {
        get => (FocusMode)Math.Clamp(UserDefaults.FreecamFocusMode.Value, 0, (int)FocusMode.Target);
        set => UserDefaults.FreecamFocusMode.Value = (int)value;
    }

    public static float FocusDistance
    {
        get => Math.Clamp(UserDefaults.FreecamFocusDistance.Value, MinFocus, MaxFocus);
        set => UserDefaults.FreecamFocusDistance.Value = Math.Clamp(value, MinFocus, MaxFocus);
    }

    public static int DofAperture
    {
        get => UserDefaults.FreecamDofAperture.Value;
        set => UserDefaults.FreecamDofAperture.Value = value;
    }

    public static int MotionBlur
    {
        get => UserDefaults.FreecamMotionBlur.Value;
        set => UserDefaults.FreecamMotionBlur.Value = value;
    }

    public static ShakeType Shake
    {
        get => (ShakeType)Math.Clamp(UserDefaults.FreecamShake.Value, 0, (int)ShakeType.Vibrate);
        set => UserDefaults.FreecamShake.Value = (int)value;
    }

    public static int ShakeIntensity
    {
        get => UserDefaults.FreecamShakeIntensity.Value;
        set => UserDefaults.FreecamShakeIntensity.Value = value;
    }

    public static bool HideHud
    {
        get => UserDefaults.FreecamHideHud.Value;
        set => UserDefaults.FreecamHideHud.Value = value;
    }

    public static bool Letterbox
    {
        get => UserDefaults.FreecamLetterbox.Value;
        set => UserDefaults.FreecamLetterbox.Value = value;
    }

    private static int _shakeCamera;

    private static ShakeType _appliedShake;

    private static float _appliedAmplitude;

    internal static void Apply(int camera, float focus)
    {
        ApplyShake(camera);

        Native.SetCamMotionBlurStrength(camera, MotionBlur / 100f);

        if (!DofEnabled)
        {
            Native.SetCamUseShallowDofMode(camera, false);

            return;
        }

        var aperture = Math.Clamp(DofAperture, 0, 100) / 100f;
        var distance = Math.Clamp(focus, MinFocus, MaxFocus);
        var band = distance * PathMath.Lerp(1.5f, 0.05f, aperture);

        Native.SetUseHiDof();
        Native.SetCamUseShallowDofMode(camera, true);
        Native.SetCamNearDof(camera, MathF.Max(0f, distance - (band * 0.5f)));
        Native.SetCamFarDof(camera, distance + (band * 0.5f));
        Native.SetCamDofStrength(camera, 1f);
        Native.SetCamDofFnumberOfLens(camera, PathMath.Lerp(22f, 0.5f, aperture));
        Native.SetCamDofFocalLengthMultiplier(camera, PathMath.Lerp(2f, 2.5f, aperture));
        Native.SetCamDofMaxNearInFocusDistance(camera, distance);
        Native.SetCamDofMaxNearInFocusDistanceBlendLevel(camera, 1f);
    }

    internal static void DrawLetterbox()
    {
        Native.DrawRect(0.5f, LetterboxBar / 2f, 1f, LetterboxBar, 0, 0, 0, 255, false);
        Native.DrawRect(0.5f, 1f - (LetterboxBar / 2f), 1f, LetterboxBar, 0, 0, 0, 255, false);
    }

    internal static void Forget()
    {
        _shakeCamera = 0;
        _appliedShake = ShakeType.None;
        _appliedAmplitude = 0f;
    }

    private static float Amplitude() =>
        MathF.Max(0.05f, (0.076f * ShakeIntensity * ShakeIntensity) - (0.29f * ShakeIntensity) + 0.3f);

    private static void ApplyShake(int camera)
    {
        if (camera != _shakeCamera || Shake != _appliedShake)
        {
            Native.StopCamShaking(camera, true);

            _shakeCamera = camera;
            _appliedShake = Shake;
            _appliedAmplitude = Amplitude();

            if (ShakeName(Shake) is { } name)
            {
                Native.ShakeCam(camera, name, _appliedAmplitude);
            }

            return;
        }

        var amplitude = Amplitude();

        if (Shake != ShakeType.None && amplitude != _appliedAmplitude)
        {
            _appliedAmplitude = amplitude;

            Native.SetCamShakeAmplitude(camera, amplitude);
        }
    }

    private static string? ShakeName(ShakeType shake) => shake switch
    {
        ShakeType.Handheld => "HAND_SHAKE",
        ShakeType.Drunk => "DRUNK_SHAKE",
        ShakeType.RoadVibration => "ROAD_VIBRATION_SHAKE",
        ShakeType.AirTurbulence => "SKY_DIVING_SHAKE",
        ShakeType.Vibrate => "VIBRATE_SHAKE",
        _ => null,
    };
}

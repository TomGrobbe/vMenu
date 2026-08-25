using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

public static class Speedometer
{
    public const int Off = 0;

    public const int Kmh = 1;

    public const int Mph = 2;

    public const int Both = 3;

    public const int BottomRight = 0;

    public const int BottomCenter = 1;

    private const long RefreshIntervalMs = 100;

    private const float MetersPerSecondToKmh = 3.6f;

    private const float MetersPerSecondToMph = 2.23694f;

    private const float InstructionalButtonsHeight = 1f / 18f;

    private const float HudStreetTop = 1f / 50f;

    private const float HudAreaTop = 1f / 18f;

    private const float HudVehicleTop = 1f / 10f;

    private const float HudTextClearance = 1f / 80f;

    // Engine, body and tank health all top out here.
    private const float FullHealth = 1000f;

    private const int VehicleNameComponent = 6;

    private const int AreaNameComponent = 7;

    private const int VehicleClassComponent = 8;

    private const int StreetNameComponent = 9;

    private const string HideMessage = """{"type":"speedometer","visible":false}""";

    private static TickHandle? _tick;

    private static bool _inVehicle;

    private static bool _shown;

    private static int _paintedMode = Off;

    private static int _paintedKmh = -1;

    private static int _paintedMph = -1;

    private static bool _paintedHealth;

    private static int _paintedEngine = -1;

    private static int _paintedBody = -1;

    private static int _paintedTank = -1;

    private static int _paintedPosition = -1;

    private static float _paintedRight = -1f;

    private static float _paintedBottom = -1f;

    public static int Mode
    {
        get => UserDefaults.DisplaySpeedometer.Value;

        set
        {
            if (UserDefaults.DisplaySpeedometer.Value == value)
            {
                return;
            }

            UserDefaults.DisplaySpeedometer.Value = value;

            Reevaluate();
        }
    }

    public static int Position
    {
        get => UserDefaults.DisplaySpeedometerPosition.Value;

        set => UserDefaults.DisplaySpeedometerPosition.Value = value;
    }

    public static bool ShowHealth
    {
        get => UserDefaults.DisplayVehicleHealth.Value && IsHealthAllowed;

        set
        {
            if (value && !IsHealthAllowed)
            {
                return;
            }

            if (UserDefaults.DisplayVehicleHealth.Value == value)
            {
                return;
            }

            UserDefaults.DisplayVehicleHealth.Value = value;

            Reevaluate();
        }
    }

    private static bool IsHealthAllowed =>
        ClientPermissions.IsAllowed(DisplaySettingsPermissions.VehicleHealth);

    public static void Initialize()
    {
        LocalVehicleTicks.VehicleChanged += OnVehicleChanged;

        ClientPermissions.PermissionsChanged += Reevaluate;

        _tick = TickRegistry.Register(
            "Misc.Speedometer",
            Flush,
            TickRate.Every(RefreshIntervalMs),
            () => (Mode != Off || ShowHealth) && _inVehicle,
            autoStart: false);
    }

    public static void Restore()
    {
        _inVehicle = Native.IsPedInAnyVehicle(Native.PlayerPedId(), false);

        Reevaluate();
    }

    private static void OnVehicleChanged(VehicleChanged changed)
    {
        _inVehicle = changed.Vehicle is not null;

        Reevaluate();
    }

    private static void Reevaluate()
    {
        _tick?.Reevaluate();

        if ((Mode == Off && !ShowHealth) || !_inVehicle)
        {
            Hide();
        }
    }

    private static void Hide()
    {
        _paintedKmh = -1;
        _paintedMph = -1;
        _paintedEngine = -1;
        _paintedBody = -1;
        _paintedTank = -1;

        if (!_shown)
        {
            return;
        }

        _shown = false;

        Native.SendNuiMessage(HideMessage);
    }

    private static void Flush()
    {
        if (!Hud.CanDraw)
        {
            Hide();

            return;
        }

        var vehicle = Native.GetVehiclePedIsIn(Native.PlayerPedId(), false);

        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            Hide();

            return;
        }

        var mode = Mode;
        var position = Position;
        var health = ShowHealth;
        var speed = Native.GetEntitySpeed(vehicle);

        var kmh = mode is Kmh or Both ? (int)MathF.Round(speed * MetersPerSecondToKmh) : 0;
        var mph = mode is Mph or Both ? (int)MathF.Round(speed * MetersPerSecondToMph) : 0;

        // Whole percentages, so the panel is not repainted for a fraction nobody can see. Engine health
        // goes negative once it is beyond saving, which the clamp turns into an empty bar.
        var engine = health ? Percent(Native.GetVehicleEngineHealth(vehicle)) : -1;
        var body = health ? Percent(Native.GetVehicleBodyHealth(vehicle)) : -1;
        var tank = health ? Percent(Native.GetVehiclePetrolTankHealth(vehicle)) : -1;

        var lift = MenuController.IsAnyMenuOpen() || NoClip.NoClip.IsActive ? InstructionalButtonsHeight : 0f;

        if (position == BottomRight)
        {
            var text = HudTextTop();

            if (text > 0f)
            {
                lift = MathF.Max(lift, text + HudTextClearance);
            }
        }

        var inset = MathF.Round((1f - Native.GetSafeZoneSize()) / 2f, 5);
        var bottom = MathF.Round(inset + lift, 5);

        if (mode == _paintedMode
            && position == _paintedPosition
            && health == _paintedHealth
            && kmh == _paintedKmh
            && mph == _paintedMph
            && engine == _paintedEngine
            && body == _paintedBody
            && tank == _paintedTank
            && inset == _paintedRight
            && bottom == _paintedBottom)
        {
            return;
        }

        _paintedMode = mode;
        _paintedPosition = position;
        _paintedHealth = health;
        _paintedKmh = kmh;
        _paintedMph = mph;
        _paintedEngine = engine;
        _paintedBody = body;
        _paintedTank = tank;
        _paintedRight = inset;
        _paintedBottom = bottom;
        _shown = true;

        Native.SendNuiMessage(ClientJson.Serialize(new SpeedometerMessage
        {
            Kmh = mode is Kmh or Both ? kmh : null,
            Mph = mode is Mph or Both ? mph : null,
            Engine = health ? engine : null,
            Body = health ? body : null,
            Tank = health ? tank : null,
            Side = position == BottomCenter ? "center" : "right",
            Right = inset,
            Bottom = bottom,
        }));
    }

    private static int Percent(float value) =>
        (int)MathF.Round(Math.Clamp(value / FullHealth, 0f, 1f) * 100f);

    private static float HudTextTop()
    {
        if (Native.IsHudComponentActive(VehicleNameComponent) || Native.IsHudComponentActive(VehicleClassComponent))
        {
            return HudVehicleTop;
        }

        if (Native.IsHudComponentActive(AreaNameComponent))
        {
            return HudAreaTop;
        }

        return Native.IsHudComponentActive(StreetNameComponent) ? HudStreetTop : 0f;
    }

    private sealed class SpeedometerMessage
    {
        public string Type { get; } = "speedometer";

        public bool Visible { get; } = true;

        public required int? Kmh { get; init; }

        public required int? Mph { get; init; }

        public required int? Engine { get; init; }

        public required int? Body { get; init; }

        public required int? Tank { get; init; }

        public required string Side { get; init; }

        public required float Right { get; init; }

        public required float Bottom { get; init; }
    }
}

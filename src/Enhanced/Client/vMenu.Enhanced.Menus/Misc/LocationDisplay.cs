using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.World;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus.Misc;

public static class LocationDisplay
{
    private const long LocationIntervalMs = 1000;

    private const long CoordinatesIntervalMs = 500;

    private const long AnchorIntervalMs = 200;

    private const float NearNodeDistanceSquared = 1400f;

    private const string HideLocationMessage = """{"type":"location","visible":false}""";

    private const string HideCoordinatesMessage = """{"type":"coordinates","visible":false}""";

    private static TickHandle? _locationTick;

    private static TickHandle? _coordinatesTick;

    private static TickHandle? _anchorTick;

    private static bool _locationShown;

    private static bool _coordinatesShown;

    private static string _paintedLocation = string.Empty;

    private static string _paintedCoordinates = string.Empty;

    private static string _paintedAnchor = string.Empty;

    public static bool ShowLocation =>
        UserDefaults.DisplayShowLocation.Value && ClientPermissions.IsAllowed(DisplaySettingsPermissions.ShowLocation);

    public static bool ShowCoordinates =>
        UserDefaults.DisplayShowCoordinates.Value && ClientPermissions.IsAllowed(DisplaySettingsPermissions.ShowCoordinates);

    private static bool AnchorWanted =>
        ShowLocation || WeatherForecast.CompactShown || WeatherForecast.ClockOnlyShown;

    public static void Initialize()
    {
        _locationTick = TickRegistry.Register(
            "Misc.LocationDisplay",
            FlushLocation,
            TickRate.Every(LocationIntervalMs),
            () => ShowLocation,
            autoStart: false);

        _coordinatesTick = TickRegistry.Register(
            "Misc.CoordinatesDisplay",
            FlushCoordinates,
            TickRate.Every(CoordinatesIntervalMs),
            () => ShowCoordinates,
            autoStart: false);

        _anchorTick = TickRegistry.Register(
            "Misc.LocationAnchor",
            FlushAnchor,
            TickRate.Every(AnchorIntervalMs),
            () => AnchorWanted,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Reevaluate;
    }

    public static void Restore() => Reevaluate();

    public static void RefreshAnchor()
    {
        _anchorTick?.Reevaluate();

        if (AnchorWanted)
        {
            FlushAnchor();
        }
    }

    public static void SetShowLocation(bool show)
    {
        UserDefaults.DisplayShowLocation.Value = show;

        Reevaluate();
    }

    public static void SetShowCoordinates(bool show)
    {
        UserDefaults.DisplayShowCoordinates.Value = show;

        Reevaluate();
    }

    private static void Reevaluate()
    {
        _locationTick?.Reevaluate();
        _coordinatesTick?.Reevaluate();
        _anchorTick?.Reevaluate();

        if (!ShowLocation)
        {
            HideLocation();
        }

        if (!ShowCoordinates)
        {
            HideCoordinates();
        }
    }

    private static void FlushLocation()
    {
        if (!Hud.CanDraw)
        {
            HideLocation();

            return;
        }

        var ped = Native.PlayerPedId();
        var position = Native.GetEntityCoords(ped, true);
        var localizer = Localizer.Current;

        Native.GetStreetNameAtCoord(position.X, position.Y, position.Z, out var street, out var crossing);

        var message = ClientJson.Serialize(new LocationMessage
        {
            Street = Native.GetStreetNameFromHashKey((uint)street),
            Crossing = crossing == 0 ? null : Native.GetStreetNameFromHashKey((uint)crossing),
            Zone = ZoneName(position),
            Near = IsOffRoad(position),
            NearLabel = localizer.Get(Loc.DisplaySettings.LocationNear),
            Compass = localizer.Get(CompassKey(Native.GetEntityHeading(ped))),
        });

        if (message == _paintedLocation)
        {
            return;
        }

        _paintedLocation = message;
        _locationShown = true;

        Native.SendNuiMessage(message);
    }

    private static void FlushCoordinates()
    {
        if (!Hud.CanDraw)
        {
            HideCoordinates();

            return;
        }

        var ped = Native.PlayerPedId();
        var position = Native.GetEntityCoords(ped, true);

        var message = ClientJson.Serialize(new CoordinatesMessage
        {
            X = Format(position.X),
            Y = Format(position.Y),
            Z = Format(position.Z),
            Heading = Format(Native.GetEntityHeading(ped)),
            HeadingLabel = Localizer.Current.Get(Loc.DisplaySettings.CoordinatesHeading),
            Side = UserPreferences.IsRightAligned ? "left" : "right",
            Inset = HudAnchor.Fraction(HudAnchor.Inset),
        });

        if (message == _paintedCoordinates)
        {
            return;
        }

        _paintedCoordinates = message;
        _coordinatesShown = true;

        Native.SendNuiMessage(message);
    }

    private static void FlushAnchor()
    {
        var (left, bottom, width) = HudAnchor.AboveMinimap();

        var message = ClientJson.Serialize(new AnchorMessage
        {
            Anchor = new AnchorBox
            {
                Left = HudAnchor.Fraction(left),
                Bottom = HudAnchor.Fraction(bottom),
                Width = HudAnchor.Fraction(width),
            },
        });

        if (message == _paintedAnchor)
        {
            return;
        }

        _paintedAnchor = message;

        Native.SendNuiMessage(message);
    }

    private static void HideLocation()
    {
        _paintedLocation = string.Empty;
        _paintedAnchor = string.Empty;

        if (!_locationShown)
        {
            return;
        }

        _locationShown = false;

        Native.SendNuiMessage(HideLocationMessage);
    }

    private static void HideCoordinates()
    {
        _paintedCoordinates = string.Empty;

        if (!_coordinatesShown)
        {
            return;
        }

        _coordinatesShown = false;

        Native.SendNuiMessage(HideCoordinatesMessage);
    }

    private static string ZoneName(Vector3 position) =>
        Native.GetLabelText(Native.GetNameOfZone(position.X, position.Y, position.Z));

    private static bool IsOffRoad(Vector3 position) =>
        !Native.GetNthClosestVehicleNode(position.X, position.Y, position.Z, 0, out var node, 0, 0f, 0f)
        || Vector3.DistanceSquared(position, node) > NearNodeDistanceSquared;

    private static string CompassKey(float heading) => heading switch
    {
        > 320f or < 45f => Loc.DisplaySettings.CompassNorth,
        <= 135f => Loc.DisplaySettings.CompassWest,
        < 225f => Loc.DisplaySettings.CompassSouth,
        _ => Loc.DisplaySettings.CompassEast,
    };

    private static string Format(float value) =>
        MathF.Round(value, 2).ToString("0.00", CultureInfo.InvariantCulture);

    private sealed class AnchorBox
    {
        public required float Left { get; init; }

        public required float Bottom { get; init; }

        public required float Width { get; init; }
    }

    private sealed class LocationMessage
    {
        public string Type { get; } = "location";

        public bool Visible { get; } = true;

        public required string Street { get; init; }

        public string? Crossing { get; init; }

        public required string Zone { get; init; }

        public required bool Near { get; init; }

        public required string NearLabel { get; init; }

        public required string Compass { get; init; }
    }

    private sealed class AnchorMessage
    {
        public string Type { get; } = "hud";

        public required AnchorBox Anchor { get; init; }
    }

    private sealed class CoordinatesMessage
    {
        public string Type { get; } = "coordinates";

        public bool Visible { get; } = true;

        public required string X { get; init; }

        public required string Y { get; init; }

        public required string Z { get; init; }

        public required string Heading { get; init; }

        public required string HeadingLabel { get; init; }

        public required string Side { get; init; }

        public required float Inset { get; init; }
    }
}

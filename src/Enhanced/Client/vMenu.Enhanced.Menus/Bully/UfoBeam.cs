using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Bully;

internal static class UfoBeam
{
    private const int BeamMarker = 1;

    private const float BeamWidth = 7.5f;

    private const int BeamRed = 93;

    private const int BeamGreen = 182;

    private const int BeamBlue = 229;

    private const int BeamAlpha = 60;

    private const float DrawDistance = 500f;

    private const int RemoteLifetimeMs = 30000;

    private static readonly Dictionary<int, RemoteBeam> Remote = [];

    private static readonly List<int> Expired = [];

    private static int _marker;

    private static TickHandle? _tick;

    public static void Initialize()
    {
        API.OnNetEvent(BullyEvents.ShowAbductionBeam, new Action<string, string, string, string, string, string>(OnShow), false);

        _tick = TickRegistry.Register("Bully.RemoteUfoBeams", DrawRemote, TickRate.PerFrame, () => Remote.Count > 0);

        BullyState.Stopped += Clear;
    }

    public static void Announce(bool on, Vector3 ground, float height) =>
        API.EmitServer(
            BullyEvents.AbductionBeam,
            on ? BullyEvents.On : BullyEvents.Off,
            Text(ground.X),
            Text(ground.Y),
            Text(ground.Z),
            Text(height));

    public static void Draw(Vector3 ground, float height)
    {
        Native.DrawLightWithRange(ground.X, ground.Y, ground.Z + 1f, BeamRed, BeamGreen, BeamBlue, 10f, 50f);

        while (_marker < Markers.Length)
        {
            try
            {
                Markers[_marker](ground, height);

                return;
            }
            catch (Exception exception)
            {
                Log.Warning($"[Bully] UFO beam marker {_marker} is unavailable: {exception.Message}");

                _marker++;
            }
        }
    }

    private static readonly Action<Vector3, float>[] Markers =
    [
        static (ground, height) => Native.DrawMarker(
            BeamMarker,
            ground.X, ground.Y, ground.Z - 1f,
            0f, 0f, 0f,
            0f, 0f, 0f,
            BeamWidth, BeamWidth, height,
            BeamRed, BeamGreen, BeamBlue, BeamAlpha,
            false, false, 2, false, string.Empty, string.Empty, false),
        static (ground, height) => Native.DrawMarker_2(
            BeamMarker,
            ground.X, ground.Y, ground.Z - 1f,
            0f, 0f, 0f,
            0f, 0f, 0f,
            BeamWidth, BeamWidth, height,
            BeamRed, BeamGreen, BeamBlue, BeamAlpha,
            false, false, 2, false, string.Empty, string.Empty, false, true, false),
    ];

    private static void OnShow(string source, string state, string x, string y, string z, string height)
    {
        if (!int.TryParse(source, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId))
        {
            return;
        }

        if (state != BullyEvents.On)
        {
            Remote.Remove(serverId);

            _tick?.Reevaluate();

            return;
        }

        if (!TryParse(x, out var px) || !TryParse(y, out var py) || !TryParse(z, out var pz) || !TryParse(height, out var ph))
        {
            return;
        }

        Remote[serverId] = new RemoteBeam(new Vector3(px, py, pz), ph, Native.GetGameTimer() + RemoteLifetimeMs);

        _tick?.Reevaluate();
    }

    private static void DrawRemote()
    {
        var now = Native.GetGameTimer();
        var camera = Native.GetFinalRenderedCamCoord();

        Expired.Clear();

        foreach (var pair in Remote)
        {
            if (now > pair.Value.ExpiresAt)
            {
                Expired.Add(pair.Key);

                continue;
            }

            if (Vector3.Distance(camera, pair.Value.Ground) <= DrawDistance)
            {
                Draw(pair.Value.Ground, pair.Value.Height);
            }
        }

        if (Expired.Count == 0)
        {
            return;
        }

        foreach (var serverId in Expired)
        {
            Remote.Remove(serverId);
        }

        _tick?.Reevaluate();
    }

    private static void Clear()
    {
        Remote.Clear();

        _tick?.Reevaluate();
    }

    private static string Text(float value) => value.ToString(CultureInfo.InvariantCulture);

    private static bool TryParse(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

    private sealed class RemoteBeam(Vector3 ground, float height, int expiresAt)
    {
        public Vector3 Ground { get; } = ground;

        public float Height { get; } = height;

        public int ExpiresAt { get; } = expiresAt;
    }
}

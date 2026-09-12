using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Integration.Server;

// Serves off the tick thread where a native would fault, so ticks fill these and the socket only reads.
public static class IntegrationSnapshots
{
    private const int ForecastCount = 6;

    private static volatile string _status = "{}";

    private static volatile string _players = "{\"count\":0,\"players\":[]}";

    private static volatile string _map = "{\"count\":0,\"players\":[]}";

    private static volatile string _blips = "{\"alwaysOn\":[],\"toggleable\":[]}";

    private static volatile string _world = "{}";

    private static volatile string _worldSignature = "";

    private static volatile string _key = "";

    public static string Status => _status;

    public static string Players => _players;

    public static string Map => _map;

    public static string Blips => _blips;

    public static string World => _world;

    public static string WorldSignature => _worldSignature;

    public static string Key => _key;

    // Main thread only: everything here reads the game through natives.
    public static void Update()
    {
        _key = IntegrationAuth.Key;
        _status = ServerJson.Serialize(IntegrationStatus.Current());
        _players = ServerJson.Serialize(OnlinePlayersResponse.Current());
        _map = ServerJson.Serialize(MapResponse.Current());

        var world = WorldSnapshot.Capture(ForecastCount);
        _world = ServerJson.Serialize(world);
        _worldSignature = Signature(world);

        var blips = LocationBlipActions.Payload;
        if (!string.IsNullOrEmpty(blips) && blips != "{}")
        {
            _blips = blips;
        }
    }

    private static string Signature(WorldSnapshot world) =>
        $"{world.Weather.Override}|{world.Weather.Scheduled}|{world.Weather.Blackout}|" +
        $"{world.Weather.Snow}|{world.Weather.SnowFalling}|{world.Clock.Frozen}|" +
        $"{world.Clock.OffsetSeconds}|{world.Clock.Speed}|{world.Sync.Weather}|{world.Sync.Time}";
}

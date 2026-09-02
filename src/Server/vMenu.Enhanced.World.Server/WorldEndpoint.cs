using System.Globalization;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;

using WorldApiSettings = vMenu.Enhanced.Data.Configuration.Settings.WorldApi;

namespace vMenu.Enhanced.World.Server;

// A read only view of the world over HTTP, on the port the server already listens on, so a Discord
// bot or a website can ask what the sky is doing without reimplementing the weather table.
public static class WorldEndpoint
{
    private const string Path = "/world";

    private const string TokenHeader = "X-vMenu-Token";

    private const string TokenQuery = "token";

    private const int DefaultForecast = 10;

    private const int MaxForecast = 48;

    // A scanner hitting a closed endpoint should not be able to fill the console.
    private const double RefusalLogIntervalSeconds = 10.0;

    private static double _lastRefusalLoggedAt;

    public static void Initialize()
    {
        NativeFixer.SetHttpHandler(Handle);

        Log.Debug(
            IsConfigured
                ? $"[WorldApi] Answering GET {Path} for callers holding the {WorldApiSettings.Token.Name} token."
                : $"[WorldApi] Switched off, because {WorldApiSettings.Token.Name} is empty.");
    }

    private static bool IsConfigured => Token.Length > 0;

    private static string Token => ServerConfig.Value(WorldApiSettings.Token);

    private static void Handle(object? request, object? response)
    {
        var call = HttpCall.From(request, response);

        try
        {
            Route(call);
        }
        catch (Exception exception)
        {
            // An escaping exception would leave the caller on a socket that never answers.
            Log.Error($"[WorldApi] {exception.GetType().Name} answering {call.Path}: {exception.Message}");

            call.Reply(500, "text/plain", "vMenu Enhanced could not work out the world state.\n");
        }
    }

    private static void Route(HttpCall call)
    {
        if (call.Path is not (Path or Path + "/"))
        {
            call.Reply(404, "text/plain", $"vMenu Enhanced serves {Path} and nothing else.\n");

            return;
        }

        if (call.Method != "GET")
        {
            call.Reply(405, "text/plain", "vMenu Enhanced only answers GET here.\n");

            return;
        }

        if (!IsConfigured)
        {
            call.Reply(
                503,
                "text/plain",
                "The vMenu Enhanced world endpoint is switched off. Set " +
                $"{WorldApiSettings.Token.Name} in your server config to a long random string, " +
                "restart, and send that string back as an " + TokenHeader + " header.\n");

            return;
        }

        if (!Matches(Presented(call), Token))
        {
            Refused(call);

            call.Reply(401, "text/plain", $"That token is not the one {WorldApiSettings.Token.Name} holds.\n");

            return;
        }

        call.Reply(200, "application/json", ServerJson.Serialize(WorldSnapshot.Capture(ForecastCount(call))));
    }

    private static string Presented(HttpCall call)
    {
        var header = call.Header(TokenHeader);

        return header.Length > 0 ? header : call.QueryValue(TokenQuery);
    }

    private static int ForecastCount(HttpCall call)
    {
        var asked = call.QueryValue("forecast");

        return asked.Length > 0 && int.TryParse(asked, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
            ? Math.Clamp(count, 0, MaxForecast)
            : DefaultForecast;
    }

    // Compares every character, so the time taken never says how much of the token was right.
    private static bool Matches(string presented, string expected)
    {
        var difference = presented.Length ^ expected.Length;

        for (var index = 0; index < presented.Length; index++)
        {
            difference |= presented[index] ^ expected[index % expected.Length];
        }

        return difference == 0;
    }

    private static void Refused(HttpCall call)
    {
        var now = ServerClock.Now();

        if (now - _lastRefusalLoggedAt < RefusalLogIntervalSeconds)
        {
            return;
        }

        _lastRefusalLoggedAt = now;

        Log.Warning(
            $"[WorldApi] Refused a request from {call.Address} carrying the wrong token. Further " +
            $"refusals are logged at most once every {RefusalLogIntervalSeconds:0} seconds.");
    }
}

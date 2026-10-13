using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Http.Server;

// World endpoints only: Cfx rate limits on FXserver make this http handler unreliable for anything more.
public static class HttpRouter
{
    private static readonly List<Route> Routes = [];

    private static bool _installed;

    public static void Initialize()
    {
        if (_installed)
        {
            return;
        }

        _installed = true;

        NativeFixer.SetHttpHandler(Dispatch);
    }

    public static void Map(string path, Action<HttpCall> handler) => Routes.Add(new Route(path, handler));

    private static void Dispatch(MessagePackBuffer? request, MessagePackBuffer? response)
    {
        var call = HttpCall.From(request, response);

        try
        {
            foreach (var route in Routes)
            {
                if (route.Matches(call.Path))
                {
                    route.Handler(call);

                    return;
                }
            }

            call.Reply(404, "text/plain", "vMenu Enhanced does not serve that path.\n");
        }
        catch (Exception exception)
        {
            Log.Error($"[Http] {exception.GetType().Name} answering {call.Path}: {exception.Message}");

            call.Reply(500, "text/plain", "vMenu Enhanced could not answer that request.\n");
        }
    }

    private sealed class Route(string path, Action<HttpCall> handler)
    {
        public Action<HttpCall> Handler { get; } = handler;

        public bool Matches(string requestPath) =>
            string.Equals(requestPath, path, StringComparison.Ordinal)
            || string.Equals(requestPath, path + "/", StringComparison.Ordinal);
    }
}

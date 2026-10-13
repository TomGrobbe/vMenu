using CitizenFX.Core.Native;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.MenuFramework;

public static class NuiCallbacks
{
    public static void Register(string callback, Action<string> handler) =>
        API.RegisterRawNuiCallback(callback, new Action<object, object>((request, _) => handler(BodyOf(request))));

    private static string BodyOf(object? request) => request switch
    {
        IDictionary<string, object> map when map.TryGetValue("body", out var body) => body as string ?? string.Empty,
        _ => Unreadable(request),
    };

    private static string Unreadable(object? request)
    {
        Log.Error($"[Nui] A callback arrived as {request?.GetType().FullName ?? "null"}, which has no body this can read.");

        return string.Empty;
    }
}

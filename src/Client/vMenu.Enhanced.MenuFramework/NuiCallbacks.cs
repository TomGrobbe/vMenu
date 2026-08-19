using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>The one place a page is given something to post back to.</summary>
// Raw NUI callbacks, because an ordinary one is dispatched as an event whose source is
// "nui:<resource>", which this runtime parses as a player id and throws on. Three things about raw
// ones are not negotiable: the reference must come from the core's own registry or the host answers
// "Invalid function", only the request may be declared because the second argument is a function
// reference that will not deserialize, and the page must post JSON because the body is parsed before
// anything is dispatched.
// Bug report: https://github.com/citizenfx/rfc/discussions/257
public static class NuiCallbacks
{
    /// <param name="handler">Handed the request body exactly as the page posted it.</param>
    public static void Register(string callback, Action<string> handler)
    {
        // To be fixed when https://github.com/citizenfx/rfc/discussions/257 and https://github.com/citizenfx/rfc/discussions/350 are solved
#pragma warning disable FIVEM001 // The only registry the host invokes from.
        var reference = SharedAPI.GetCore().FuncRefManager.Register(new Action<object>(request => handler(BodyOf(request))));
#pragma warning restore FIVEM001

        Native.RegisterRawNuiCallback(callback, (int)reference);
    }

    private static string BodyOf(object? request) => request switch
    {
        IDictionary<object, object> map when map.TryGetValue("body", out var body) => body as string ?? string.Empty,
        IDictionary<string, object> map when map.TryGetValue("body", out var body) => body as string ?? string.Empty,
        _ => Unreadable(request),
    };

    private static string Unreadable(object? request)
    {
        Log.Error($"[Nui] A callback arrived as {request?.GetType().FullName ?? "null"}, which has no body this can read.");

        return string.Empty;
    }
}

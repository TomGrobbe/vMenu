using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.NativeHooks;

/// <summary>
/// Runs around every <em>Native.*</em> call in the client, through the forwarder class that
/// vMenu.Enhanced.NativeHooks.Generator emits. Only the NativeHooks configuration builds it,
/// so nothing here costs anything in a debug or release build. Fill the bodies in with
/// whatever you need.
/// </summary>
public static class NativeHook
{
    /// <summary>Runs just before the native is invoked.</summary>
    /// <param name="name">Name of the native, for example <em>PlayerPedId</em>.</param>
    public static void Before(string name)
    {
        if (!(name is "ProfilerEnterScope" or "ProfilerExitScope"))
        {
            Native.ProfilerEnterScope($"NativeFunction.{name}");
        }
    }

    /// <summary>Runs after the native returns, including when it threw.</summary>
    /// <param name="name">Name of the native, for example <em>PlayerPedId</em>.</param>
    public static void After(string name)
    {
        if (!(name is "ProfilerEnterScope" or "ProfilerExitScope"))
        {
            Native.ProfilerExitScope();
        }
    }

    /// <summary>Runs when the native throws. The exception is rethrown afterwards.</summary>
    /// <param name="name">Name of the native, for example <em>PlayerPedId</em>.</param>
    /// <param name="exception">What the native threw.</param>
    public static void OnException(string name, Exception exception)
    {
    }
}

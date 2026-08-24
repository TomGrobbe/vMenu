using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.NativeHooks;

/// <summary>Runs around every <em>Native.*</em> call in the client, through the forwarder class
/// that vMenu.Enhanced.NativeHooks.Generator emits. Only the NativeHooks configuration builds it,
/// so nothing here costs anything in a debug or release build. Fill the bodies in as needed.</summary>
public static class NativeHook
{
    /// <summary>Runs just before the native is invoked.</summary>
    public static void Before(string name)
    {
        if (!(name is "ProfilerEnterScope" or "ProfilerExitScope"))
        {
            Native.ProfilerEnterScope($"NativeFunction.{name}");
        }
    }

    /// <summary>Runs after the native returns, including when it threw.</summary>
    public static void After(string name)
    {
        if (!(name is "ProfilerEnterScope" or "ProfilerExitScope"))
        {
            Native.ProfilerExitScope();
        }
    }

    /// <summary>Runs when the native throws. The exception is rethrown afterwards.</summary>
    public static void OnException(string name, Exception exception)
    {
    }
}

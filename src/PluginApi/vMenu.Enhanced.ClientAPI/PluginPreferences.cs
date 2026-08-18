using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>
/// Persists player preferences in this resource's own key value store, so a persisted checkbox
/// reopens the way the player left it. The data lives with the plugin resource, never in vMenu,
/// and disappears with it.
/// </summary>
internal static class PluginPreferences
{
    private const string KeyPrefix = "vmenu_plugin_pref_";

    internal static bool? ReadBool(string itemId)
    {
        var raw = Native.GetResourceKvpString(KeyPrefix + itemId);

        if (string.Equals(raw, "true", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(raw, "false", StringComparison.Ordinal))
        {
            return false;
        }

        return null;
    }

    internal static void WriteBool(string itemId, bool value) =>
        Native.SetResourceKvp(KeyPrefix + itemId, value ? "true" : "false");
}

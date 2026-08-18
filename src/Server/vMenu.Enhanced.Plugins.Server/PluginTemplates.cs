using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Plugins.Server;

/// <summary>
/// Writes the two templates a registered plugin gets, into <c>config/plugins/</c> under names
/// starting with the plugin's resource name. Rewritten on every registration, so what an owner
/// reads is always what the running plugin declared.
/// </summary>
// Their own files rather than lines appended to vMenu's own permissions.cfg and configuration.cfg:
// those two describe vMenu, and a server owner who drops a plugin should be able to drop everything
// that came with it in one go.
public static class PluginTemplates
{
    public static void Write(RegisteredServerPlugin plugin)
    {
        WritePermissions(plugin);
        WriteSettings(plugin);
    }

    // Written even when the plugin declares nothing, so a plugin that took its last permission or
    // setting away leaves an empty template behind rather than the previous one, which an owner
    // would otherwise read as still current.
    private static void WritePermissions(RegisteredServerPlugin plugin)
    {
        var entries = PermissionRegistry.EnumerateSubtree(PluginPermissions.AllFor(plugin.Id))
            .Select(static entry => new PermissionExampleEntry(
                entry.Node.Name,
                entry.Depth,
                entry.Node.Source,
                entry.Node.IsStaffOnly,
                entry.Node.ExtraParents));

        Save(
            PermissionsExample.PluginResourcePath(plugin.Resource),
            PermissionsExample.RenderForPlugin(plugin.Resource, plugin.DisplayName, entries));
    }

    private static void WriteSettings(RegisteredServerPlugin plugin)
    {
        Save(
            ConfigurationExample.PluginResourcePath(plugin.Resource),
            ConfigurationExample.RenderForPlugin(plugin.Resource, plugin.DisplayName, plugin.Settings));
    }

    private static void Save(string path, string contents)
    {
        var resource = Native.GetCurrentResourceName();

        if (NativeFixer.SaveResourceFile(resource, path, contents))
        {
            Log.Debug($"[Plugins] Wrote {path}.");
            return;
        }

        // The folder ships with vMenu, so a failure here is about permission and nothing else.
        Log.Error(
            $"[Plugins] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}. If you deleted {ExampleFile.PluginsDirectory}/, put it "
            + "back: nothing can create it again.");
    }
}

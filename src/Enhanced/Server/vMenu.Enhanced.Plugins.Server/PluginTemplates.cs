using System.Text;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Plugins.Server;

// Writes the two templates a registered plugin gets, into config/plugins/ under names starting with
// the plugin's resource name. Rewritten on every registration, so what an owner reads is always what
// the running plugin declared. Their own files rather than lines appended to vMenu's own configs,
// because a server owner who drops a plugin should be able to drop everything that came with it.
public static class PluginTemplates
{
    public static void Write(RegisteredServerPlugin plugin)
    {
        WritePermissions(plugin);
        WriteSettings(plugin);
    }

    // Written even when the plugin declares nothing, so a plugin that took its last permission or
    // setting away leaves an empty template behind rather than the previous one, which an owner would
    // otherwise read as still current.
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
        var bytes = Encoding.UTF8.GetBytes(contents);

        if (Native.SaveResourceFile(resource, path, bytes))
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

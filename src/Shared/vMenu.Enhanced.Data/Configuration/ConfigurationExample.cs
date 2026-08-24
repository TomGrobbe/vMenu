using System.Text;

namespace vMenu.Enhanced.Data.Configuration;

public static class ConfigurationExample
{
    public const string CopyName = "configuration.cfg";

    public static string ResourcePath => $"{ExampleFile.ConfigDirectory}/{CopyName}{ExampleFile.Extension}";

    // Where one plugin's own settings template lives.
    public static string PluginResourcePath(string resource) =>
        $"{ExampleFile.PluginsDirectory}/{ExampleFile.PluginCopyName(resource, CopyName)}{ExampleFile.Extension}";

    // One plugin's settings on their own, for its own example file in the shared plugins folder.
    public static string RenderForPlugin(string resource, string displayName, IEnumerable<Setting> settings)
    {
        var declared = settings.ToList();
        var file = new StringBuilder();

        file.Append(ExampleFile.BannerIn(
            ExampleFile.PluginsDirectory,
            ExampleFile.PluginCopyName(resource, CopyName),
            "These options belong to the plugin '" + displayName + "', which the resource '" + resource
            + "' provides. They are listed here and not in vMenu's own configuration.cfg, so removing "
            + "the plugin means removing the files named after it rather than hunting through that one.",
            "Nothing here is written while the plugin is not running, so start it before you read "
            + "this. If you have removed the plugin for good, delete every file in this folder whose "
            + "name starts with '" + resource + ".'.",
            "Every option uses 'setr' so it is replicated to clients. Deleting an option, or "
            + "commenting it out, restores the plugin's own default for it."));

        if (declared.Count == 0)
        {
            file.Append('\n');
            file.Append(ExampleFile.Comment("This plugin declares no options, so there is nothing to set here."));

            return file.ToString();
        }

        foreach (var setting in declared)
        {
            file.Append('\n');
            file.Append(ExampleFile.Comment(setting.Description));
            file.Append("setr " + setting.Name + " " + setting.DefaultText + "\n");
        }

        return file.ToString();
    }

    public static string Render()
    {
        var file = new StringBuilder();

        file.Append(ExampleFile.Banner(
            CopyName,
            "Every option below uses 'setr' so it is replicated to clients." +
            "Deleting an option, or commenting it out, restores vMenu's own default for it. " +
            "Although I do recommend that you manually set it to the default yourself instead of " +
            "commenting it out, because if the default ever changes with an update, you won't be " +
            "surprised when it's suddenly changed in-game!"));

        foreach (var section in ConfigCatalog.Sections)
        {
            file.Append("\n### " + section.Title + " ###\n");

            foreach (var setting in section.Settings)
            {
                file.Append('\n');
                file.Append(ExampleFile.Comment(setting.Description));
                file.Append("setr " + setting.Name + " " + setting.DefaultText + "\n");
            }
        }

        return file.ToString();
    }
}

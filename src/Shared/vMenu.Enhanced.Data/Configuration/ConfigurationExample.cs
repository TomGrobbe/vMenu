using System.Text;

namespace vMenu.Enhanced.Data.Configuration;

public static class ConfigurationExample
{
    public const string CopyName = "configuration.cfg";

    public static string ResourcePath => $"{ExampleFile.ConfigDirectory}/{CopyName}{ExampleFile.Extension}";

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

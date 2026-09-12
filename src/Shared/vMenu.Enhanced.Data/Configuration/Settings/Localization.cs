namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Localization
{
    public static readonly StringSetting Languages = new("vMenu.Enhanced.Languages")
    {
        Description =
            "The languages players can pick, as a comma separated list of file names without .json, " +
            "in picker order. Each code needs a matching language/<code>.json. To add one, copy " +
            "language/example.json to language/<code>.json, translate it, and list the code here. " +
            "Don't include 'en' (English is built in and always available), nor use 'example' (a template file that is " +
            "rewritten on every vMenu update).",
        Default = "nl,de,es,fr",
    };
}

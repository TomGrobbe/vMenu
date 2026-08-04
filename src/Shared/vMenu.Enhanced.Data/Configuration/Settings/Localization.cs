namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Localization
{
    public static readonly StringSetting Languages = new("vMenu.Enhanced.Languages")
    {
        Description =
            "Which languages players can pick, as a comma separated list of the language file names " +
            "without the .json, in the order the picker lists them. A file must exist at " +
            "language/<code>.json for its code to do anything. To add your own, copy " +
            "language/example.json to language/<code>.json, translate it, and add the code here. " +
            "English is built into vMenu and is always available, so do not list 'en'. Do not list " +
            "'example' either, since that file is a generated template and is rewritten on every " +
            "vMenu build.",
        Default = "nl,de,es,fr",
    };
}

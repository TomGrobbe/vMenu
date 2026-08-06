using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using vMenu.Enhanced.Data.Configuration;

using AboutSetting = vMenu.Enhanced.Data.Configuration.Settings.About;
using DebuggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Debugging;

namespace vMenu.Enhanced.Menus;

public sealed class AboutMenu : MenuDefinition
{
    public override MenuText Title => MenuText.Literal("About vMenu");

    public override MenuText Subtitle => MenuText.Literal("About vMenu Enhanced");

    public override MenuText LinkDescription =>
        MenuText.Literal("Version information, links and credits.");

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(Fact(
            "Version",
            "The version of vMenu this server is running.",
            MenuText.From(Metadata("version"))));

        // Not the manifest's author field, which carries a legal name.
        menu.Entries.Add(Fact(
            "Author",
            "Who made vMenu.",
            MenuText.Literal("Vespura")));

        menu.Entries.Add(Fact(
            "Client Debug Mode",
            "Whether vMenu's diagnostic commands work in your own console.",
            MenuText.From(State(DebuggingSettings.Client))));

        menu.Entries.Add(Fact(
            "Server Debug Mode",
            "Whether vMenu's diagnostic commands work in the server console.",
            MenuText.From(State(DebuggingSettings.Server))));

        menu.Entries.Add(Fact(
            "Experimental Features",
            "Whether unfinished features are switched on.",
            MenuText.From(State(DebuggingSettings.ExperimentalFeatures))));

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Literal("Documentation"),
            Description = MenuText.From(() => ClientConfig.Value(AboutSetting.DocumentationUrl)),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Literal("Discord"),
            Description = MenuText.From(() => ClientConfig.Value(AboutSetting.DiscordUrl)),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Literal("Credits"),
            Description = MenuText.Literal(
                "Thank you to everyone who has contributed to vMenu over the years, and to "
                + "~b~Ricky~s~ in particular for his help creating vMenu Enhanced."),
        });
    }

    private static ButtonEntry Fact(string text, string description, MenuText value) =>
        new()
        {
            Text = MenuText.Literal(text),
            Description = MenuText.Literal(description),
            Label = value,
        };

    /// <summary>A manifest value, or a marker when the key is absent.</summary>
    private static Func<string> Metadata(string key) => () => Read(key) ?? "Unknown";

    /// <summary>A flag as words rather than as true or false.</summary>
    private static Func<string> State(BoolSetting setting) =>
        () => ClientConfig.Value(setting) ? "Enabled" : "Disabled";

    private static string? Read(string key)
    {
        var resource = Native.GetCurrentResourceName();

        if (Native.GetNumResourceMetadata(resource, key) == 0)
        {
            return null;
        }

        var value = Native.GetResourceMetadata(resource, key, 0)?.Trim();

        return string.IsNullOrEmpty(value) ? null : value;
    }
}

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus;

/// <summary>Everything saved on this machine, as one code the player can copy and paste back.</summary>
[VMenu(
    TitleKey = Loc.DataTransfer.Title,
    SubtitleKey = Loc.DataTransfer.Subtitle,
    DescriptionKey = Loc.DataTransfer.LinkDescription)]
public sealed class DataTransferMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DataTransfer.Summary),
            Description = MenuText.From(Describe),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DataTransfer.Export),
            Description = MenuText.Key(Loc.DataTransfer.ExportDescription),
            OnSelectedAsync = _ => DataTransfer.ExportAsync(),
        });

        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.Key(Loc.DataTransfer.ImportGroup),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.DataTransfer.Merge),
            Description = MenuText.Key(Loc.DataTransfer.MergeDescription),
            OnSelectedAsync = _ => DataTransfer.ImportAsync(KvpImportMode.Merge),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.DataTransfer.Replace),
            Description = MenuText.Key(Loc.DataTransfer.ReplaceDescription),
            ConfirmationDescription = MenuText.Key(Loc.DataTransfer.ReplaceConfirm),
            OnConfirmedAsync = _ => DataTransfer.ImportAsync(KvpImportMode.Replace),
        });

        // The row above is only written when the menu is relabelled, and opening it is not one of
        // those moments, so without this it would still be showing whatever was true when the menu
        // was first built. Also what makes the counts right straight after an import.
        menu.OnOpened = _ => MenuRegistry.Refresh(menu.Menu);
    }

    // Counted every time the row is resolved. That only reads the key names, never a stored value,
    // so it costs a key listing rather than anything that has to be deserialized.
    private static string Describe()
    {
        var inventory = KvpTransfer.Measure();

        var summary = inventory.Total == 0
            ? MenuText.Key(Loc.DataTransfer.SummaryEmpty)
            : MenuText.Key(
                Loc.DataTransfer.SummaryDescription,
                ("total", Number(inventory.Total)),
                ("vehicles", Number(inventory.Vehicles)),
                ("peds", Number(inventory.Peds)),
                ("characters", Number(inventory.Characters)),
                ("loadouts", Number(inventory.Loadouts)),
                ("settings", Number(inventory.Settings)));

        return summary.Resolve(Localizer.Current);
    }

    private static MenuText Number(int value) => MenuText.Literal(value.ToString());
}

using System.Globalization;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.PlayerOptions.MpStats,
    SubtitleKey = Loc.PlayerOptions.MpStatsSubtitle,
    DescriptionKey = Loc.PlayerOptions.MpStatsDescription,
    Permission = PlayerOptionsPermissions.MpStats)]
public sealed class MpStatsMenu : MenuDefinition
{
    private static MenuText[] Percentages { get; } = BuildPercentages();

    protected override void Build(MenuBuilder menu)
    {
        foreach (var stat in MpStats.All)
        {
            var current = stat;

            menu.Entries.Add(new ListEntry
            {
                Text = MenuText.Key(current.TextKey),
                Description = MenuText.From(() => Describe(current)),
                Options = Percentages,
                ReadSelectedIndex = () => MpStats.Chosen(current) / MpStats.Step,
                OnIndexChanged = changed => MpStats.SetChosen(current, changed.NewIndex * MpStats.Step),
            });
        }
    }

    private static string Describe(MpStat stat)
    {
        var localizer = Localizer.Current;
        var text = localizer.Get(stat.DescriptionKey);
        var limit = MpStats.LimitOf(stat);

        if (limit >= MpStats.Full)
        {
            return text;
        }

        return MenuText.Key(
            Loc.PlayerOptions.MpStatsLimited,
            ("description", MenuText.Literal(text)),
            ("limit", MenuText.Literal(limit.ToString(CultureInfo.InvariantCulture))))
            .Resolve(localizer);
    }

    private static MenuText[] BuildPercentages()
    {
        var options = new MenuText[(MpStats.Full / MpStats.Step) + 1];

        for (var index = 0; index < options.Length; index++)
        {
            options[index] = MenuText.Literal($"{index * MpStats.Step}%");
        }

        return options;
    }
}

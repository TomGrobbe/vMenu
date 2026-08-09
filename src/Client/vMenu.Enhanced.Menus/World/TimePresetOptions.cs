using System.Collections;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework.Localization;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>The preset times an owner listed in the convar, shaped as the list a menu row holds.</summary>
// A live view rather than a snapshot taken while the menu is built: the row is created once at start
// up, so re-reading the convar here is what lets an owner change the presets without every player
// reconnecting.
internal sealed class TimePresetOptions : IReadOnlyList<MenuText>
{
    private string _source = string.Empty;

    private List<int> _seconds = [];

    private List<MenuText> _labels = [];

    public int Count
    {
        get
        {
            Sync();

            return _labels.Count;
        }
    }

    public MenuText this[int index]
    {
        get
        {
            Sync();

            return _labels[index];
        }
    }

    public IEnumerator<MenuText> GetEnumerator()
    {
        Sync();

        return _labels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>The time behind an option, or null once the list has moved on under the selection.</summary>
    internal int? SecondOfDay(int index)
    {
        Sync();

        return index >= 0 && index < _seconds.Count ? _seconds[index] : null;
    }

    private void Sync()
    {
        var source = ClientConfig.Value(TimeOptionsSettings.Presets);

        if (string.Equals(source, _source, StringComparison.Ordinal))
        {
            return;
        }

        _source = source;

        var rejected = new List<string>();
        var seconds = TimePresets.Parse(source, rejected);
        var labels = new List<MenuText>(seconds.Count);

        foreach (var second in seconds)
        {
            labels.Add(MenuText.Literal(TimeText.Format(second)));
        }

        _seconds = seconds;
        _labels = labels;

        if (rejected.Count > 0)
        {
            API.Log.Warn(
                $"[World] Ignoring {rejected.Count} time preset(s) in {TimeOptionsSettings.Presets.Name}: " +
                string.Join(", ", rejected) +
                ". Every preset has to be four digits on a 24 hour clock, such as 0930 or 2130.");
        }
    }
}

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Weapons;

/// <summary>
/// What a weapon or a component is called on screen. The config file may name one of the game's own
/// labels, which is already translated, or write the text out.
/// </summary>
internal static class WeaponNames
{
    /// <summary>
    /// Deferred rather than resolved once, so a language change relabels it for free along with
    /// everything else.
    /// </summary>
    internal static MenuText Display(string label, string fallback) =>
        MenuText.From(() => Resolve(label, fallback));

    internal static string Resolve(string label, string fallback)
    {
        if (label.Length > 0 && Native.DoesTextLabelExist(label))
        {
            var text = Native.GetLabelText(label);

            // A label the game knows but has nothing for comes back as the "not found" marker, which
            // reads worse on a row than the spawn name does.
            if (!string.IsNullOrWhiteSpace(text) && text != "NULL")
            {
                return text;
            }
        }

        return label.Length > 0 ? label : fallback;
    }
}

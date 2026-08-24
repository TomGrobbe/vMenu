using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Players.Appearance;

// Nothing on the row itself says a helmet has a visor, and there is no reason a player would guess
// it, so the menu has to say it once. Legacy said it on every single change to the hat slot,
// including the ones that took a hat off, which is how a helpful message turns into noise.
public static class PedVisorHint
{
    private static bool _shown;

    // Forget that it was said, so the next menu session can say it again.
    public static void Reset() => _shown = false;

    // Says it, if this really is a helmet with a visor and it has not been said yet.
    public static void ShowIfHelmet(int ped, int slot)
    {
        if (_shown || slot != PedPropSlots.Hats)
        {
            return;
        }

        if (!VisorToggle.HasVisor(ped))
        {
            return;
        }

        _shown = true;

        Notifications.Info(MenuText.Key(Loc.PlayerAppearance.VisorHint));
    }
}

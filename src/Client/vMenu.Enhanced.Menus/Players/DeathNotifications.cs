using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Deaths;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Weapons;

namespace vMenu.Enhanced.Menus.Players;

public static class DeathNotifications
{
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(
            DeathEvents.Announce,
            new Action<string, string, string, string, string>(OnDeath),
            false);
    }

    private static void OnDeath(string victimId, string victimName, string killerId, string killerName, string cause)
    {
        if (!UserPreferences.AreDeathNotificationsEnabled)
        {
            return;
        }

        var killer = killerName.Length > 0 ? MenuText.Literal(killerName) : (MenuText?)null;

        Notifications.Info(
            MenuText.Key(
                Loc.DeathNotifications.Sentence,
                ("victim", MenuText.Literal(victimName)),
                ("what", Describe(killer, Hash(cause)))));
    }

    // The middle of the sentence: everything except who it happened to.
    private static MenuText Describe(MenuText? killer, uint cause)
    {
        if (DeathCauses.Find(cause) is { } known)
        {
            return killer is { } byWhom && known.ByKiller is { } phrase
                ? MenuText.Key(phrase, ("killer", byWhom))
                : MenuText.Key(known.Solo);
        }

        // Null whenever the server owner left this weapon out of their config, which is the whole reason
        // there is a wording for a kill with no weapon named.
        var weapon = WeaponHashNames.Resolve(cause);

        if (killer is { } who)
        {
            return weapon is null
                ? MenuText.Key(Loc.DeathNotifications.KilledBy, ("killer", who))
                : MenuText.Key(
                    Loc.DeathNotifications.KilledByWithWeapon,
                    ("killer", who),
                    ("weapon", MenuText.Literal(weapon)));
        }

        // An NPC with a gun: there is a weapon but nobody to name, so the weapon is all there is.
        return weapon is null
            ? MenuText.Key(Loc.DeathNotifications.Died)
            : MenuText.Key(Loc.DeathNotifications.KilledWithWeapon, ("weapon", MenuText.Literal(weapon)));
    }

    private static uint Hash(string value) =>
        uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hash) ? hash : 0;
}

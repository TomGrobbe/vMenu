using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminFreeze
{
    private static bool _frozen;

    private static bool _watching;

    private static bool _registered;

    public static bool IsFrozen => _frozen;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        ResourceShutdown.Stopping += Release;
    }

    public static void SetFrozen(bool frozen)
    {
        if (frozen == _frozen)
        {
            return;
        }

        _frozen = frozen;

        Watch(frozen);

        if (frozen)
        {
            Apply();

            return;
        }

        Native.FreezeEntityPosition(Native.PlayerPedId(), false);

        PlayerFreeze.Reapply();
    }

    private static void Release() => SetFrozen(false);

    private static void Apply()
    {
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.FreezeEntityPosition(ped, true);
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalPlayerTicks.PlayerPedIdChanged += OnPedChanged;
            LocalPlayerTicks.PlayerPedRevived += OnRevived;

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
        LocalPlayerTicks.PlayerPedRevived -= OnRevived;
    }

    private static void OnPedChanged(PlayerPedIdChanged _) => Apply();

    private static void OnRevived(PlayerPedRevived _) => Apply();
}

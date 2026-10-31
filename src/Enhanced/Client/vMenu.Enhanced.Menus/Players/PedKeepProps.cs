using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.Menus.Players;

public static class PedKeepProps
{
    private const int OnDamage = 0;

    private const int HelmetOnHeadshot = 1;

    public static void Initialize()
    {
        LocalPlayerTicks.PlayerPedIdChanged += changed => Apply(changed.NewPed);
        LocalPlayerTicks.PlayerPedRevived += revived => Apply(revived.Ped);
        StaffOnlyMode.Changed += () => Apply(Native.PlayerPedId());

        Apply(Native.PlayerPedId());
    }

    public static void Apply(int ped)
    {
        if (ped == 0)
        {
            return;
        }

        var canLose = StaffOnlyMode.Active;

        Native.SetPedCanLosePropsOnDamage(ped, canLose, OnDamage);
        Native.SetPedCanLosePropsOnDamage(ped, canLose, HelmetOnHeadshot);
    }
}

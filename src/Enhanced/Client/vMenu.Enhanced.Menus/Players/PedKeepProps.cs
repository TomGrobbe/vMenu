using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;

namespace vMenu.Enhanced.Menus.Players;

public static class PedKeepProps
{
    private const int OnDamage = 0;

    private const int HelmetOnHeadshot = 1;

    public static void Initialize()
    {
        LocalPlayerTicks.PlayerPedIdChanged += changed => Apply(changed.NewPed);
        LocalPlayerTicks.PlayerPedRevived += revived => Apply(revived.Ped);

        Apply(Native.PlayerPedId());
    }

    public static void Apply(int ped)
    {
        if (ped == 0)
        {
            return;
        }

        Native.SetPedCanLosePropsOnDamage(ped, false, OnDamage);
        Native.SetPedCanLosePropsOnDamage(ped, false, HelmetOnHeadshot);
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Events;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Players;

public static class PedKeepProps
{
    private const int OnDamage = 0;

    private const int HelmetOnHeadshot = 1;

    public static void Initialize()
    {
        LocalPlayerTicks.PlayerPedIdChanged += changed => Apply(changed.NewPed);
        LocalPlayerTicks.PlayerPedRevived += revived => Apply(revived.Ped);

        Apply(API.PlayerPedId());
    }

    public static void Apply(int ped)
    {
        if (ped == 0)
        {
            return;
        }

        API.SetPedCanLosePropsOnDamage(ped, false, OnDamage);
        API.SetPedCanLosePropsOnDamage(ped, false, HelmetOnHeadshot);
    }
}

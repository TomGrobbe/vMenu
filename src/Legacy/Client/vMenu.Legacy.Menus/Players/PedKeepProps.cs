using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;

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
        StaffOnlyMode.Changed += () => Apply(API.PlayerPedId());

        Apply(API.PlayerPedId());
    }

    public static void Apply(int ped)
    {
        if (ped == 0)
        {
            return;
        }

        var canLose = StaffOnlyMode.Active;

        API.SetPedCanLosePropsOnDamage(ped, canLose, OnDamage);
        API.SetPedCanLosePropsOnDamage(ped, canLose, HelmetOnHeadshot);
    }
}

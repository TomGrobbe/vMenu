using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminHold
{
    private const float OffsetForward = 0.5f;

    private const float OffsetUp = 0.5f;

    private const float FacingBack = 180f;

    private const int RootBone = 0;

    private const long ReattachIntervalMs = 500;

    private static TickHandle? _tick;

    private static int _holder;

    private static bool _registered;

    public static bool IsHeld => _holder != 0;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        _tick = TickRegistry.Register(
            "Admin.Hold",
            Reattach,
            TickRate.Every(ReattachIntervalMs),
            () => IsHeld);

        ResourceShutdown.Stopping += Detach;
    }

    public static void SetHolder(int holderServerId)
    {
        _holder = holderServerId;

        if (_holder == 0)
        {
            Detach();
        }
        else
        {
            Attach();
        }

        _tick?.Reevaluate();
    }

    private static void Reattach()
    {
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        if (!Native.IsEntityAttachedToAnyPed(ped))
        {
            Attach();
        }
    }

    private static void Attach()
    {
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        var carrier = Carrier();

        if (carrier == 0)
        {
            return;
        }

        Native.AttachEntityToEntity(
            ped,
            carrier,
            RootBone,
            0f, OffsetForward, OffsetUp,
            0f, 0f, FacingBack,
            false, false, false, false,
            2,
            true,
            false);
    }

    private static void Detach()
    {
        var ped = Native.PlayerPedId();

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        if (Native.IsEntityAttachedToAnyPed(ped))
        {
            Native.DetachEntity(ped, true, true);
        }
    }

    private static int Carrier()
    {
        var slot = Native.GetPlayerFromServerId(_holder);

        if (slot < 0 || !Native.NetworkIsPlayerActive(slot))
        {
            return 0;
        }

        var ped = Native.GetPlayerPed(slot);

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            Log.Debug($"[Admin] Cannot attach to #{_holder} yet: their ped is not streamed in.");

            return 0;
        }

        return ped;
    }
}

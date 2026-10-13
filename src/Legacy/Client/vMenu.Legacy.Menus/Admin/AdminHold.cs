using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Admin;

public static class AdminHold
{
    private const float OffsetForward = 0.5f;

    private const float OffsetUp = 0.5f;

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
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        if (!API.IsEntityAttachedToAnyPed(ped))
        {
            Attach();
        }
    }

    private static void Attach()
    {
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        var carrier = Carrier();

        if (carrier == 0)
        {
            return;
        }

        NativeFixer.AttachEntityToEntity(
            ped,
            carrier,
            RootBone,
            0f, OffsetForward, OffsetUp,
            0f, 0f, 0f,
            false, false, false, false,
            2,
            true,
            false);
    }

    private static void Detach()
    {
        var ped = API.PlayerPedId();

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            return;
        }

        if (API.IsEntityAttachedToAnyPed(ped))
        {
            API.DetachEntity(ped, true, true);
        }
    }

    private static int Carrier()
    {
        var slot = API.GetPlayerFromServerId(_holder);

        if (slot < 0 || !API.NetworkIsPlayerActive(slot))
        {
            return 0;
        }

        var ped = API.GetPlayerPed(slot);

        if (ped == 0 || !API.DoesEntityExist(ped))
        {
            Log.Debug($"[Admin] Cannot attach to #{_holder} yet: their ped is not streamed in.");

            return 0;
        }

        return ped;
    }
}

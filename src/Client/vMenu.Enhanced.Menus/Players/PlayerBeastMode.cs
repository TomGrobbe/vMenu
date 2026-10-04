using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerBeastMode
{
    private const int DisablePlayerVaulting = 47;

    private static TickHandle? _tick;

    public static bool Enabled => UserDefaults.PlayerBeastMode.Value && PlayerSuperJump.Enabled;

    public static void Initialize() =>
        _tick = TickRegistry.Register("Player.BeastMode", Apply, TickRate.PerFrame, () => Enabled);

    public static void SetEnabled(bool enabled)
    {
        if (enabled && !PlayerSuperJump.IsAllowed)
        {
            return;
        }

        UserDefaults.PlayerBeastMode.Value = enabled;

        if (enabled)
        {
            PlayerSuperJump.SetEnabled(true);
        }

        _tick?.Reevaluate();
    }

    internal static void Reevaluate() => _tick?.Reevaluate();

    private static void Apply()
    {
        Native.SetBeastJumpThisFrame(Native.PlayerId());
        Native.SetPedResetFlag(Native.PlayerPedId(), DisablePlayerVaulting, true);
    }
}

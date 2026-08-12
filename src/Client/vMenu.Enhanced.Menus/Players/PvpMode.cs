using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration.Settings;
using vMenu.Enhanced.Events;

namespace vMenu.Enhanced.Menus.Players;

public static class PvpMode
{
    private const int Enable = 1;

    private const int Disable = 2;

    private static bool _watching;

    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        ClientConfig.Changed += Apply;

        Apply();
    }

    private static void Apply()
    {
        var mode = ClientConfig.Value(Gameplay.PvpMode);

        Watch(mode is Enable or Disable);

        if (mode is not (Enable or Disable))
        {
            return;
        }

        Native.NetworkSetFriendlyFireOption(mode == Enable);

        WritePed(Native.PlayerPedId());
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

            return;
        }

        LocalPlayerTicks.PlayerPedIdChanged -= OnPedChanged;
    }

    private static void OnPedChanged(PlayerPedIdChanged changed) => WritePed(changed.NewPed);

    private static void WritePed(int ped)
    {
        var mode = ClientConfig.Value(Gameplay.PvpMode);

        if (mode is not (Enable or Disable) || ped == 0 || !Native.DoesEntityExist(ped))
        {
            return;
        }

        Native.SetCanAttackFriendly(ped, mode == Enable, false);
    }
}

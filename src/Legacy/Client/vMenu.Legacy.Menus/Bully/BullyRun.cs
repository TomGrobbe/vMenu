using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Bullying;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Bully;

internal sealed class BullyRun(int option, List<int> victims, Action<string> report)
{
    private bool _reported;

    public int Option { get; } = option;

    public List<int> Victims { get; } = victims;

    public void Started() => Report(BullyEvents.Started);

    public void Busy() => Report(BullyEvents.RefusedBusy);

    public void Failed() => Report(BullyEvents.RefusedFailed);

    public void Skip(string reason) => Report(reason);

    public static int PedOf(int serverId)
    {
        if (serverId == API.GetPlayerServerId(API.PlayerId()))
        {
            return API.PlayerPedId();
        }

        var player = API.GetPlayerFromServerId(serverId);

        return player == -1 ? 0 : API.GetPlayerPed(player);
    }

    private void Report(string outcome)
    {
        if (_reported)
        {
            return;
        }

        _reported = true;

        report(outcome);
    }
}

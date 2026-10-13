using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;

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
        if (serverId == Native.GetPlayerServerId(Native.PlayerId()))
        {
            return Native.PlayerPedId();
        }

        var player = Native.GetPlayerFromServerId(serverId);

        return player == -1 ? 0 : Native.GetPlayerPed(player);
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

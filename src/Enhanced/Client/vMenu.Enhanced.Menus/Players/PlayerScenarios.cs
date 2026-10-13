using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

namespace vMenu.Enhanced.Menus.Players;

public static class PlayerScenarios
{
    private const float TooFast = 5f;

    private const float SeatBack = -0.5f;

    private const float SeatDown = -0.5f;

    private static string _playing = string.Empty;

    private static bool IsAllowed => ClientPermissions.IsAllowed(PlayerOptionsPermissions.Scenarios);

    public static void Play(int index)
    {
        if (!IsAllowed || index < 0 || index >= PedScenarios.All.Length)
        {
            return;
        }

        var scenario = PedScenarios.All[index];

        if (_playing == scenario.Name && Native.IsPedUsingScenario(Native.PlayerPedId(), scenario.Name))
        {
            Stop();

            return;
        }

        if (!CanPlay(out var refusal))
        {
            Notifications.Warning(refusal);

            return;
        }

        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);

        _playing = scenario.Name;

        if (scenario.PositionBased)
        {
            var seat = Native.GetOffsetFromEntityInWorldCoords(ped, 0f, SeatBack, SeatDown);

            Native.TaskStartScenarioAtPosition(
                ped, scenario.Name, seat.X, seat.Y, seat.Z, Native.GetEntityHeading(ped), -1, true, false);

            return;
        }

        Native.TaskStartScenarioInPlace(ped, scenario.Name, 0, true);
    }

    public static void Stop()
    {
        _playing = string.Empty;

        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);
        Native.ClearPedSecondaryTask(ped);
    }

    public static void ForceStop()
    {
        _playing = string.Empty;

        var ped = Native.PlayerPedId();

        Native.ClearPedTasks(ped);
        Native.ClearPedTasksImmediately(ped);
    }

    private static bool CanPlay(out MenuText refusal)
    {
        var ped = Native.PlayerPedId();

        refusal = MenuText.Empty;

        if (Native.IsEntityDead(ped, false))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioDead);
        }
        else if (Native.IsPlayerInCutscene(Native.PlayerId()))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioCutscene);
        }
        else if (Native.NetworkIsInSpectatorMode())
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioSpectating);
        }
        else if (!Native.IsPedOnFoot(ped))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioNotOnFoot);
        }
        else if (Native.IsPedFalling(ped) || Native.IsPedRagdoll(ped))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioFalling);
        }
        else if (Native.IsPedRunning(ped) || Native.GetEntitySpeed(ped) > TooFast)
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioMoving);
        }

        return refusal.IsEmpty;
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.PedModels;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Permissions;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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

        if (_playing == scenario.Name && API.IsPedUsingScenario(API.PlayerPedId(), scenario.Name))
        {
            Stop();

            return;
        }

        if (!CanPlay(out var refusal))
        {
            Notifications.Warning(refusal);

            return;
        }

        var ped = API.PlayerPedId();

        API.ClearPedTasks(ped);

        _playing = scenario.Name;

        if (scenario.PositionBased)
        {
            var seat = API.GetOffsetFromEntityInWorldCoords(ped, 0f, SeatBack, SeatDown);

            API.TaskStartScenarioAtPosition(
                ped, scenario.Name, seat.X, seat.Y, seat.Z, API.GetEntityHeading(ped), -1, true, false);

            return;
        }

        API.TaskStartScenarioInPlace(ped, scenario.Name, 0, true);
    }

    public static void Stop()
    {
        _playing = string.Empty;

        var ped = API.PlayerPedId();

        API.ClearPedTasks(ped);
        API.ClearPedSecondaryTask(ped);
    }

    public static void ForceStop()
    {
        _playing = string.Empty;

        var ped = API.PlayerPedId();

        API.ClearPedTasks(ped);
        API.ClearPedTasksImmediately(ped);
    }

    private static bool CanPlay(out MenuText refusal)
    {
        var ped = API.PlayerPedId();

        refusal = MenuText.Empty;

        if (NativeFixer.IsEntityDead(ped, false))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioDead);
        }
        else if (API.IsPlayerInCutscene(API.PlayerId()))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioCutscene);
        }
        else if (API.NetworkIsInSpectatorMode())
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioSpectating);
        }
        else if (!API.IsPedOnFoot(ped))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioNotOnFoot);
        }
        else if (API.IsPedFalling(ped) || API.IsPedRagdoll(ped))
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioFalling);
        }
        else if (API.IsPedRunning(ped) || API.GetEntitySpeed(ped) > TooFast)
        {
            refusal = MenuText.Key(Loc.PlayerOptions.ScenarioMoving);
        }

        return refusal.IsEmpty;
    }
}

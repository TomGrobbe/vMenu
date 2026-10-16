using CitizenFX.Core;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;

using CutscenesPermissions = vMenu.Enhanced.Data.Permissions.Menus.Cutscenes;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class CutsceneActions
{
    private const long PrivateWorldWindowMs = 30 * 1000;

    private static readonly ActionRateLimit Limit = new(
        "cutscenes",
        OnlinePlayerSettings.ActionLimit,
        OnlinePlayerSettings.ActionLimitSeconds);

    public static void Register() =>
        ActionRegistry.Register(ActionIds.Cutscenes.Play, CutscenesPermissions.Menu, Play, Limit);

    private static ActionResponse Play(Player source, string[] args)
    {
        if (BullyPrivateWorlds.IsBusy(source.ServerId()))
        {
            return ActionResponse.Refused(BullyEvents.RefusedBusy);
        }

        BullyPrivateWorlds.Allow(source.ServerId(), Environment.TickCount64 + PrivateWorldWindowMs);

        return ActionResponse.Ok();
    }
}

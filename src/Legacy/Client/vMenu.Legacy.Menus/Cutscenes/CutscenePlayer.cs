using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Data.Cutscenes;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Menus.Bully;

namespace vMenu.Enhanced.Menus.Cutscenes;

internal delegate Task CutscenePlay(int ped, Func<bool> stopped, Action started, Action failed);

public static class CutscenePlayer
{
    private static readonly Dictionary<string, CutscenePlay> Scenes = new(StringComparer.Ordinal)
    {
        [CutsceneCatalog.HaircutRoast] = HaircutRoast.PlayAsync,
        [CutsceneCatalog.BridgePullOver] = BridgePullOver.PlayAsync,
    };

    private static bool _playing;

    internal static bool IsPlaying => _playing;

    private static bool IsBusy => _playing || Relocation.IsAway || AlienAbduction.IsActive;

    public static void Initialize()
    {
        HaircutRoast.Initialize();
        BridgePullOver.Initialize();

        foreach (var entry in CutsceneCatalog.All.Where(entry => !Scenes.ContainsKey(entry.Id)))
        {
            Log.Warning($"[Cutscenes] '{entry.Id}' is in the catalog but has no scene to play.");
        }
    }

    internal static async Task FromMenuAsync(string id)
    {
        var ped = API.PlayerPedId();

        if (!Scenes.TryGetValue(id, out var play))
        {
            Notifications.Error(MenuText.Key(Loc.Cutscenes.DidNotWork));

            return;
        }

        if (IsBusy)
        {
            Notifications.Warning(MenuText.Key(Loc.Cutscenes.Busy));

            return;
        }

        if (!API.IsPedOnFoot(ped))
        {
            Notifications.Warning(MenuText.Key(Loc.Cutscenes.NeedsOnFoot));

            return;
        }

        _playing = true;

        try
        {
            if (await AllowedAsync())
            {
                await play(ped, static () => false, static () => { }, static () => Notifications.Error(MenuText.Key(Loc.Cutscenes.DidNotWork)));
            }
        }
        finally
        {
            _playing = false;
        }
    }

    internal static async Task FromBullyAsync(BullyRun run, string id)
    {
        var ped = API.PlayerPedId();

        if (!Scenes.TryGetValue(id, out var play))
        {
            run.Failed();

            return;
        }

        if (IsBusy)
        {
            run.Busy();

            return;
        }

        if (!API.IsPedOnFoot(ped))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _playing = true;

        var stopAllCount = BullyState.StopAllCount;

        try
        {
            await play(ped, () => stopAllCount != BullyState.StopAllCount, run.Started, run.Failed);
        }
        finally
        {
            _playing = false;
        }
    }

    private static async Task<bool> AllowedAsync()
    {
        var result = await ServerActions.InvokeAsync(ActionIds.Cutscenes.Play);

        switch (result.Status)
        {
            case ActionStatus.Ok:
                return true;
            case ActionStatus.RateLimited when result.Data.Length > 0:
                Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.TooManyActions, ("seconds", MenuText.Literal(result.Data[0]))));
                return false;
            case ActionStatus.Refused when result.Data.Length > 0 && result.Data[0] == BullyEvents.RefusedBusy:
                Notifications.Warning(MenuText.Key(Loc.Cutscenes.Busy));
                return false;
            default:
                Notifications.Error(MenuText.Key(Loc.Cutscenes.DidNotWork));
                return false;
        }
    }
}

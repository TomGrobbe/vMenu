using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Data.Cutscenes;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Menus.Cutscenes;

using BullySettings = vMenu.Enhanced.Data.Configuration.Settings.Bully;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Bully;

public static class BullyReceiver
{
    private static readonly Dictionary<string, Func<BullyRun, Task>> Effects = new(StringComparer.Ordinal)
    {
        [BullyEffects.Explode] = Sync(PedPranks.Explode),
        [BullyEffects.Atomizer] = Sync(PedPranks.Atomize),
        [BullyEffects.Stun] = PedPranks.Stun,
        [BullyEffects.Fire] = PedPranks.Fire,
        [BullyEffects.Ragdoll] = Sync(PedPranks.Ragdoll),
        [BullyEffects.Jump] = Sync(PedPranks.Jump),
        [BullyEffects.Dance] = AnimationPranks.Dance,
        [BullyEffects.Drunk] = ScreenPranks.Drunk,
        [BullyEffects.Drugged] = ScreenPranks.Drugged,
        [BullyEffects.Timecycle] = ScreenPranks.Timecycle,
        [BullyEffects.Sound] = ScreenPranks.Sound,
        [BullyEffects.Fireworks] = ScreenPranks.Firework,
        [BullyEffects.Beast] = ScreenPranks.Beast,
        [BullyEffects.Chaff] = ScreenPranks.Chaff,
        [BullyEffects.Carjack] = Carjackers.Start,
        [BullyEffects.Mug] = Mugger.Start,
        [BullyEffects.Clowns] = ClownAttack.Start,
        [BullyEffects.Cougar] = CougarAttack.Start,
        [BullyEffects.Clone] = EvilClone.Start,
        [BullyEffects.Teleport] = Relocation.TeleportAndReturn,
        [BullyEffects.Abduct] = AlienAbduction.Start,
        [BullyEffects.Float] = VehiclePranks.Float,
        [BullyEffects.Transform] = VehiclePranks.Transform,
    };

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        foreach (var entry in CutsceneCatalog.All)
        {
            Effects[entry.Id] = run => CutscenePlayer.FromBullyAsync(run, entry.Id);
        }

        Script.EventHandlers[BullyEvents.Apply] += new Action<string, string, string, string>(OnApply);
        Script.EventHandlers[BullyEvents.Toggle] += new Action<string, string, string>(OnToggle);
        Script.EventHandlers[BullyEvents.StopAll] += new Action(BullyState.StopAll);

        // Shutdown handlers run in this order: the transform puts the real vehicle back before tracked spawns are deleted.
        VehiclePranks.Initialize();
        AlienAbduction.Initialize();
        Relocation.Initialize();
        Mugger.Initialize();
        BullySpawnCleanup.Initialize();
        ScreenPranks.Initialize();
        ToggleEffects.Initialize();
        UfoBeam.Initialize();

        ResourceShutdown.Stopping += BullyState.StopAll;

        BaseScript.TriggerServerEvent(BullyEvents.Ready);
    }

    private static Func<BullyRun, Task> Sync(Action effect) => _ =>
    {
        effect();

        return Task.CompletedTask;
    };

    private static void OnApply(string requestId, string effect, string option, string victims)
    {
        int.TryParse(option, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index);

        var run = new BullyRun(index, Victims(victims), outcome => BaseScript.TriggerServerEvent(BullyEvents.Result, requestId, outcome));

        if (!ClientConfig.Value(BullySettings.Enabled))
        {
            run.Skip(BullyEvents.RefusedDisabled);

            return;
        }

        if (CutscenePlayer.IsPlaying)
        {
            run.Busy();

            return;
        }

        if (!Effects.TryGetValue(effect, out var work))
        {
            Log.Warning($"[Bully] Ignoring an unknown effect '{effect}'.");

            run.Failed();

            return;
        }

        if (BullyEffects.Find(effect)?.TurnedOffBy is { } setting && ClientConfig.Value(setting))
        {
            run.Skip(BullyEvents.RefusedTurnedOff);

            return;
        }

        BullyTask.Run(() => RunAsync(work, run), effect);
    }

    private static async Task RunAsync(Func<BullyRun, Task> work, BullyRun run)
    {
        try
        {
            await work(run);

            run.Started();
        }
        catch
        {
            run.Failed();

            throw;
        }
    }

    private static List<int> Victims(string list)
    {
        var victims = new List<int>();

        foreach (var part in list.Split(BullyEvents.ListSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId))
            {
                victims.Add(serverId);
            }
        }

        if (victims.Count == 0)
        {
            victims.Add(API.GetPlayerServerId(API.PlayerId()));
        }

        return victims;
    }

    private static void OnToggle(string toggle, string state, string scope)
    {
        if (BullyToggles.IsKnown(toggle))
        {
            BullyState.Set(toggle, state == BullyEvents.On, scope == BullyEvents.ServerScope);
        }
    }
}

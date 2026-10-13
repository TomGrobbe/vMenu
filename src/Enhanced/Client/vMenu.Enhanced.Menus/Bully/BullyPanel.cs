using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Bully;

internal sealed class BullyPanel
{
    private static readonly (string Group, string[] Effects)[] Groups =
    [
        (Loc.Bully.PranksGroup,
        [
            BullyEffects.Explode, BullyEffects.Atomizer, BullyEffects.Stun, BullyEffects.Fire, BullyEffects.Ragdoll,
            BullyEffects.Jump, BullyEffects.Dance, BullyEffects.Drunk, BullyEffects.Drugged,
            BullyEffects.Timecycle,
        ]),
        (Loc.Bully.ScaresGroup,
        [
            BullyEffects.Sound, BullyEffects.Fireworks, BullyEffects.Beast, BullyEffects.Clone, BullyEffects.Teleport,
            BullyEffects.Abduct, BullyEffects.Haircut,
        ]),
        (Loc.Bully.AttackersGroup, [BullyEffects.Carjack, BullyEffects.Mug, BullyEffects.Clowns, BullyEffects.Cougar]),
        (Loc.Bully.VehicleGroup, [BullyEffects.Float, BullyEffects.Transform]),
    ];

    private readonly Func<(int ServerId, string Name)?>? _target;

    private readonly HashSet<string> _active = new(StringComparer.Ordinal);

    private MenuBuilder? _menu;

    private bool _includeSelf;

    private BullyPanel(Func<(int ServerId, string Name)?>? target) => _target = target;

    private bool Everyone => _target is null;

    public static BullyPanel ForPlayer(Func<(int ServerId, string Name)?> target) => new(target);

    public static BullyPanel ForSelf() => new(static () =>
        (Native.GetPlayerServerId(Native.PlayerId()), Native.GetPlayerName(Native.PlayerId())));

    public static BullyPanel ForEveryone() => new(null);

    public void Build(MenuBuilder menu)
    {
        _menu = menu;

        if (Everyone)
        {
            menu.Entries.Add(new CheckboxEntry
            {
                Text = MenuText.Key(Loc.Bully.IncludeSelf),
                Description = MenuText.Key(Loc.Bully.IncludeSelfDescription),
                ReadState = () => _includeSelf,
                OnChanged = changed => _includeSelf = changed.Checked,
            });
        }
        else
        {
            menu.OnOpenedAsync = _ => ReadTogglesAsync();
        }

        foreach (var (group, effects) in Groups)
        {
            menu.Entries.Add(new SeparatorEntry { Text = MenuText.Key(group) });
            menu.Entries.AddRange(effects.Select(EffectRow));
        }

        if (Everyone)
        {
            return;
        }

        menu.Entries.Add(new SeparatorEntry { Text = MenuText.Key(Loc.Bully.ContinuousGroup) });
        menu.Entries.AddRange(BullyToggles.All.Select(ToggleRow));
    }

    private MenuEntry EffectRow(string effect) => effect switch
    {
        BullyEffects.Timecycle => OptionRow(effect, ScreenPranks.Timecycles.Count, Loc.Bully.TimecycleOption),
        BullyEffects.Sound => OptionRow(effect, ScreenPranks.SoundCount, Loc.Bully.SoundOption),
        _ => new ButtonEntry
        {
            Text = MenuText.Key(Loc.Bully.EffectName(effect)),
            Description = MenuText.Key(Loc.Bully.EffectDescription(effect)),
            Gate = TurnedOnGate(effect),
            Behaviour = GateBehaviour.Lock,
            LockedDescription = MenuText.Key(Loc.Bully.TurnedOff),
            OnSelectedAsync = _ => ApplyAsync(effect, string.Empty),
        },
    };

    private ListEntry OptionRow(string effect, int count, Func<int, string> key) => new()
    {
        Text = MenuText.Key(Loc.Bully.EffectName(effect)),
        Description = MenuText.Key(Loc.Bully.EffectDescription(effect)),
        Gate = TurnedOnGate(effect),
        Behaviour = GateBehaviour.Lock,
        LockedDescription = MenuText.Key(Loc.Bully.TurnedOff),
        Options = Enumerable.Range(0, count).Select(index => MenuText.Key(key(index))).ToList(),
        OnSelectedAsync = selected => ApplyAsync(effect, selected.SelectedIndex.ToString(CultureInfo.InvariantCulture)),
    };

    private static MenuGate TurnedOnGate(string effect) =>
        BullyEffects.Find(effect)?.TurnedOffBy is { } setting
            ? MenuGate.When(() => !ClientConfig.Value(setting))
            : MenuGate.Always;

    private CheckboxEntry ToggleRow(string toggle) => new()
    {
        Text = MenuText.Key(Loc.Bully.ToggleName(toggle)),
        Description = MenuText.Key(Loc.Bully.ToggleDescription(toggle)),
        ReadState = () => _active.Contains(toggle),
        OnChangedAsync = changed => SetToggleAsync(toggle, changed.Checked),
    };

    private async Task ApplyAsync(string effect, string option)
    {
        if (Everyone)
        {
            var everyone = await ServerActions.InvokeAsync(
                ActionIds.Bully.Everyone(effect),
                _includeSelf ? BullyEvents.On : BullyEvents.Off,
                option);

            if (everyone.Status == ActionStatus.Ok && everyone.Data.Length > 0)
            {
                Notifications.Success(MenuText.Key(Loc.Bully.SentEveryone, ("count", MenuText.Literal(everyone.Data[0]))));
            }
            else
            {
                Report(everyone, string.Empty);
            }

            return;
        }

        if (_target?.Invoke() is not { } target)
        {
            return;
        }

        var result = await ServerActions.InvokeAsync(ActionIds.Bully.Effect(effect), Id(target.ServerId), option);

        if (result.Status == ActionStatus.Ok)
        {
            var unconfirmed = result.Data.Length > 1 && result.Data[1] == BullyEvents.Unconfirmed;

            Notifications.Success(MenuText.Key(
                unconfirmed ? Loc.Bully.SentUnconfirmed : Loc.Bully.Sent,
                ("player", MenuText.Literal(target.Name))));
        }
        else
        {
            Report(result, target.Name);
        }
    }

    private async Task SetToggleAsync(string toggle, bool on)
    {
        if (_target?.Invoke() is not { } target)
        {
            return;
        }

        var result = await ServerActions.InvokeAsync(ActionIds.Bully.Toggle(toggle, on), Id(target.ServerId));

        if (result.Status == ActionStatus.Ok && result.Data.Length > 0)
        {
            var nowOn = result.Data[0] == BullyEvents.On;

            _ = nowOn ? _active.Add(toggle) : _active.Remove(toggle);

            Notifications.Success(MenuText.Key(
                nowOn ? Loc.Bully.ToggleOn : Loc.Bully.ToggleOff,
                ("player", MenuText.Literal(target.Name))));
        }
        else
        {
            Report(result, target.Name);
        }

        Refresh();
    }

    private async Task ReadTogglesAsync()
    {
        _active.Clear();

        if (_target?.Invoke() is { } target
            && await ServerActions.InvokeAsync(ActionIds.Bully.GetToggles, Id(target.ServerId)) is { Status: ActionStatus.Ok } result)
        {
            _active.UnionWith(result.Data);
        }

        Refresh();
    }

    private void Refresh()
    {
        if (_menu is { } menu)
        {
            MenuRegistry.Refresh(menu.Menu);
        }
    }

    public static void Report(ActionResult result, string player)
    {
        if (result.Status == ActionStatus.RateLimited && result.Data.Length > 0)
        {
            Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.TooManyActions, ("seconds", MenuText.Literal(result.Data[0]))));

            return;
        }

        if (result.Status == ActionStatus.Refused && result.Data.Length > 0)
        {
            var refusal = result.Data[0] switch
            {
                BullyEvents.RefusedOnFoot => Loc.Bully.NeedsOnFoot,
                BullyEvents.RefusedDriving => Loc.Bully.NeedsDriving,
                BullyEvents.RefusedTurnedOff => Loc.Bully.TurnedOff,
                BullyEvents.RefusedBusy => Loc.Bully.Busy,
                BullyEvents.RefusedNoVehicle => Loc.Bully.NoVehicle,
                BullyEvents.RefusedFailed => Loc.Bully.DidNotWork,
                BullyEvents.RefusedIntoxicated => Loc.Bully.AlreadyIntoxicated,
                _ => Loc.Bully.Disabled,
            };

            var name = result.Data.Length > 1 ? result.Data[1] : player;

            Notifications.Warning(MenuText.Key(refusal, ("player", MenuText.Literal(name))));

            return;
        }

        var key = result.Status switch
        {
            ActionStatus.Denied => Loc.OnlinePlayers.Denied,
            ActionStatus.NotFound => Loc.OnlinePlayers.NotFound,
            ActionStatus.NotReady => Loc.OnlinePlayers.StillJoining,
            _ => Loc.OnlinePlayers.Failed,
        };

        Notifications.Error(MenuText.Key(key, ("player", MenuText.Literal(player))));
    }

    private static string Id(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);
}

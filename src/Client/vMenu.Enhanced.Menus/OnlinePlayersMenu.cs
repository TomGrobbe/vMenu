using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Plugins;
using vMenu.Enhanced.Ticks;

using OnlinePlayersPermissions = vMenu.Enhanced.Data.Permissions.Menus.OnlinePlayers;

namespace vMenu.Enhanced.Menus;

// The list is a snapshot the server hands over when the menu opens. It deliberately does not keep
// itself up to date: rows arriving or disappearing under the cursor would shuffle an alphabetical
// list around while somebody is reading it. The subtitle turns red instead, and closing and
// reopening the menu takes a fresh one.
[VMenu(
    TitleKey = Loc.OnlinePlayers.Title,
    DescriptionKey = Loc.OnlinePlayers.LinkDescription,
    Permission = OnlinePlayersPermissions.Menu)]
public sealed class OnlinePlayersMenu : MenuDefinition
{
    // Just over twice what fits on screen, so paging stays the quicker way to cross a busy server.
    private const int PlayersPerPage = 24;

    // Long enough for the longest identifier anybody actually has.
    private const int SearchMaxLength = 64;

    private const int QueryDisplayLength = 16;

    private const string DeletedVehicle = "1";

    private const string PassengerOnly = "2";

    private const string SummonMoved = "0";

    private const string SummonNearby = "1";

    private const string SummonRiding = "2";

    private const string SummonNoVehicle = "3";

    private const int DriverSeat = -1;

    private const string StatusOn = "1";

    private const string StatusOff = "0";

    private const int NoClipPollMs = 1000;

    private readonly List<OnlinePlayer> _players = [];

    private MenuBuilder? _menu;

    private DetachedMenu? _actions;

    private DetachedMenu? _identifiers;

    private OnlinePlayer? _selected;

    private bool _open;

    private string _query = string.Empty;

    private int _revisionAtBuild;

    private bool _outdated;

    private bool _hasSnapshot;

    private bool _busy;

    private bool _actionsOpen;

    private bool _readingNoClip;

    private bool _targetOwnsNoClip;

    private bool _targetLentNoClip;

    private bool _targetInNoClip;

    private TickHandle? _noClipPoll;

    // Opening the actions menu closes this one, and closing and reopening is what refreshes, so without
    // this backing out of a player would refetch the list and lose their place. Going back closes the
    // child and reopens the parent in one call, so the two land on the same frame, which is what tells
    // them apart from the player closing the menu and opening it again later.
    private int _leftForActions = -1;

    public override MenuText Subtitle => MenuText.From(BuildSubtitle);

    protected override void Build(MenuBuilder menu)
    {
        _menu = menu;

        menu.Menu.SetPageSize(PlayersPerPage);
        menu.Menu.WrapPages = true;
        menu.Menu.OnPageChange += OnPageChanged;

        menu.Keys.Add(new MenuKey
        {
            Name = "search",
            Description = MenuText.Key(Loc.OnlinePlayers.SearchBinding),
            DefaultKey = "SPACE",
            DefaultButton = "R1_INDEX",
            ShadowedControl = Control.Jump,
            Text = MenuText.Key(Loc.OnlinePlayers.SearchButton),
            Handler = (_, _) => _ = SearchAsync(),
        });

        _actions = menu.AddDetachedMenu(
            MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuText.From(() => MenuText
                .Key(Loc.OnlinePlayers.ActionsSubtitle, ("id", MenuText.Literal(SelectedId())))
                .Resolve(Localizer.Current)),
            BuildActions);

        _actions.Builder.OnOpenedAsync = _ =>
        {
            _actionsOpen = true;

            _noClipPoll?.Reevaluate();

            return ReadNoClipAsync();
        };

        _actions.Builder.OnClosed = _ =>
        {
            _actionsOpen = false;

            _noClipPoll?.Reevaluate();

            _leftForActions = Native.GetGameTimer();
        };

        _noClipPoll = TickRegistry.Register(
            "OnlinePlayers.NoClip",
            ReadNoClipAsync,
            TickRate.Every(NoClipPollMs),
            () => _actionsOpen);

        menu.OnOpened = _ => OnOpened();
        menu.OnClosed = _ => _open = false;

        // Not a setting, so the module has to be told about it before anything can listen for it.
        ClientConfig.Track([PlayerEvents.RevisionConvar]);
        ClientConfig.AddEventListenerFor([PlayerEvents.RevisionConvar], CheckStaleness);
    }

    private void OnOpened()
    {
        if (_menu is not { } menu)
        {
            return;
        }

        _open = true;

        // Rewritten here rather than declared once, so they follow a language change like everything else
        // does. MenuAPI's button hints hold a plain string, not a translation key.
        var localizer = Localizer.Current;

        menu.Menu.PreviousPageButtonText = localizer.Get(Loc.OnlinePlayers.PreviousPageButton);
        menu.Menu.NextPageButtonText = localizer.Get(Loc.OnlinePlayers.NextPageButton);

        if (_leftForActions == Native.GetGameTimer())
        {
            // Opening the actions menu closed this one, so anything the list missed while it was down never
            // reached the listener. This is the one path that keeps the old snapshot.
            CheckStaleness();

            UpdateSubtitle();

            return;
        }

        _ = RefreshAsync(_query);
    }

    private async Task RefreshAsync(string query)
    {
        if (_menu is not { } menu || _busy)
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.GetList, query);

            if (result.Status != ActionStatus.Ok)
            {
                Report(result, MenuText.Empty);

                return;
            }

            _query = query;

            _players.Clear();

            foreach (var row in result.Data)
            {
                if (PlayerRow.TryParse(row, out var serverId, out var name))
                {
                    _players.Add(new OnlinePlayer(serverId, name));
                }
            }

            // By name, because that is the order somebody reading a page expects. Ties broken by server id so
            // two players called the same thing do not swap places between refreshes.
            _players.Sort((left, right) =>
            {
                var byName = string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

                return byName != 0 ? byName : left.ServerId.CompareTo(right.ServerId);
            });

            _revisionAtBuild = Revision();
            _outdated = false;
            _hasSnapshot = true;

            RebuildRows(menu);

            UpdateSubtitle();
        }
        finally
        {
            _busy = false;
        }
    }

    private void RebuildRows(MenuBuilder menu)
    {
        menu.ClearEntries();

        if (_players.Count == 0)
        {
            // A row rather than an empty menu: MenuAPI ignores every direction key while a menu has no items,
            // which would leave the player unable to page or to search again.
            menu.AddRange([
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.OnlinePlayers.NoResults),
                    Description = MenuText.Key(Loc.OnlinePlayers.NoResultsDescription),
                },
            ]);

            return;
        }

        var rows = new List<MenuEntry>(_players.Count);

        foreach (var player in _players)
        {
            // Copied out of the loop variable so each row's handler captures its own player.
            var current = player;
            var id = MenuText.Literal(current.ServerId.ToString(CultureInfo.InvariantCulture));

            rows.Add(new ButtonEntry
            {
                // A player's name is data, not prose, so it is never looked up as a translation key.
                Text = MenuText.Literal(current.Name),
                Label = MenuText.Key(Loc.OnlinePlayers.ActionsSubtitle, ("id", id)),
                Description = MenuText.Key(Loc.OnlinePlayers.RowDescription, ("id", id)),
                OnSelected = _ => OpenActions(current),
            });
        }

        menu.AddRange(rows);
    }

    private void OpenActions(OnlinePlayer player)
    {
        if (_actions is not { } actions)
        {
            return;
        }

        _selected = player;

        _targetOwnsNoClip = false;
        _targetLentNoClip = false;
        _targetInNoClip = false;

        actions.Open();
    }

    private void BuildActions(MenuBuilder actions)
    {
        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.SendMessage),
            Description = MenuText.Key(Loc.OnlinePlayers.SendMessageDescription),
            Gate = OnlinePlayersPermissions.SendMessage,
            OnSelectedAsync = _ => SendMessageAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.TeleportTo),
            Description = MenuText.Key(Loc.OnlinePlayers.TeleportToDescription),
            Gate = OnlinePlayersPermissions.TeleportTo,
            OnSelectedAsync = _ => TeleportToAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.TeleportIntoVehicle),
            Description = MenuText.Key(Loc.OnlinePlayers.TeleportIntoVehicleDescription),
            Gate = OnlinePlayersPermissions.TeleportIntoVehicle,
            OnSelectedAsync = _ => TeleportIntoVehicleAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.Summon),
            Description = MenuText.Key(Loc.OnlinePlayers.SummonDescription),
            Gate = OnlinePlayersPermissions.Summon,
            OnSelectedAsync = _ => SummonAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicle),
            Description = MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleDescription),
            Gate = OnlinePlayersPermissions.SummonIntoVehicle,
            OnSelectedAsync = _ => SummonIntoVehicleAsync(),
        });

        actions.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.SetWantedLevel),
            Description = MenuText.Key(Loc.OnlinePlayers.SetWantedLevelDescription),
            Gate = OnlinePlayersPermissions.SetWantedLevel,
            Options = WantedLevels(),
            OnSelectedAsync = selected => SetWantedLevelAsync(selected.SelectedIndex),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.Kill),
            Description = MenuText.Key(Loc.OnlinePlayers.KillDescription),
            Gate = OnlinePlayersPermissions.Kill,
            OnSelectedAsync = _ => SendAsync(ActionIds.OnlinePlayers.Kill, Loc.OnlinePlayers.KillDone, allowSelf: true),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.Kick),
            Description = MenuText.Key(Loc.OnlinePlayers.KickDescription),
            Gate = OnlinePlayersPermissions.Kick,
            OnSelectedAsync = _ => KickAsync(),
        });

        actions.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.DeleteVehicle),
            Description = MenuText.Key(Loc.OnlinePlayers.DeleteVehicleDescription),
            ConfirmationDescription = MenuText.From(() => MenuText
                .Key(Loc.OnlinePlayers.DeleteVehicleConfirm, ("player", MenuText.Literal(_selected?.Name ?? string.Empty)))
                .Resolve(Localizer.Current)),
            Gate = OnlinePlayersPermissions.DeleteVehicle,
            OnConfirmedAsync = _ => DeleteVehicleAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.CheckStatus),
            Description = MenuText.Key(Loc.OnlinePlayers.CheckStatusDescription),
            Gate = OnlinePlayersPermissions.CheckStatus,
            OnSelectedAsync = _ => CheckStatusAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.Waypoint),
            Description = MenuText.Key(Loc.OnlinePlayers.WaypointDescription),
            Gate = OnlinePlayersPermissions.Waypoint,
            OnSelectedAsync = _ => SetWaypointAsync(),
        });

        actions.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.NoClipAccess),
            Description = MenuText.Key(Loc.OnlinePlayers.NoClipAccessDescription),
            Gate = OnlinePlayersPermissions.NoClip,
            ReadState = () => _targetOwnsNoClip || _targetLentNoClip,
            ReadEnabled = () => !_targetOwnsNoClip,
            OnChangedAsync = _ => ToggleNoClipAccessAsync(),
        });

        actions.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.NoClip),
            Description = MenuText.Key(Loc.OnlinePlayers.NoClipDescription),
            Gate = OnlinePlayersPermissions.NoClip,
            ReadState = () => _targetInNoClip,
            OnChangedAsync = _ => ToggleNoClipAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.RefreshPermissions),
            Description = MenuText.Key(Loc.OnlinePlayers.RefreshPermissionsDescription),
            Gate = OnlinePlayersPermissions.RefreshPermissions,
            OnSelectedAsync = _ => SendAsync(
                ActionIds.OnlinePlayers.RefreshPermissions,
                Loc.OnlinePlayers.RefreshPermissionsDone,
                allowSelf: true),
        });

        // Its own menu rather than a notification: there are usually five or six of these and every one is
        // far too long to read in a corner of the screen.
        _identifiers = actions.AddDetachedMenu(
            MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuText.Key(Loc.OnlinePlayers.IdentifiersSubtitle),
            _ => { },
            OnlinePlayersPermissions.Identifiers);

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.Identifiers),
            Description = MenuText.Key(Loc.OnlinePlayers.IdentifiersDescription),
            Label = MenuText.Literal("→"),
            Gate = OnlinePlayersPermissions.Identifiers,
            OnSelectedAsync = _ => ShowIdentifiersAsync(),
        });

        actions.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.OnlinePlayers.TxAdmin),
            Description = MenuText.Key(Loc.OnlinePlayers.TxAdminDescription),
            Gate = MenuGate.Permission(OnlinePlayersPermissions.TxAdmin) & MenuGate.When(TxAdminRunning),
            Behaviour = GateBehaviour.Hide,
            OnSelected = _ =>
            {
                if (_selected is { } player)
                {
                    Native.ExecuteCommand($"tx {Id(player)}");
                }
            },
        });

        actions.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.Plugins.PlayerActions),
            Description = MenuText.Key(Loc.Plugins.PlayerActionsDescription),
            Gate = MenuGate.When(PluginPlayerActions.AnyVisible),
            Behaviour = GateBehaviour.Hide,
            MenuTitle = MenuText.From(() => _selected?.Name ?? string.Empty),
            MenuSubtitle = MenuText.Key(Loc.Plugins.PlayerActions),
            Build = builder => PluginPlayerActions.Attach(
                builder,
                () => _selected is { } player ? (player.ServerId, player.Name) : ((int, string)?)null),
        });
    }

    private async Task DeleteVehicleAsync()
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.DeleteVehicle, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            Report(result, name);

            return;
        }

        switch (result.Data[0])
        {
            case DeletedVehicle:
                Notifications.Success(MenuText.Key(Loc.OnlinePlayers.DeleteVehicleDone, ("player", name)));

                break;

            case PassengerOnly:
                Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.DeleteVehicleNotDriving, ("player", name)));

                break;

            default:
                Notifications.Info(MenuText.Key(Loc.OnlinePlayers.DeleteVehicleOnFoot, ("player", name)));

                break;
        }
    }

    private Task ToggleNoClipAsync() =>
        SendNoClipToggleAsync(
            ActionIds.OnlinePlayers.SetNoClip,
            Loc.OnlinePlayers.NoClipOnDone,
            Loc.OnlinePlayers.NoClipOffDone,
            on => _targetInNoClip = on);

    private Task ToggleNoClipAccessAsync() =>
        SendNoClipToggleAsync(
            ActionIds.OnlinePlayers.SetNoClipAccess,
            Loc.OnlinePlayers.NoClipAccessOnDone,
            Loc.OnlinePlayers.NoClipAccessOffDone,
            on => _targetLentNoClip = on);

    private async Task SendNoClipToggleAsync(string action, string onKey, string offKey, Action<bool> apply)
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(action, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 2)
        {
            Report(result, name);

            _actions?.Refresh();

            return;
        }

        var on = result.Data[0] == StatusOn;

        apply(on);

        _actions?.Refresh();

        Notifications.Success(MenuText.Key(on ? onKey : offKey, ("player", MenuText.Literal(result.Data[1]))));
    }

    private async Task ReadNoClipAsync()
    {
        if (_readingNoClip
            || _selected is not { } player
            || !ClientPermissions.IsAllowed(OnlinePlayersPermissions.NoClip))
        {
            return;
        }

        _readingNoClip = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.GetNoClip, Id(player));

            if (result.Status != ActionStatus.Ok || result.Data.Length < 3)
            {
                return;
            }

            var owns = result.Data[0] == StatusOn;
            var lent = result.Data[1] == StatusOn;
            var active = result.Data[2] == StatusOn;

            if (owns == _targetOwnsNoClip && lent == _targetLentNoClip && active == _targetInNoClip)
            {
                return;
            }

            _targetOwnsNoClip = owns;
            _targetLentNoClip = lent;
            _targetInNoClip = active;

            _actions?.Refresh();
        }
        finally
        {
            _readingNoClip = false;
        }
    }

    private async Task CheckStatusAsync()
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.GetStatus, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 2)
        {
            Report(result, name);

            return;
        }

        Notifications.Info(MenuText.Key(
            Loc.OnlinePlayers.CheckStatusResult,
            ("player", name),
            ("player_god", StatusText(result.Data[0])),
            ("vehicle_god", StatusText(result.Data[1]))));
    }

    private static MenuText StatusText(string flag) => flag switch
    {
        StatusOn => MenuText.Key(Loc.OnlinePlayers.StatusOn),
        StatusOff => MenuText.Key(Loc.OnlinePlayers.StatusOff),
        _ => MenuText.Key(Loc.OnlinePlayers.StatusUnknown),
    };

    private static bool TxAdminRunning() =>
        string.Equals(Native.GetResourceState("txadmin"), "started", StringComparison.OrdinalIgnoreCase);

    private async Task ShowIdentifiersAsync()
    {
        if (_identifiers is not { } identifiers || _selected is not { } player)
        {
            return;
        }

        var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.GetIdentifiers, Id(player));

        if (result.Status != ActionStatus.Ok)
        {
            Report(result, MenuText.Literal(player.Name));

            return;
        }

        identifiers.Builder.ClearEntries();

        if (result.Data.Length == 0)
        {
            identifiers.Builder.AddRange([
                new ButtonEntry
                {
                    Text = MenuText.Key(Loc.OnlinePlayers.IdentifiersNone),
                    Description = MenuText.Key(Loc.OnlinePlayers.IdentifiersNoneDescription),
                },
            ]);

            identifiers.Open();

            return;
        }

        var rows = new List<MenuEntry>(result.Data.Length);

        foreach (var entry in result.Data)
        {
            var identifier = entry;
            var separator = identifier.IndexOf(':');

            rows.Add(new ButtonEntry
            {
                // The kind on the row, the whole thing in the description box, which is the only part of a menu wide
                // enough to show fifty characters.
                Text = MenuText.Literal(separator > 0 ? identifier[..separator] : identifier),
                Description = MenuText.Key(
                    Loc.OnlinePlayers.IdentifierDescription,
                    ("identifier", MenuText.Literal(identifier))),
                OnSelected = _ =>
                {
                    Log.Info($"[OnlinePlayers] {player.Name} (#{player.ServerId}): {identifier}");

                    Notifications.Info(MenuText.Key(Loc.OnlinePlayers.IdentifierPrinted));
                },
            });
        }

        identifiers.Builder.AddRange(rows);

        identifiers.Open();
    }

    private async Task SendMessageAsync()
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.OnlinePlayers.SendMessagePrompt, ("player", MenuText.Literal(player.Name))),
            maxLength: 200);

        if (string.IsNullOrWhiteSpace(typed))
        {
            return;
        }

        await SendAsync(ActionIds.OnlinePlayers.SendMessage, Loc.OnlinePlayers.SendMessageDelivered, allowSelf: true, typed.Trim());
    }

    private async Task KickAsync()
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.OnlinePlayers.KickPrompt, ("player", MenuText.Literal(player.Name))),
            maxLength: 200);

        // Cancelling cancels the kick. An empty box does not: the server has a default reason for it.
        if (typed is null)
        {
            return;
        }

        await SendAsync(ActionIds.OnlinePlayers.Kick, Loc.OnlinePlayers.KickDone, allowSelf: true, typed.Trim());
    }

    private async Task TeleportToAsync()
    {
        if (await CoordsAsync(ActionIds.OnlinePlayers.GetCoordsForTeleport) is not { } found)
        {
            return;
        }

        await PlayerTeleport.ToCoordsAsync(found.Coords);

        Notifications.Success(MenuText.Key(Loc.OnlinePlayers.TeleportToDone, ("player", MenuText.Literal(found.Player.Name))));
    }

    private async Task TeleportIntoVehicleAsync()
    {
        if (Target() is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.GetVehicleForTeleport, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 5)
        {
            Report(result, name);

            return;
        }

        if (!int.TryParse(result.Data[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId)
            || !TryParse(result.Data[2], out var x)
            || !TryParse(result.Data[3], out var y)
            || !TryParse(result.Data[4], out var z))
        {
            Report(ActionStatus.Failed, name);

            return;
        }

        var destination = new Vector3(x, y, z);

        if (result.Data[0] != "1")
        {
            await PlayerTeleport.ToCoordsAsync(destination);

            Notifications.Info(MenuText.Key(Loc.OnlinePlayers.TeleportIntoVehicleOnFoot, ("player", name)));

            return;
        }

        if (!await PlayerTeleport.IntoVehicleAsync(networkId, destination))
        {
            Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.TeleportIntoVehicleFull, ("player", name)));

            return;
        }

        Notifications.Success(MenuText.Key(Loc.OnlinePlayers.TeleportIntoVehicleDone, ("player", name)));
    }

    private async Task SummonAsync()
    {
        if (Target() is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(ActionIds.OnlinePlayers.Summon, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            Report(result, name);

            return;
        }

        switch (result.Data[0])
        {
            case SummonNearby:
                Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonNearby, ("player", name)));

                break;

            case SummonRiding:
                Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonRiding, ("player", name)));

                break;

            default:
                Notifications.Success(MenuText.Key(Loc.OnlinePlayers.SummonDone, ("player", name)));

                break;
        }
    }

    private async Task SummonIntoVehicleAsync()
    {
        if (Target() is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var vehicle = Native.GetVehiclePedIsIn(Native.PlayerPedId(), false);

        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleNoVehicle, ("player", name)));

            return;
        }

        if (FreeSeat(vehicle) is not { } seat)
        {
            Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleFull, ("player", name)));

            return;
        }

        var result = await ServerActions.InvokeAsync(
            ActionIds.OnlinePlayers.SummonIntoVehicle,
            Id(player),
            seat.ToString(CultureInfo.InvariantCulture));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            Report(result, name);

            return;
        }

        switch (result.Data[0])
        {
            case SummonMoved:
                Notifications.Success(MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleDone, ("player", name)));

                break;

            case SummonRiding:
                Notifications.Info(MenuText.Key(Loc.OnlinePlayers.SummonRiding, ("player", name)));

                break;

            case SummonNoVehicle:
                Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleNoVehicle, ("player", name)));

                break;

            default:
                Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.SummonIntoVehicleFull, ("player", name)));

                break;
        }
    }

    private static int? FreeSeat(int vehicle)
    {
        var seats = Native.GetVehicleModelNumberOfSeats((uint)Native.GetEntityModel(vehicle));

        for (var seat = 0; seat <= seats - 2; seat++)
        {
            if (Native.IsVehicleSeatFree(vehicle, seat, false))
            {
                return seat;
            }
        }

        return Native.IsVehicleSeatFree(vehicle, DriverSeat, false) ? DriverSeat : null;
    }

    private async Task SetWantedLevelAsync(int stars)
    {
        if (Target(allowSelf: true) is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        var result = await ServerActions.InvokeAsync(
            ActionIds.OnlinePlayers.SetWantedLevel,
            Id(player),
            stars.ToString(CultureInfo.InvariantCulture));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 1)
        {
            Report(result, name);

            return;
        }

        if (!int.TryParse(result.Data[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var reached))
        {
            Report(ActionStatus.Failed, name);

            return;
        }

        var wanted = MenuText.Literal(stars.ToString(CultureInfo.InvariantCulture));

        if (reached != stars)
        {
            Notifications.Warning(MenuText.Key(
                Loc.OnlinePlayers.SetWantedLevelBlocked,
                ("player", name),
                ("stars", wanted),
                ("actual", MenuText.Literal(reached.ToString(CultureInfo.InvariantCulture)))));

            return;
        }

        Notifications.Success(MenuText.Key(
            Loc.OnlinePlayers.SetWantedLevelDone,
            ("player", name),
            ("stars", wanted)));
    }

    private static MenuText[] WantedLevels()
    {
        var options = new MenuText[6];

        options[0] = MenuText.Key(Loc.OnlinePlayers.SetWantedLevelNone);

        for (var stars = 1; stars < options.Length; stars++)
        {
            options[stars] = MenuText.Literal(stars.ToString(CultureInfo.InvariantCulture));
        }

        return options;
    }

    private async Task SetWaypointAsync()
    {
        if (await CoordsAsync(ActionIds.OnlinePlayers.GetCoordsForWaypoint) is not { } found)
        {
            return;
        }

        Native.SetNewWaypoint(found.Coords.X, found.Coords.Y);

        Notifications.Success(MenuText.Key(Loc.OnlinePlayers.WaypointDone, ("player", MenuText.Literal(found.Player.Name))));
    }

    private async Task<(OnlinePlayer Player, Vector3 Coords)?> CoordsAsync(string action)
    {
        if (Target() is not { } player)
        {
            return null;
        }

        var result = await ServerActions.InvokeAsync(action, Id(player));

        if (result.Status != ActionStatus.Ok || result.Data.Length < 3)
        {
            Report(result, MenuText.Literal(player.Name));

            return null;
        }

        if (!TryParse(result.Data[0], out var x) || !TryParse(result.Data[1], out var y) || !TryParse(result.Data[2], out var z))
        {
            Report(ActionStatus.Failed, MenuText.Literal(player.Name));

            return null;
        }

        return (player, new Vector3(x, y, z));
    }

    private async Task SendAsync(string action, string successKey, bool allowSelf, params string[] extraArguments)
    {
        if (Target(allowSelf) is not { } player)
        {
            return;
        }

        var name = MenuText.Literal(player.Name);

        // Filled by hand rather than with a collection expression: spreading an array into one makes the
        // compiler reach for ReadOnlySpan, which the client's IL verifier refuses to load.
        var arguments = new string[extraArguments.Length + 1];

        arguments[0] = Id(player);

        Array.Copy(extraArguments, 0, arguments, 1, extraArguments.Length);

        var result = await ServerActions.InvokeAsync(action, arguments);

        if (result.Status != ActionStatus.Ok)
        {
            Report(result, name);

            return;
        }

        Notifications.Success(MenuText.Key(successKey, ("player", name)));
    }

    // Summon and teleporting into somebody's vehicle are the two the server still refuses on yourself, so
    // for those this only turns a generic refusal into a sentence that says what actually happened.
    private OnlinePlayer? Target(bool allowSelf = false)
    {
        if (_selected is not { } player)
        {
            return null;
        }

        if (!allowSelf && player.ServerId == Native.GetPlayerServerId(Native.PlayerId()))
        {
            Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.NotYourself));

            return null;
        }

        return player;
    }

    private async Task SearchAsync()
    {
        if (_busy)
        {
            return;
        }

        var typed = await UserInput.GetTextAsync(
            MenuText.Key(Loc.OnlinePlayers.SearchPrompt),
            SearchMaxLength,
            _query,
            NameSuggestions());

        if (typed is null)
        {
            return;
        }

        var query = typed.Trim();

        // A search is a fresh fetch either way, so it doubles as the refresh and clears the stale marker.
        await RefreshAsync(query);

        Notifications.Info(query.Length == 0
            ? MenuText.Key(Loc.OnlinePlayers.SearchCleared)
            : MenuText.Key(
                Loc.OnlinePlayers.SearchResults,
                ("count", MenuText.Literal(_players.Count.ToString(CultureInfo.InvariantCulture))),
                ("query", MenuText.Literal(query))));
    }

    // Names only. Identifiers never reach the client, which is the whole reason the search runs on the
    // server, so there is nothing to suggest for them.
    private IReadOnlyList<InputSuggestion> NameSuggestions() =>
        [.. _players.Select(player => new InputSuggestion
        {
            Value = player.Name,
            Label = player.Name,
            Detail = player.ServerId.ToString(CultureInfo.InvariantCulture),
        })];

    private void OnPageChanged(Menu menu, int oldPage, int newPage, bool wrapped)
    {
        UpdateSubtitle();

        if (!wrapped)
        {
            return;
        }

        Notifications.Info(MenuText.Key(
            Loc.OnlinePlayers.PageWrapped,
            ("page", MenuText.Literal((newPage + 1).ToString(CultureInfo.InvariantCulture))),
            ("pages", MenuText.Literal(menu.PageCount.ToString(CultureInfo.InvariantCulture)))));
    }

    private void CheckStaleness()
    {
        // There is nothing to be out of date with until a list has actually arrived, and nothing to say
        // about it while the list is not on screen: the notice asks the player to reopen the menu, which
        // reads as nonsense to somebody who does not have it open.
        if (!_open || !_hasSnapshot || _busy)
        {
            return;
        }

        if (_outdated || Revision() == _revisionAtBuild)
        {
            return;
        }

        _outdated = true;

        UpdateSubtitle();

        // The subtitle bar only has room to say that the list is stale, not what to do about it, so the
        // instruction goes where there is space for a sentence. Fires once, on the way stale.
        Notifications.Warning(MenuText.Key(Loc.OnlinePlayers.OutdatedNotice));
    }

    private static int Revision() => ClientConfig.GetInt(PlayerEvents.RevisionConvar) ?? 0;

    private void UpdateSubtitle()
    {
        if (_menu is { } menu)
        {
            menu.Menu.MenuSubtitle = BuildSubtitle();
        }
    }

    // Also what the framework resolves on a language or permission refresh, so both paths produce the
    // same line and neither can leave a stale one behind.
    private string BuildSubtitle()
    {
        if (_menu is not { } menu)
        {
            return string.Empty;
        }

        var localizer = Localizer.Current;

        var text = MenuText.Key(
            _query.Length == 0 ? Loc.OnlinePlayers.Subtitle : Loc.OnlinePlayers.SubtitleSearch,
            ("page", MenuText.Literal((menu.Menu.PageIndex + 1).ToString(CultureInfo.InvariantCulture))),
            ("pages", MenuText.Literal(menu.Menu.PageCount.ToString(CultureInfo.InvariantCulture))),
            ("count", MenuText.Literal(_players.Count.ToString(CultureInfo.InvariantCulture))),
            ("query", MenuText.Literal(Shorten(_query)))).Resolve(localizer);

        if (!_outdated)
        {
            return text;
        }

        // Only the marker is red, the page and the count keep reading normally. The blue is spelled out
        // because MenuAPI stops applying it once the string carries a colour code of its own, and
        // HUD_COLOUR_RED rather than ~r~ because MenuAPI draws the subtitle in capitals and an uppercased
        // ~r~ is a token the game does not know.
        return $"~HUD_COLOUR_FREEMODE~{text} ~HUD_COLOUR_RED~{localizer.Get(Loc.OnlinePlayers.SubtitleOutdated)}";
    }

    private static void Report(ActionResult result, MenuText player)
    {
        if (result.Status == ActionStatus.RateLimited && result.Data.Length > 0)
        {
            Notifications.Warning(MenuText.Key(
                Loc.OnlinePlayers.TooManyActions,
                ("seconds", MenuText.Literal(result.Data[0]))));

            return;
        }

        Report(result.Status, player);
    }

    private static void Report(ActionStatus status, MenuText player)
    {
        var key = status switch
        {
            ActionStatus.Denied => Loc.OnlinePlayers.Denied,
            ActionStatus.NotFound => Loc.OnlinePlayers.NotFound,
            ActionStatus.NotReady => Loc.OnlinePlayers.StillJoining,
            ActionStatus.Refused => Loc.OnlinePlayers.NotYourself,
            _ => Loc.OnlinePlayers.Failed,
        };

        Notifications.Error(MenuText.Key(key, ("player", player)));
    }

    // A licence is around fifty characters, several times what the bar can show next to the page count.
    private static string Shorten(string value) =>
        value.Length <= QueryDisplayLength ? value : value[..(QueryDisplayLength - 1)] + "…";

    private string SelectedId() =>
        (_selected?.ServerId ?? 0).ToString(CultureInfo.InvariantCulture);

    private static string Id(OnlinePlayer player) =>
        player.ServerId.ToString(CultureInfo.InvariantCulture);

    private static bool TryParse(string value, out float result) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class OnlinePlayer(int serverId, string name)
    {
        public int ServerId { get; } = serverId;

        public string Name { get; } = name;
    }
}

using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.MenuFramework;

public enum MenuPriority
{
    None,
    Low,
    Medium,
    High,
}

public enum MenuOpenOutcome
{
    Opened,
    BlockedByPriority,
    BlockedByLock,
    NotAllowed,
}

public static class MenuArbiter
{
    private const int HoldMs = 5000;

    private const int ConfirmTimeoutMs = 10000;

    // MenuAPI re-parents freely, so a chain that loops back on itself has to end somewhere.
    private const int MaxParentDepth = 32;

    private const int AcceptControl = 201;

    private const int CancelControl = 202;

    private const string HideCardMessage = """{"type":"menuLock","state":"hidden"}""";

    private static Session? _session;

    private static LockState? _lock;

    private static Menu? _lastSeen;

    private static TickHandle? _tick;

    private static MenuKeyBinding? _holdBinding;

    private static int _holdStarted = -1;

    private static int _confirmStarted = -1;

    private static bool _cardShown;

    private static bool? _toggleKeyBefore;

    public static Func<Menu, string?>? OwnerOf { get; set; }

    public static event Action<Menu>? Replacing;

    // Also raised when vMenu itself had to close a locked menu, so its owner always learns the lock is gone.
    public static event Action<Menu>? ForceClosed;

    public static MenuOpenOutcome TryOpen(Menu menu, string owner, MenuPriority priority, bool doNotClose)
    {
        var current = MenuController.GetCurrentMenu();
        var sameOwner = false;

        if (current is not null)
        {
            var currentOwner = OwnerOf?.Invoke(current);

            sameOwner = Same(currentOwner, owner);

            if (_lock is { } held && !Same(held.Owner, owner))
            {
                return MenuOpenOutcome.BlockedByLock;
            }

            if (!sameOwner && (priority == MenuPriority.None || priority < PriorityOf(currentOwner)))
            {
                return MenuOpenOutcome.BlockedByPriority;
            }
        }

        // Before anything closes, so a refusal leaves the player where they were.
        if (!MenuRegistry.CanOpen(menu))
        {
            return MenuOpenOutcome.NotAllowed;
        }

        var kept = sameOwner ? PriorityOf(owner) : MenuPriority.None;

        if (current is null)
        {
            // Its menu went away before Watch saw it, which still counts as the lock being broken.
            if (_lock is { } lost)
            {
                BreakLock(lost);
            }
        }
        else
        {
            if (!sameOwner)
            {
                Raise(Replacing, current);
            }

            if (_lock is { } own && !IsWithin(menu, own.Root))
            {
                ReleaseLock();
            }

            MenuController.CloseAllMenus();
        }

        _session = new Session(owner, priority > kept ? priority : kept);
        _lastSeen = menu;

        menu.OpenMenu();

        // An OnOpened handler can still close it again on the way in.
        if (!menu.Visible)
        {
            ReleaseLock();
            ApplyFlags();

            return MenuOpenOutcome.NotAllowed;
        }

        if (doNotClose)
        {
            TakeLock(menu, owner, forceClosable: true);
        }

        ApplyFlags();
        EnsureTick();

        return MenuOpenOutcome.Opened;
    }

    // The player cannot force a lock of vMenu's own shut, so the flow taking it has to offer its own way out.
    public static void Lock(Menu root) => TakeLock(root, null, forceClosable: false);

    public static void Unlock(Menu root)
    {
        if (_lock is { } held && ReferenceEquals(held.Root, root))
        {
            ReleaseLock();
        }
    }

    // Called before an owner closes its own menu, so that close does not count as the lock being broken.
    public static void ReleaseFor(string owner)
    {
        if (_lock is { } held && Same(held.Owner, owner))
        {
            ReleaseLock();
        }
    }

    private static MenuPriority PriorityOf(string? owner) =>
        _session is { } session && Same(session.Owner, owner) ? session.Priority : MenuPriority.None;

    private static void TakeLock(Menu root, string? owner, bool forceClosable)
    {
        if (_lock is { } existing && ReferenceEquals(existing.Root, root) && Same(existing.Owner, owner))
        {
            return;
        }

        ReleaseLock();

        Menu.KeyBindingHandler? button = null;

        if (forceClosable)
        {
            button = new Menu.KeyBindingHandler(
                HoldBinding(),
                MenuKeyPressType.PRESSED,
                null,
                Localizer.Current.Get(Loc.Framework.ForceCloseButton));

            root.KeyBindingHandlers.Add(button);
        }

        _lock = new LockState(root, owner, forceClosable, button);

        ApplyFlags();

        MenuController.DisableBackButton = ReferenceEquals(MenuController.GetCurrentMenu(), root);

        EnsureTick();
    }

    private static void ReleaseLock()
    {
        if (_lock is not { } held)
        {
            return;
        }

        _lock = null;

        if (held.Button is { } button)
        {
            MenuRegistry.RemoveByReference(held.Root.KeyBindingHandlers, button);
        }

        MenuController.DisableBackButton = false;

        ApplyFlags();
        ResetHold();
    }

    // Not PreventExitingMenu: that one also keeps the pause menu shut, and a locked menu should not trap the
    // player away from their game settings or the map.
    private static void ApplyFlags()
    {
        if (_lock is not null || _session is { Priority: MenuPriority.High })
        {
            // Turned back on by someone else meanwhile, which is what they will want once this lets go.
            if (_toggleKeyBefore is null || MenuController.EnableMenuToggleKey)
            {
                _toggleKeyBefore = MenuController.EnableMenuToggleKey;
            }

            MenuController.EnableMenuToggleKey = false;
        }
        else if (_toggleKeyBefore is { } before)
        {
            _toggleKeyBefore = null;

            MenuController.EnableMenuToggleKey = before;
        }
    }

    private static void BreakLock(LockState held)
    {
        ReleaseLock();

        if (held.Owner is null)
        {
            return;
        }

        Log.Warning($"[Menu] '{held.Root.MenuTitle}' from '{held.Owner}' asked not to be closed, but something else closed it.");

        Raise(ForceClosed, held.Root);
    }

    private static MenuKeyBinding HoldBinding() =>
        _holdBinding ??= new Menu(null).AddKeyBinding(
            "forceclose",
            Localizer.Current.Get(Loc.Framework.ForceCloseBinding),
            "BACK",
            "RRIGHT_INDEX").Binding;

    private static void EnsureTick()
    {
        _tick ??= TickRegistry.Register(
            "Menu.Arbiter",
            Watch,
            TickRate.PerFrame,
            static () => _session is not null || _lock is not null);

        _tick.Reevaluate();
    }

    private static void Watch()
    {
        var current = MenuController.GetCurrentMenu();

        if (current is null)
        {
            if (_lock is { } lost)
            {
                BreakLock(lost);
            }

            _session = null;
            _lastSeen = null;
        }
        else if (!ReferenceEquals(current, _lastSeen))
        {
            _lastSeen = current;

            if (_lock is { } held && !IsWithin(current, held.Root))
            {
                BreakLock(held);
            }

            if (_session is { } session && !Same(OwnerOf?.Invoke(current), session.Owner))
            {
                _session = null;
            }
        }

        ApplyFlags();

        if (current is null || _lock is not { } locked)
        {
            return;
        }

        var onRoot = ReferenceEquals(current, locked.Root);

        MenuController.DisableBackButton = onRoot;

        if (locked.ForceClosable && onRoot)
        {
            ProcessHold(locked);
        }
        else
        {
            ResetHold();
        }
    }

    private static void ProcessHold(LockState locked)
    {
        var now = Native.GetGameTimer();

        if (_confirmStarted >= 0)
        {
            Confirm(locked, now);
            return;
        }

        if (_holdBinding is not { Held: true })
        {
            if (_holdStarted >= 0)
            {
                _holdStarted = -1;
                HideCard();
            }

            return;
        }

        if (_holdStarted < 0)
        {
            _holdStarted = now;
            ShowCard("holding", HoldMs, Loc.Framework.ForceCloseTitle, Loc.Framework.ForceCloseHolding);

            return;
        }

        if (now - _holdStarted < HoldMs)
        {
            return;
        }

        _holdStarted = -1;
        _confirmStarted = now;

        // Select answers the question, and MenuAPI would otherwise also select the row under the cursor.
        MenuButtonLock.Take();

        ShowCard("confirm", ConfirmTimeoutMs, Loc.Framework.ForceCloseConfirmTitle, Loc.Framework.ForceCloseConfirm);
    }

    private static void Confirm(LockState locked, int now)
    {
        if (Pressed(AcceptControl))
        {
            ResetHold();
            ForceClose(locked);

            return;
        }

        if (Pressed(CancelControl) || now - _confirmStarted > ConfirmTimeoutMs)
        {
            ResetHold();
        }
    }

    private static void ForceClose(LockState locked)
    {
        Log.Warning($"[Menu] The player forced '{locked.Root.MenuTitle}' from '{locked.Owner}' shut, which asked not to be closed.");

        ReleaseLock();

        _session = null;

        ApplyFlags();

        MenuController.CloseAllMenus();

        Raise(ForceClosed, locked.Root);
    }

    private static void ResetHold()
    {
        _holdStarted = -1;

        HideCard();

        if (_confirmStarted < 0)
        {
            return;
        }

        _confirmStarted = -1;

        MenuButtonLock.Release();
    }

    private static void ShowCard(string state, int durationMs, string titleKey, string textKey)
    {
        var localizer = Localizer.Current;

        Native.SendNuiMessage(ClientJson.Serialize(new LockCardMessage
        {
            State = state,
            DurationMs = durationMs,
            Title = localizer.Get(titleKey),
            Text = localizer.Get(textKey),
            Accept = localizer.Get(Loc.Framework.ForceCloseAccept),
            AcceptKey = localizer.Get(Loc.Framework.ForceCloseSelectKey),
            Cancel = localizer.Get(Loc.Framework.ForceCloseCancel),
            CancelKey = localizer.Get(Loc.Framework.ForceCloseBackKey),
        }));

        _cardShown = true;
    }

    private static void HideCard()
    {
        if (!_cardShown)
        {
            return;
        }

        _cardShown = false;

        Native.SendNuiMessage(HideCardMessage);
    }

    private static bool Pressed(int control) =>
        Native.IsControlJustPressed(0, control) || Native.IsDisabledControlJustPressed(0, control);

    private static bool IsWithin(Menu menu, Menu root)
    {
        var current = menu;

        for (var depth = 0; current is not null && depth < MaxParentDepth; depth++)
        {
            if (ReferenceEquals(current, root))
            {
                return true;
            }

            current = current.ParentMenu;
        }

        return false;
    }

    private static bool Same(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static void Raise(Action<Menu>? handlers, Menu menu)
    {
        try
        {
            handlers?.Invoke(menu);
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] A menu arbiter handler threw: {exception}");
        }
    }

    private sealed class LockCardMessage
    {
        public string Type { get; } = "menuLock";

        public required string State { get; init; }

        public required int DurationMs { get; init; }

        public required string Title { get; init; }

        public required string Text { get; init; }

        public required string Accept { get; init; }

        public required string AcceptKey { get; init; }

        public required string Cancel { get; init; }

        public required string CancelKey { get; init; }
    }

    private sealed class Session(string? owner, MenuPriority priority)
    {
        internal string? Owner { get; } = owner;

        internal MenuPriority Priority { get; } = priority;
    }

    private sealed class LockState(Menu root, string? owner, bool forceClosable, Menu.KeyBindingHandler? button)
    {
        internal Menu Root { get; } = root;

        internal string? Owner { get; } = owner;

        internal bool ForceClosable { get; } = forceClosable;

        internal Menu.KeyBindingHandler? Button { get; } = button;
    }
}

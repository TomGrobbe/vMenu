using CitizenFX.Core.Native;

using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>Your plugin's client side entry point. Create it once, declare your menus, translations
/// and settings, then call <see cref="ConnectAsync"/>. It re-registers itself whenever vMenu
/// restarts, so everything you declared and changed since is restored.</summary>
public sealed class VMenuPlugin
{
    private static VMenuPlugin? _instance;

    private readonly Dictionary<string, PluginItem> _itemsById = new(StringComparer.Ordinal);

    private readonly Dictionary<string, PluginMenu> _menusById = new(StringComparer.Ordinal);

    private readonly Dictionary<string, PluginKey> _keysById = new(StringComparer.Ordinal);

    private readonly Dictionary<int, TaskCompletionSource<PromptResult>> _pendingPrompts = [];

    private readonly Dictionary<int, TaskCompletionSource<OpenMenuOutcome>> _pendingOpens = [];

    private readonly List<PluginMenu> _standaloneMenus = [];

    private readonly Text _displayName;

    private TaskCompletionSource<RegisterResult>? _firstResult;

    private List<UpdateOp>? _batch;

    private readonly HashSet<string> _pendingItems = new(StringComparer.Ordinal);

    private readonly HashSet<string> _pendingMenus = new(StringComparer.Ordinal);

    private readonly List<PluginMenu> _dirtyFilters = [];

    /// <summary>Open batch handles. A helper that batches internally must not flush its caller's.</summary>
    private int _batchDepth;

    private int _nextItemId;

    private int _nextMenuId;

    private int _nextPromptId;

    private int _nextOpenId;

    private int _vMenuProtocol;

    private bool _handlersRegistered;

    private VMenuPlugin(Text displayName)
    {
        _displayName = displayName;

        Resource = API.GetCurrentResourceName();
        Id = PluginId.Sanitize(Resource);
        Settings = new PluginSettings(Id);
        Themes = new PluginThemes(this);
        Translations = new PluginTranslations(this);
        PlayerActions = new PluginPlayerActions(this);
        RootMenu = new PluginMenu(this, new MenuNode { Id = "root", Title = displayName.ToRef() });

        RegisterMenu(RootMenu);
    }

    /// <summary>The resource this plugin runs in, its identity towards vMenu.</summary>
    public string Resource { get; }

    /// <summary>The sanitized identity used inside permission and convar names.</summary>
    public string Id { get; }

    /// <summary>Extra line under the resource name in your row's description, as a translation key.</summary>
    public string? DescriptionKey { get; set; }

    public PluginSettings Settings { get; }

    public PluginTranslations Translations { get; }

    public PluginThemes Themes { get; }

    /// <summary>The menu behind your row in vMenu's Plugins menu.</summary>
    public PluginMenu RootMenu { get; }

    /// <summary>Menus added with <see cref="AddStandaloneMenu"/>.</summary>
    public IReadOnlyList<PluginMenu> StandaloneMenus => _standaloneMenus;

    /// <summary>Actions injected into every player's entry of vMenu's Online Players menu.</summary>
    public PluginPlayerActions PlayerActions { get; }

    /// <summary>Whether vMenu currently has this plugin registered.</summary>
    public bool IsConnected { get; private set; }

    /// <summary>Raised on every registration answer, including automatic re-registrations.</summary>
    public event Action<RegisterResult>? RegistrationAnswered;

    /// <summary>Raised when vMenu stops, after which the plugin waits to re-register.</summary>
    public event Action? Disconnected;

    /// <summary>Creates the plugin. One per resource: a second call returns the first instance.</summary>
    public static VMenuPlugin Create(Text displayName)
    {
        if (_instance is { } existing)
        {
            PluginLog.Warn($"[{existing.Resource}] VMenuPlugin.Create was called twice, returning the first instance.");
            return existing;
        }

        _instance = new VMenuPlugin(displayName);

        return _instance;
    }

    /// <summary>Registers with vMenu. The task completes on vMenu's first answer, which can be a while
    /// when vMenu starts later than your resource. It never throws: a refusal arrives as a result with
    /// <c>Accepted</c> false.</summary>
    public Task<RegisterResult> ConnectAsync()
    {
        _firstResult ??= new TaskCompletionSource<RegisterResult>();

        EnsureHandlers();

        PluginEmit.Local(PluginEvents.Probe);

        return _firstResult.Task;
    }

    /// <summary>Adds a top level menu of your own, outside vMenu's Plugins menu. It has no row anywhere and no toggle
    /// key: only your code opens it, through <see cref="PluginMenu.RequestOpenAsync()"/>. Needs a vMenu that knows
    /// standalone menus, an older one refuses the registration. Adding one after connecting appears live.</summary>
    /// <param name="id">Letters, digits and underscores, unique within your plugin. "root" and "m" followed by
    /// digits are taken by the menus the API names itself.</param>
    /// <param name="title">The banner title.</param>
    /// <param name="subtitle">The line under the banner.</param>
    /// <exception cref="ArgumentException">The id is malformed, reserved or already used.</exception>
    /// <exception cref="InvalidOperationException">The plugin already has 50 standalone menus, or it is connected to a
    /// vMenu that predates standalone menus.</exception>
    public PluginMenu AddStandaloneMenu(string id, Text title, Text subtitle = default)
    {
        if (string.IsNullOrEmpty(id) || !id.All(static c => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_'))
        {
            throw new ArgumentException("A standalone menu id may only contain letters, digits and underscores.", nameof(id));
        }

        if (id == "root" || (id.Length > 1 && id[0] == 'm' && id.Skip(1).All(char.IsDigit)))
        {
            throw new ArgumentException($"'{id}' is reserved for the menus the API names itself.", nameof(id));
        }

        if (_menusById.ContainsKey(id))
        {
            throw new ArgumentException($"A menu with id '{id}' already exists.", nameof(id));
        }

        if (_standaloneMenus.Count >= PluginProtocol.MaxStandaloneMenus)
        {
            throw new InvalidOperationException($"A plugin may have at most {PluginProtocol.MaxStandaloneMenus} standalone menus.");
        }

        // Registering again to add it would get the whole plugin refused by a vMenu that does not know these menus.
        if (IsConnected && _vMenuProtocol < PluginProtocol.Version)
        {
            throw new InvalidOperationException("The vMenu this plugin is connected to predates standalone menus. Update vMenu.");
        }

        var menu = new PluginMenu(this, new MenuNode { Id = id, Title = title.ToRef(), Subtitle = subtitle.ToRef() });

        RegisterMenu(menu);
        _standaloneMenus.Add(menu);

        EmitOp(new UpdateOp { Op = UpdateOps.AddStandaloneMenu, Menu = menu.Node });

        return menu;
    }

    /// <summary>Shows a message through vMenu's notification area, credited to your resource.</summary>
    public void Notify(NotifyStyle style, Text text, int? durationMs = null)
    {
        if (!IsConnected)
        {
            return;
        }

        var request = new NotifyRequest
        {
            Style = style switch
            {
                NotifyStyle.Success => "success",
                NotifyStyle.Warning => "warning",
                NotifyStyle.Error => "error",
                _ => "info",
            },
            Text = text.ToRef(),
            DurationMs = durationMs,
        };

        PluginEmit.Local(PluginEvents.Notify, PluginJson.Serialize(request));
    }

    /// <summary>Asks the player for text through vMenu's input box. Null if they cancelled or the box
    /// was unavailable.</summary>
    public async Task<string?> GetTextAsync(
        Text title,
        int maxLength = 60,
        string initialValue = "",
        IReadOnlyList<PromptSuggestion>? suggestions = null)
    {
        var answers = await GetTextAsync(new PluginPrompt(title, maxLength, initialValue, suggestions));

        return answers is { Length: > 0 } ? answers[0] : null;
    }

    /// <summary>Asks several questions one after another. Null if the player cancelled any of them.</summary>
    public async Task<string[]?> GetTextAsync(params PluginPrompt[] prompts)
    {
        if (prompts.Length == 0 || !IsConnected)
        {
            return null;
        }

        var requestId = ++_nextPromptId;
        var pending = new TaskCompletionSource<PromptResult>();

        _pendingPrompts[requestId] = pending;

        var request = new PromptRequest { RequestId = requestId };

        foreach (var prompt in prompts)
        {
            var node = new PromptNode
            {
                Title = prompt.Title.ToRef(),
                MaxLength = prompt.MaxLength,
                Initial = prompt.InitialValue,
            };

            if (prompt.Suggestions is { Count: > 0 } suggestions)
            {
                node.Suggestions = [];

                foreach (var suggestion in suggestions)
                {
                    node.Suggestions.Add(new SuggestionNode
                    {
                        Value = suggestion.Value,
                        Description = suggestion.Description,
                    });
                }
            }

            request.Prompts.Add(node);
        }

        PluginEmit.Local(PluginEvents.Prompt, PluginJson.Serialize(request));

        var result = await pending.Task;

        return result.Cancelled || result.Answers is null ? null : [.. result.Answers];
    }

    /// <summary>Groups every change made until the returned handle is disposed into one update, so many
    /// small changes cost vMenu a single repaint. Nesting is fine: only the outermost handle sends.</summary>
    public IDisposable BeginBatch()
    {
        _batch ??= [];
        _batchDepth++;

        return new BatchScope(this);
    }

    internal Task<OpenMenuOutcome> RequestOpenAsync(PluginMenu menu, MenuPriority priority, bool doNotClose)
    {
        if (!IsConnected)
        {
            return Task.FromResult(OpenMenuOutcome.NotConnected);
        }

        // An older vMenu never answers, and its plain open would close whatever the player is in.
        if (_vMenuProtocol < PluginProtocol.Version)
        {
            return Task.FromResult(OpenMenuOutcome.NotSupported);
        }

        var requestId = ++_nextOpenId;
        var pending = new TaskCompletionSource<OpenMenuOutcome>();

        _pendingOpens[requestId] = pending;

        EmitOp(new UpdateOp
        {
            Op = UpdateOps.OpenMenu,
            MenuId = menu.Id,
            Priority = priority switch
            {
                MenuPriority.Low => MenuPriorities.Low,
                MenuPriority.Medium => MenuPriorities.Medium,
                MenuPriority.High => MenuPriorities.High,
                _ => MenuPriorities.None,
            },
            DoNotClose = doNotClose ? true : null,
            RequestId = requestId,
        });

        return pending.Task;
    }

    internal string NextItemId() => "i" + (++_nextItemId);

    internal string NextMenuId() => "m" + (++_nextMenuId);

    internal void RegisterMenu(PluginMenu menu) => _menusById[menu.Id] = menu;

    internal void RegisterItem(PluginItem item) => _itemsById[item.Node.Id] = item;

    internal void RegisterKey(PluginKey key)
    {
        if (!_keysById.TryAdd(key.Id, key))
        {
            PluginLog.Warn($"[{Resource}] Key id '{key.Id}' is used twice, vMenu will skip the second one.");
        }
    }

    internal void UnregisterItem(PluginItem item)
    {
        _itemsById.Remove(item.Node.Id);

        if (item is not PluginSubmenu submenu)
        {
            return;
        }

        _menusById.Remove(submenu.Menu.Id);

        foreach (var key in submenu.Menu.Keys)
        {
            _keysById.Remove(key.Id);
        }

        foreach (var child in submenu.Menu.Items)
        {
            UnregisterItem(child);
        }
    }

    internal void EmitOp(UpdateOp op)
    {
        if (!IsConnected)
        {
            return;
        }

        if (_batch is not { } batch)
        {
            Send([op]);
            return;
        }

        if (CarriedByPendingAdd(op))
        {
            return;
        }

        if (op.Op == UpdateOps.AddStandaloneMenu && op.Menu is { } standalone)
        {
            _pendingMenus.Add(standalone.Id);
        }

        if (op.Op == UpdateOps.AddItems)
        {
            foreach (var node in op.Items ?? [])
            {
                _pendingItems.Add(node.Id);

                if (node.Menu is { } menu)
                {
                    _pendingMenus.Add(menu.Id);
                }
            }
        }

        batch.Add(op);
    }

    internal void FilterChanged(PluginMenu menu)
    {
        if (!IsConnected)
        {
            return;
        }

        if (_batch is null)
        {
            Send([menu.FilterOp()]);
            return;
        }

        MarkFilterDirty(menu);
    }

    internal void EmitAdd(PluginMenu menu, PluginItem item, UpdateOp add)
    {
        if (!IsConnected)
        {
            return;
        }

        if (_batch is not null)
        {
            EmitOp(add);

            if (menu.HasFilter)
            {
                MarkFilterDirty(menu);
            }

            return;
        }

        var ops = new List<UpdateOp> { add };

        if (menu.Hides(item))
        {
            ops.Add(new UpdateOp { Op = UpdateOps.SetFilter, MenuId = menu.Id, ItemIds = [item.Id], Flag = true });
        }

        Send(ops);
    }

    private void MarkFilterDirty(PluginMenu menu)
    {
        if (!_dirtyFilters.Exists(dirty => ReferenceEquals(dirty, menu)))
        {
            _dirtyFilters.Add(menu);
        }
    }

    private bool CarriedByPendingAdd(UpdateOp op) => op.Op switch
    {
        UpdateOps.SetText
            or UpdateOps.SetDescription
            or UpdateOps.SetLabel
            or UpdateOps.SetLockedDescription
            or UpdateOps.SetConfirmationDescription
            or UpdateOps.SetIcons
            or UpdateOps.SetChecked
            or UpdateOps.SetOptions
            or UpdateOps.SetSelectedIndex
            or UpdateOps.SetSliderPosition
            or UpdateOps.SetValue
            or UpdateOps.SetVisible
            or UpdateOps.SetEnabled
            or UpdateOps.SetGate
            or UpdateOps.SetLog
            or UpdateOps.SetBehaviour
            or UpdateOps.SetItemEvents => op.ItemId is { } itemId && _pendingItems.Contains(itemId),
        UpdateOps.SetMenuTitle
            or UpdateOps.SetMenuSubtitle
            or UpdateOps.SetMenuEvents
            or UpdateOps.AddKeys => op.MenuId is { } menuId && _pendingMenus.Contains(menuId),
        _ => false,
    };

    private void Send(List<UpdateOp> ops)
    {
        foreach (var op in ops)
        {
            if (op.Op == UpdateOps.AddItems && op.Items is { } items)
            {
                op.Items = items.ConvertAll(PluginMenu.AsAdded);
            }

            // Rows added after this op went into the batch travel as their own addItems ops.
            if (op.Op == UpdateOps.AddStandaloneMenu && op.Menu is { } menu)
            {
                op.Menu = PluginMenu.WithoutItems(menu);
            }
        }

        PluginEmit.Local(PluginEvents.Update, PluginJson.Serialize(new UpdateBatch { Ops = ops }));
    }

    internal void MergeTranslations(string code, Dictionary<string, string> entries)
    {
        if (IsConnected)
        {
            EmitOp(new UpdateOp { Op = UpdateOps.MergeTranslations, Language = code, Entries = entries });
        }
    }

    private void EnsureHandlers()
    {
        if (_handlersRegistered)
        {
            return;
        }

        _handlersRegistered = true;

        PluginScript.On(PluginEvents.Ready, OnReady);
        PluginScript.On(PluginEvents.ReadyFor(Resource), OnReady);
        PluginScript.On(PluginEvents.RegisterResultFor(Resource), OnRegisterResult);
        PluginScript.On(PluginEvents.EventFor(Resource), OnCallback);
        PluginScript.On(PluginEvents.PromptResultFor(Resource), OnPromptResult);
        PluginScript.On(PluginEvents.OpenResultFor(Resource), OnOpenResult);
        PluginScript.On(PluginEvents.ThemesFor(Resource), Themes.Handle);
        PluginScript.On("onResourceStop", OnResourceStop);
    }

    private void OnReady(int protocolVersion)
    {
        _vMenuProtocol = protocolVersion;

        SendRegistration();
    }

    private void SendRegistration() =>
        PluginEmit.Local(PluginEvents.Register, PluginJson.Serialize(BuildRequest()));

    private RegisterRequest BuildRequest()
    {
        var request = new RegisterRequest
        {
            // Only a plugin that needs a newer vMenu says so, so everything else still registers with an older one.
            ProtocolVersion = _standaloneMenus.Count > 0 ? PluginProtocol.Version : PluginProtocol.BaseVersion,
            DisplayName = _displayName.ToRef(),
            DescriptionKey = DescriptionKey,
            // Always sent, even while empty. vMenu leaves the plugin's row out until the menu has something in
            // it, and it can only accept rows added later for a menu it knows about.
            Menu = RootMenu.Node,
            PlayerActions = PlayerActions.Nodes.Count > 0 ? PlayerActions.Nodes : null,
        };

        if (_standaloneMenus.Count > 0)
        {
            request.StandaloneMenus = _standaloneMenus.ConvertAll(static menu => menu.Node);
        }

        if (Translations.Tables.Count > 0)
        {
            request.Translations = Translations.Tables;
        }

        if (Settings.Nodes.Count > 0)
        {
            request.Settings = [.. Settings.Nodes];
        }

        return request;
    }

    private void OnRegisterResult(string json)
    {
        if (!PluginJson.TryDeserialize<RegisterResult>(json, out var result) || result is null)
        {
            PluginLog.Warn($"[{Resource}] vMenu sent a registration answer that did not parse.");
            return;
        }

        foreach (var error in result.Errors)
        {
            PluginLog.Error($"[{Resource}] vMenu refused the plugin registration: {error}");
        }

        foreach (var warning in result.Warnings)
        {
            PluginLog.Warn($"[{Resource}] vMenu accepted the plugin registration with a note: {warning}");
        }

        IsConnected = result.Accepted;

        // A refused registration means vMenu no longer knows this plugin, so no answer is coming for
        // anything already asked and whoever is awaiting one would wait for good.
        if (!IsConnected)
        {
            CancelPendingPrompts();
            CancelPendingOpens();
        }
        else
        {
            using (BeginBatch())
            {
                foreach (var menu in _menusById.Values)
                {
                    if (menu.HasFilter)
                    {
                        FilterChanged(menu);
                    }
                }
            }
        }

        _firstResult?.TrySetResult(result);

        try
        {
            RegistrationAnswered?.Invoke(result);
        }
        catch (Exception exception)
        {
            PluginLog.Error($"[{Resource}] A RegistrationAnswered handler threw: {exception}");
        }
    }

    private void OnCallback(string json)
    {
        if (!PluginJson.TryDeserialize<PluginCallback>(json, out var callback) || callback is null)
        {
            return;
        }

        try
        {
            switch (callback.Type)
            {
                case CallbackTypes.MenuOpened:
                case CallbackTypes.MenuClosed:
                case CallbackTypes.MenuIndexChanged:
                case CallbackTypes.MenuReplaced:
                case CallbackTypes.MenuForceClosed:
                    if (callback.MenuId is { } menuId && _menusById.TryGetValue(menuId, out var menu))
                    {
                        menu.HandleMenu(callback);
                    }

                    break;

                case CallbackTypes.KeyPressed:
                    if (callback.KeyId is { } keyId && _keysById.TryGetValue(keyId, out var key))
                    {
                        key.Handle(new PluginKeyPress(ItemOrNull(callback.ItemId), ItemOrNull(callback.DisabledItemId)));
                    }

                    break;

                default:
                    if (callback.ItemId is { } itemId && _itemsById.TryGetValue(itemId, out var item))
                    {
                        item.Handle(callback);
                    }

                    break;
            }
        }
        catch (Exception exception)
        {
            PluginLog.Error($"[{Resource}] A menu callback handler threw: {exception}");
        }
    }

    private PluginItem? ItemOrNull(string? id) => id is null ? null : _itemsById.GetValueOrDefault(id);

    private void OnPromptResult(string json)
    {
        if (!PluginJson.TryDeserialize<PromptResult>(json, out var result) || result is null)
        {
            return;
        }

        if (_pendingPrompts.Remove(result.RequestId, out var pending))
        {
            pending.TrySetResult(result);
        }
    }

    private void OnOpenResult(string json)
    {
        if (!PluginJson.TryDeserialize<OpenMenuResult>(json, out var result) || result is null)
        {
            return;
        }

        if (!_pendingOpens.Remove(result.RequestId, out var pending))
        {
            return;
        }

        pending.TrySetResult(result.Reason switch
        {
            OpenMenuReasons.Opened => OpenMenuOutcome.Opened,
            OpenMenuReasons.BlockedByLock => OpenMenuOutcome.BlockedByLock,
            OpenMenuReasons.UnknownMenu => OpenMenuOutcome.UnknownMenu,
            OpenMenuReasons.NotAllowed => OpenMenuOutcome.NotAllowed,
            OpenMenuReasons.Superseded => OpenMenuOutcome.Superseded,
            _ => OpenMenuOutcome.BlockedByPriority,
        });
    }

    private void CancelPendingOpens()
    {
        foreach (var pending in _pendingOpens.Values)
        {
            pending.TrySetResult(OpenMenuOutcome.NotConnected);
        }

        _pendingOpens.Clear();
    }

    private void CancelPendingPrompts()
    {
        foreach (var pending in _pendingPrompts.Values)
        {
            pending.TrySetResult(new PromptResult { Cancelled = true });
        }

        _pendingPrompts.Clear();
    }

    private void OnResourceStop(string stopped)
    {
        if (!string.Equals(stopped, PluginProtocol.VMenuResource, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        IsConnected = false;

        CancelPendingPrompts();
        CancelPendingOpens();

        try
        {
            Disconnected?.Invoke();
        }
        catch (Exception exception)
        {
            PluginLog.Error($"[{Resource}] A Disconnected handler threw: {exception}");
        }
    }

    private sealed class BatchScope : IDisposable
    {
        private readonly VMenuPlugin _plugin;

        private bool _disposed;

        internal BatchScope(VMenuPlugin plugin) => _plugin = plugin;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (--_plugin._batchDepth > 0)
            {
                return;
            }

            if (_plugin._batch is not { } ops)
            {
                return;
            }

            _plugin._batch = null;
            _plugin._pendingItems.Clear();
            _plugin._pendingMenus.Clear();

            foreach (var menu in _plugin._dirtyFilters)
            {
                ops.Add(menu.FilterOp());
            }

            _plugin._dirtyFilters.Clear();

            if (ops.Count == 0 || !_plugin.IsConnected)
            {
                return;
            }

            _plugin.Send(ops);
        }
    }
}

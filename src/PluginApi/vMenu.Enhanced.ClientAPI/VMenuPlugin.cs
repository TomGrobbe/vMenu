using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

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

    private readonly Dictionary<int, TaskCompletionSource<PromptResult>> _pendingPrompts = new();

    private readonly Text _displayName;

    private TaskCompletionSource<RegisterResult>? _firstResult;

    private List<UpdateOp>? _batch;

    /// <summary>Open batch handles. A helper that batches internally must not flush its caller's.</summary>
    private int _batchDepth;

    private int _nextItemId;

    private int _nextMenuId;

    private int _nextPromptId;

    private bool _handlersRegistered;

    private VMenuPlugin(Text displayName)
    {
        _displayName = displayName;

        Resource = Native.GetCurrentResourceName();
        Id = PluginId.Sanitize(Resource);
        Settings = new PluginSettings(Id);
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

    /// <summary>The menu behind your row in vMenu's Plugins menu.</summary>
    public PluginMenu RootMenu { get; }

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
            SharedAPI.Log.Warn($"[{existing.Resource}] VMenuPlugin.Create was called twice, returning the first instance.");
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
        SendRegistration();

        PluginEmit.Local(PluginEvents.Probe);

        return _firstResult.Task;
    }

    /// <summary>Shows a message through vMenu's notification area, credited to your resource.</summary>
    public void Notify(NotifyStyle style, Text text, int? durationMs = null)
    {
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
                node.Suggestions = new List<SuggestionNode>();

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

        return result.Cancelled || result.Answers is null ? null : result.Answers.ToArray();
    }

    /// <summary>Groups every change made until the returned handle is disposed into one update, so many
    /// small changes cost vMenu a single repaint. Nesting is fine: only the outermost handle sends.</summary>
    public IDisposable BeginBatch()
    {
        _batch ??= new List<UpdateOp>();
        _batchDepth++;

        return new BatchScope(this);
    }

    internal string NextItemId() => "i" + (++_nextItemId);

    internal string NextMenuId() => "m" + (++_nextMenuId);

    internal void RegisterMenu(PluginMenu menu) => _menusById[menu.Id] = menu;

    internal void RegisterItem(PluginItem item) => _itemsById[item.Node.Id] = item;

    internal void UnregisterItem(PluginItem item)
    {
        _itemsById.Remove(item.Node.Id);

        if (item is not PluginSubmenu submenu)
        {
            return;
        }

        _menusById.Remove(submenu.Menu.Id);

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

        if (_batch is { } batch)
        {
            batch.Add(op);
            return;
        }

        var single = new UpdateBatch();
        single.Ops.Add(op);

        PluginEmit.Local(PluginEvents.Update, PluginJson.Serialize(single));
    }

    internal void MergeTranslations(string code, Dictionary<string, string> entries)
    {
        if (IsConnected)
        {
            EmitOp(new UpdateOp { Op = UpdateOps.MergeTranslations, Language = code, Entries = entries });
        }
    }

    /// <summary>Re-sends the whole registration, replacing the tree. For changes ops cannot express.</summary>
    internal void ReRegisterIfConnected()
    {
        if (IsConnected)
        {
            SendRegistration();
        }
    }

    private void EnsureHandlers()
    {
        if (_handlersRegistered)
        {
            return;
        }

        _handlersRegistered = true;

        API.OnEvent(PluginEvents.Ready, new Action<int>(OnReady), false);
        API.OnEvent(PluginEvents.ReadyFor(Resource), new Action<int>(OnReady), false);
        API.OnEvent(PluginEvents.RegisterResultFor(Resource), new Action<string>(OnRegisterResult), false);
        API.OnEvent(PluginEvents.EventFor(Resource), new Action<string>(OnCallback), false);
        API.OnEvent(PluginEvents.PromptResultFor(Resource), new Action<string>(OnPromptResult), false);
        API.OnEvent("onResourceStop", new Action<string>(OnResourceStop), false);
    }

    private void OnReady(int protocolVersion) => SendRegistration();

    private void SendRegistration() =>
        PluginEmit.Local(PluginEvents.Register, PluginJson.Serialize(BuildRequest()));

    private RegisterRequest BuildRequest()
    {
        var request = new RegisterRequest
        {
            ProtocolVersion = PluginProtocol.Version,
            DisplayName = _displayName.ToRef(),
            DescriptionKey = DescriptionKey,
            // Always sent, even while empty. vMenu leaves the plugin's row out until the menu has something in
            // it, and it can only accept rows added later for a menu it knows about.
            Menu = RootMenu.Node,
            PlayerActions = PlayerActions.Nodes.Count > 0 ? PlayerActions.Nodes : null,
        };

        if (Translations.Tables.Count > 0)
        {
            request.Translations = Translations.Tables;
        }

        if (Settings.Nodes.Count > 0)
        {
            request.Settings = new List<SettingNode>(Settings.Nodes);
        }

        return request;
    }

    private void OnRegisterResult(string json)
    {
        if (!PluginJson.TryDeserialize<RegisterResult>(json, out var result) || result is null)
        {
            SharedAPI.Log.Warn($"[{Resource}] vMenu sent a registration answer that did not parse.");
            return;
        }

        foreach (var error in result.Errors)
        {
            SharedAPI.Log.Error($"[{Resource}] vMenu refused the plugin registration: {error}");
        }

        foreach (var warning in result.Warnings)
        {
            SharedAPI.Log.Warn($"[{Resource}] vMenu accepted the plugin registration with a note: {warning}");
        }

        IsConnected = result.Accepted;

        // A refused registration means vMenu no longer knows this plugin, so no answer is coming for
        // anything already asked and whoever is awaiting one would wait for good.
        if (!IsConnected)
        {
            CancelPendingPrompts();
        }

        _firstResult?.TrySetResult(result);

        try
        {
            RegistrationAnswered?.Invoke(result);
        }
        catch (Exception exception)
        {
            SharedAPI.Log.Error($"[{Resource}] A RegistrationAnswered handler threw: {exception}");
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
                    if (callback.MenuId is { } menuId && _menusById.TryGetValue(menuId, out var menu))
                    {
                        menu.HandleMenu(callback);
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
            SharedAPI.Log.Error($"[{Resource}] A menu callback handler threw: {exception}");
        }
    }

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

        try
        {
            Disconnected?.Invoke();
        }
        catch (Exception exception)
        {
            SharedAPI.Log.Error($"[{Resource}] A Disconnected handler threw: {exception}");
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

            if (ops.Count == 0 || !_plugin.IsConnected)
            {
                return;
            }

            var batch = new UpdateBatch { Ops = ops };

            PluginEmit.Local(PluginEvents.Update, PluginJson.Serialize(batch));
        }
    }
}

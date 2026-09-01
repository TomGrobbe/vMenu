using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.BrokenNatives;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.PluginContracts;
using vMenu.Enhanced.Serialization;

using ConfigPath = vMenu.Enhanced.Data.Configuration.ConfigPath;
using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Plugins;

// Handlers are registered imperatively because attribute discovery only scans the assembly named as
// the client_script, and this one is a project reference.
public static class PluginHost
{
    private const string StopEvent = "onResourceStop";

    private const int MinNotifyDurationMs = 5000;

    private const int MaxNotifyDurationMs = 30000;

    private const int MaxPromptLength = 500;

    private const int MaxThemesPerResource = 25;

    private static readonly Dictionary<string, PluginState> Plugins = new(StringComparer.OrdinalIgnoreCase);

    // Registrations that arrived before the menu tree was built.
    private static readonly Dictionary<string, string> Pending = new(StringComparer.OrdinalIgnoreCase);

    // Last accepted payload per resource, so a repeated registration costs nothing.
    private static readonly Dictionary<string, string> LastPayload = new(StringComparer.OrdinalIgnoreCase);

    // Convars already wired to a refresh listener, so a re-register cannot double them.
    private static readonly HashSet<string> ListenedConvars = new(StringComparer.OrdinalIgnoreCase);

    private static MenuBuilder? _pluginsBuilder;

    private static bool _handlersRegistered;

    private static bool _ready;

    private static bool _promptBusy;

    public static int Count => Plugins.Count;

    internal static IReadOnlyCollection<PluginState> All => Plugins.Values;

    internal static event Action? PluginsChanged;

    public static void RegisterEventHandlers()
    {
        if (_handlersRegistered)
        {
            return;
        }

        _handlersRegistered = true;

        API.OnEvent(PluginEvents.Probe, new Action(OnProbe), false);
        API.OnEvent(PluginEvents.Register, new Action<string>(OnRegister), false);
        API.OnEvent(PluginEvents.Unregister, new Action(OnUnregister), false);
        API.OnEvent(PluginEvents.Update, new Action<string>(OnUpdate), false);
        API.OnEvent(PluginEvents.Notify, new Action<string>(OnNotify), false);
        API.OnEvent(PluginEvents.Prompt, new Action<string>(OnPrompt), false);
        API.OnEvent(PluginEvents.SetTheme, new Action<string>(OnSetTheme), false);
        API.OnEvent(PluginEvents.RegisterThemes, new Action<string>(OnRegisterThemes), false);
        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);

        MenuSkin.Changed += BroadcastThemes;
    }

    public static void AnnounceReady()
    {
        _ready = true;

        foreach (var pending in Pending.ToList())
        {
            ApplyRegistration(pending.Key, pending.Value);
        }

        Pending.Clear();

        NativeFixer.EmitLocal(PluginEvents.Ready, PluginProtocol.Version);
    }

    internal static void AttachPluginsMenu(MenuBuilder builder) => _pluginsBuilder = builder;

    internal static void Emit(PluginState state, PluginCallback callback) =>
        NativeFixer.EmitLocal(state.EventName, ClientJson.Serialize(callback));

    private static void OnProbe()
    {
        if (_ready && Sender() is { } resource)
        {
            NativeFixer.EmitLocal(PluginEvents.ReadyFor(resource), PluginProtocol.Version);
        }
    }

    private static void OnRegister(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        if (!_ready)
        {
            Pending[resource] = json;
            return;
        }

        ApplyRegistration(resource, json);
    }

    private static void OnUnregister()
    {
        if (Sender() is { } resource)
        {
            Teardown(resource);
        }
    }

    private static void OnResourceStop(string stopped)
    {
        var themes = MenuSkin.RemoveCustomFrom(stopped);

        if (themes > 0)
        {
            Log.Info($"[Plugins] '{stopped}' stopped, dropping {themes} theme(s) it provided.");
        }

        if (Plugins.ContainsKey(stopped) || Pending.ContainsKey(stopped))
        {
            Teardown(stopped);
        }
    }

    private static void Teardown(string resource)
    {
        Pending.Remove(resource);
        LastPayload.Remove(resource);

        if (!Plugins.ContainsKey(resource))
        {
            return;
        }

        // Before the plugin is dropped, since the check walks the registered set and a plugin already out of
        // it can no longer be recognised as the owner of the menu on screen.
        CloseIfInsideAPluginMenu();

        Plugins.Remove(resource);

        Log.Info($"[Plugins] '{resource}' unregistered, removing its menus.");

        RebuildRows();

        RaiseChanged();
    }

    private static void ApplyRegistration(string resource, string json)
    {
        try
        {
            Register(resource, json);
        }
        catch (Exception exception)
        {
            Log.Error($"[Plugins] Registration from '{resource}' failed: {exception}");
            Reply(resource, Refused($"vMenu hit an internal error: {exception.Message}"));
        }
    }

    private static void Register(string resource, string json)
    {
        if (LastPayload.TryGetValue(resource, out var previous) && previous == json && Plugins.ContainsKey(resource))
        {
            Reply(resource, new RegisterResult { Accepted = true });
            SendThemes(resource);
            return;
        }

        if (!ClientJson.TryDeserialize<RegisterRequest>(json, out var request) || request is null)
        {
            Reply(resource, Refused("The registration payload did not parse."));
            return;
        }

        if (request.ProtocolVersion > PluginProtocol.Version)
        {
            Reply(resource, Refused(
                $"The plugin speaks protocol {request.ProtocolVersion} but this vMenu only knows "
                + $"{PluginProtocol.Version}. Update vMenu or use an older plugin API package."));
            return;
        }

        var id = PluginId.Sanitize(resource);

        if (id.Length == 0)
        {
            Reply(resource, Refused($"The resource name '{resource}' cannot be turned into a usable identity."));
            return;
        }

        foreach (var other in Plugins.Values)
        {
            if (other.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
                && !other.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
            {
                Reply(resource, Refused(
                    $"The identity '{id}' is already taken by resource '{other.Resource}'. Rename one of the two resources."));
                return;
            }
        }

        var result = new RegisterResult { Accepted = true };
        var state = new PluginState(resource, id);

        if (!CopyTranslations(state, request, result))
        {
            Reply(resource, result);
            return;
        }

        state.DisplayName = request.DisplayName;
        state.DescriptionRef = request.DescriptionKey is { Length: > 0 } key ? TextRef.ForKey(key) : null;

        if (request.Menu is { } menu && !PluginValidation.IndexMenuTree(state, menu, result))
        {
            result.Accepted = false;
            Reply(resource, result);
            return;
        }

        // After validation, never before it: tracking a convar adds a listener that lives as long as the
        // client does, and a refused registration would leave those behind for a plugin that never made it in.
        TrackSettings(state, request.Settings, result);

        state.RootMenu = request.Menu;

        if (request.PlayerActions is { Count: > 0 } actions)
        {
            foreach (var action in actions)
            {
                if (PluginValidation.IndexPlayerAction(state, action, result))
                {
                    state.PlayerActions.Add(action);
                }
            }
        }

        // Before the dictionary is written, since the check reads it and the state going in is a fresh one
        // that knows nothing of the menus the player may be standing in right now.
        CloseIfInsideAPluginMenu();

        var firstRegistration = !Plugins.ContainsKey(resource);

        Plugins[resource] = state;
        LastPayload[resource] = json;

        RebuildRows();

        RaiseChanged();

        if (firstRegistration)
        {
            Log.Info($"[Plugins] Registered '{resource}'.");
        }
        else
        {
            Log.Debug($"[Plugins] '{resource}' re-registered with {state.ItemsById.Count} item(s).");
        }

        Reply(resource, result);

        SendThemes(resource);
    }

    private static bool CopyTranslations(PluginState state, RegisterRequest request, RegisterResult result)
    {
        if (request.Translations is not { Count: > 0 } tables)
        {
            return true;
        }

        foreach (var pair in tables)
        {
            // Re-copied with an ordinal comparer: the deserializer builds tables with the default one, whose
            // internals the sandbox does not always permit.
            state.Translations[pair.Key.Trim().ToLowerInvariant()] =
                new Dictionary<string, string>(pair.Value, StringComparer.Ordinal);
        }

        // Asked of the copies, not of the payload: a hand written 'EN' is a perfectly good English table once
        // the loop above has normalised its code.
        if (state.Translations.ContainsKey("en"))
        {
            return true;
        }

        result.Accepted = false;
        result.Errors.Add("Translations were provided without an 'en' table. English is the required fallback.");

        return false;
    }

    private static void TrackSettings(PluginState state, List<SettingNode>? nodes, RegisterResult result)
    {
        if (nodes is null || nodes.Count == 0)
        {
            return;
        }

        var convars = new List<string>();

        foreach (var node in nodes)
        {
            if (!ConfigPath.IsValidSegment(node.Name))
            {
                result.Warnings.Add(
                    $"Setting '{node.Name}' was skipped: names may only contain letters, digits and underscores.");
                continue;
            }

            var fullName = PluginPermissions.Prefix + ConfigPath.Separator + state.Id + ConfigPath.Separator + node.Name;

            convars.Add(fullName);

            if (node.Type == SettingTypes.Bool)
            {
                state.BoolSettings[node.Name] = new BoolSetting(fullName)
                {
                    Description = node.Description,
                    Default = string.Equals(node.Default, "true", StringComparison.OrdinalIgnoreCase),
                };
            }
        }

        if (convars.Count == 0)
        {
            return;
        }

        ClientConfig.Track(convars);

        // Tracked convars are quiet, so the framework's catch-all listener never fires for them. Each one
        // needs its own refresh listener, added once even across re-registrations.
        var fresh = convars.Where(convar => ListenedConvars.Add(convar)).ToList();

        if (fresh.Count > 0)
        {
            ClientConfig.AddEventListenerFor(fresh, MenuRegistry.RefreshAll);
        }
    }

    private static void RebuildRows()
    {
        if (_pluginsBuilder is not { } builder)
        {
            return;
        }

        builder.ClearEntries();

        var rows = new List<MenuEntry>();

        foreach (var state in Plugins.Values.OrderBy(static plugin => plugin.Resource, StringComparer.OrdinalIgnoreCase))
        {
            state.Builders.Clear();
            state.NodesByItem.Clear();

            // An empty menu gets no row at all, so a plugin that only contributes player actions does not
            // advertise a menu with nothing in it. The first rows it adds bring the row into being.
            if (state.RootMenu is not { Items.Count: > 0 } root)
            {
                continue;
            }

            var plugin = state;

            rows.Add(new SubmenuEntry
            {
                Text = MenuText.From(() => DisplayNameOf(plugin)),
                Description = MenuText.From(() => RowDescriptionOf(plugin)),
                // Live, so a plugin renaming its own menu after it connected lands.
                MenuTitle = MenuText.From(() => root.Title is { } title
                    ? plugin.Resolve(title)
                    : DisplayNameOf(plugin)),
                MenuSubtitle = PluginEntryFactory.LiveText(plugin, () => root.Subtitle),
                Build = childBuilder => PluginEntryFactory.BuildMenu(plugin, root, childBuilder),
            });
        }

        builder.AddRange(rows);

        // After materialisation, so the filter never runs over a menu that has no items yet.
        foreach (var state in Plugins.Values)
        {
            foreach (var menuBuilder in state.Builders.Values)
            {
                menuBuilder.SetUserFilter(state.VisibilityFilter);
            }
        }
    }

    private static string DisplayNameOf(PluginState state) =>
        state.DisplayName is { } name && state.Resolve(name) is { Length: > 0 } resolved
            ? resolved
            : state.Resource;

    private static string RowDescriptionOf(PluginState state)
    {
        var text = Localizer.Current
            .Get(Loc.Plugins.RowDescription)
            .Replace("{resource}", state.Resource);

        if (state.DescriptionRef is { } description)
        {
            text += "~n~" + state.Resolve(description);
        }

        return text;
    }

    // For a plugin whose menu was empty at registration and has just been given its first rows. Until
    // then it has no row, and so no live menu to add anything to.
    internal static void MaterialiseRows()
    {
        CloseIfInsideAPluginMenu();

        RebuildRows();
    }

    // Call this before the registered set changes, never after: it recognises the menu on screen by
    // walking that set, so a plugin already added or removed makes it answer no.
    private static void CloseIfInsideAPluginMenu()
    {
        if (MenuController.GetCurrentMenu() is { } open && OwnerOf(open) is not null)
        {
            MenuController.CloseAllMenus();
        }
    }

    private static PluginState? OwnerOf(Menu menu)
    {
        foreach (var state in Plugins.Values)
        {
            foreach (var builder in state.Builders.Values)
            {
                if (ReferenceEquals(builder.Menu, menu))
                {
                    return state;
                }
            }
        }

        return null;
    }

    private static void OnUpdate(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        if (!Plugins.TryGetValue(resource, out var state))
        {
            Log.Warning($"[Plugins] '{resource}' sent an update but is not registered.");
            return;
        }

        if (!ClientJson.TryDeserialize<UpdateBatch>(json, out var batch) || batch is null)
        {
            Log.Warning($"[Plugins] An update from '{resource}' did not parse.");
            return;
        }

        try
        {
            PluginUpdateOps.Apply(state, batch);
        }
        catch (Exception exception)
        {
            Log.Error($"[Plugins] An update from '{resource}' failed: {exception}");
        }
    }

    internal static void ReapplyFilter(PluginState state, string menuId)
    {
        if (state.Builders.TryGetValue(menuId, out var builder))
        {
            builder.SetUserFilter(state.VisibilityFilter);
        }
    }

    internal static void RaiseChanged()
    {
        try
        {
            PluginsChanged?.Invoke();
        }
        catch (Exception exception)
        {
            Log.Error($"[Plugins] A PluginsChanged handler threw: {exception}");
        }
    }

    private static void OnNotify(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        if (!Plugins.TryGetValue(resource, out var state))
        {
            Log.Warning($"[Plugins] '{resource}' sent a notification but is not registered. Use the vmenu:notify event instead.");
            return;
        }

        if (!ClientJson.TryDeserialize<NotifyRequest>(json, out var request) || request is null)
        {
            return;
        }

        var duration = Math.Clamp(
            request.DurationMs is > 0 and { } wanted ? wanted : Notifications.DefaultDurationMs,
            MinNotifyDurationMs,
            MaxNotifyDurationMs);

        Notifications.Show(StyleFor(request.Style), state.Resolve(request.Text), duration, resource);
    }

    // Nullable, because a plugin writing the payload by hand rather than through the API package can put
    // a null in there, and this handler is not wrapped in a catch.
    private static NotificationStyle StyleFor(string? style) => style?.ToLowerInvariant() switch
    {
        "success" => NotificationStyle.Success,
        "warning" => NotificationStyle.Warning,
        "error" => NotificationStyle.Error,
        _ => NotificationStyle.Info,
    };

    private static async void OnPrompt(string json)
    {
        // Read before the first await: the invoking resource is only set during the dispatch.
        if (Sender() is not { } resource)
        {
            return;
        }

        // Parsed before anything else is decided, because the plugin is awaiting an answer carrying this
        // request's id and every path out of here owes it one.
        if (!ClientJson.TryDeserialize<PromptRequest>(json, out var request)
            || request is null
            || request.Prompts.Count == 0)
        {
            Log.Warning($"[Plugins] A prompt from '{resource}' did not parse, so it cannot be answered.");
            return;
        }

        if (!Plugins.TryGetValue(resource, out var state))
        {
            Log.Warning($"[Plugins] '{resource}' asked for input but is not registered.");
            ReplyPrompt(resource, new PromptResult { RequestId = request.RequestId, Cancelled = true });
            return;
        }

        if (_promptBusy)
        {
            ReplyPrompt(resource, new PromptResult { RequestId = request.RequestId, Busy = true, Cancelled = true });
            return;
        }

        _promptBusy = true;

        try
        {
            var prompts = new InputPrompt[request.Prompts.Count];

            for (var index = 0; index < prompts.Length; index++)
            {
                var prompt = request.Prompts[index];

                prompts[index] = new InputPrompt(
                    PluginEntryFactory.TextFor(state, prompt.Title),
                    Math.Clamp(prompt.MaxLength, 1, MaxPromptLength),
                    prompt.Initial ?? string.Empty,
                    prompt.Suggestions?.Select(static suggestion => new InputSuggestion
                    {
                        Value = suggestion.Value,
                        Label = string.IsNullOrEmpty(suggestion.Description) ? suggestion.Value : suggestion.Description,
                    }).ToList());
            }

            var answers = await UserInput.GetTextAsync(prompts);

            ReplyPrompt(resource, new PromptResult
            {
                RequestId = request.RequestId,
                Cancelled = answers is null,
                Answers = answers?.ToList(),
            });
        }
        catch (Exception exception)
        {
            Log.Error($"[Plugins] A prompt for '{resource}' failed: {exception}");

            ReplyPrompt(resource, new PromptResult { RequestId = request.RequestId, Cancelled = true });
        }
        finally
        {
            _promptBusy = false;
        }
    }

    private static void OnSetTheme(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        if (!Plugins.ContainsKey(resource))
        {
            Log.Warning($"[Plugins] '{resource}' asked for a theme but is not registered.");
            return;
        }

        if (!ClientJson.TryDeserialize<ThemeRequest>(json, out var request) || request is null)
        {
            Log.Warning($"[Plugins] A theme request from '{resource}' did not parse.");
            return;
        }

        if (request.Theme is not { Length: > 0 } theme)
        {
            MenuSkin.ClearOverride();
            return;
        }

        if (MenuSkin.TryApplyOverride(theme))
        {
            return;
        }

        Log.Warning($"[Plugins] '{resource}' asked for theme '{theme}', which vMenu does not know about.");

        SendThemes(resource);
    }

    private static void OnRegisterThemes(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        if (!ClientJson.TryDeserialize<RegisterThemesRequest>(json, out var request) || request is null)
        {
            Log.Warning($"[Plugins] The themes from '{resource}' did not parse.");
            ReplyThemes(resource, Refused("The theme payload did not parse. It has to be JSON."));

            return;
        }

        MenuSkin.RemoveCustomFrom(resource);

        var result = new RegisterResult { Accepted = true };
        var accepted = 0;

        foreach (var theme in request.Themes)
        {
            if (accepted >= MaxThemesPerResource)
            {
                result.Warnings.Add(
                    $"Only the first {MaxThemesPerResource} themes were taken, the rest were skipped.");

                break;
            }

            if (MenuSkin.TryRegisterCustom(
                    resource, theme.Id, theme.Name, theme.Css, theme.Banner, out var error, out var warning))
            {
                accepted++;

                if (warning is not null)
                {
                    result.Warnings.Add(warning);

                    Log.Warning($"[Plugins] The banner of theme '{theme.Id}' from '{resource}': {warning}");
                }

                continue;
            }

            result.Warnings.Add(error);

            Log.Warning($"[Plugins] A theme from '{resource}' was skipped: {error}");
        }

        if (accepted == 0 && request.Themes.Count > 0)
        {
            result.Accepted = false;
        }

        Log.Info($"[Plugins] '{resource}' provided {accepted} theme(s).");

        ReplyThemes(resource, result);

        MenuSkin.Refresh();
    }

    private static void ReplyThemes(string resource, RegisterResult result) =>
        NativeFixer.EmitLocal(PluginEvents.ThemesRegisteredFor(resource), ClientJson.Serialize(result));

    private static void BroadcastThemes()
    {
        if (Plugins.Count == 0)
        {
            return;
        }

        var json = ClientJson.Serialize(BuildThemeList());

        foreach (var state in Plugins.Values)
        {
            NativeFixer.EmitLocal(PluginEvents.ThemesFor(state.Resource), json);
        }
    }

    private static void SendThemes(string resource) =>
        NativeFixer.EmitLocal(PluginEvents.ThemesFor(resource), ClientJson.Serialize(BuildThemeList()));

    private static ThemeList BuildThemeList()
    {
        var list = new ThemeList
        {
            Current = MenuSkin.CurrentId,
            Configured = MenuSkin.ConfiguredId,
            Overridden = MenuSkin.IsOverridden,
        };

        foreach (var choice in MenuSkin.Choices())
        {
            list.Themes.Add(new ThemeInfo { Id = choice.Id, Name = choice.Name });
        }

        return list;
    }

    private static void ReplyPrompt(string resource, PromptResult result) =>
        NativeFixer.EmitLocal(PluginEvents.PromptResultFor(resource), ClientJson.Serialize(result));

    private static void Reply(string resource, RegisterResult result) =>
        NativeFixer.EmitLocal(PluginEvents.RegisterResultFor(resource), ClientJson.Serialize(result));

    private static RegisterResult Refused(string reason)
    {
        var result = new RegisterResult { Accepted = false };
        result.Errors.Add(reason);

        return result;
    }

    private static string? Sender()
    {
        var resource = Native.GetInvokingResource();

        if (string.IsNullOrEmpty(resource) || resource == Native.GetCurrentResourceName())
        {
            Log.Warning("[Plugins] Ignored a plugin event without an invoking resource.");
            return null;
        }

        return resource;
    }
}

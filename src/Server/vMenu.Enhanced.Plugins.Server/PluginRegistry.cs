using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.PluginContracts;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Webhooks.Server;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Plugins.Server;

// Handlers are registered imperatively because attribute discovery only scans the assembly named as
// the server_script, and this one is a project reference.
public static class PluginRegistry
{
    private const int RefreshDebounceMs = 500;

    private const string StopEvent = "onResourceStop";

    private static readonly Dictionary<string, RegisteredServerPlugin> Registered = new(StringComparer.OrdinalIgnoreCase);

    // Sanitized id to owning resource, so two resources cannot claim the same names.
    private static readonly Dictionary<string, string> IdOwners = new(StringComparer.OrdinalIgnoreCase);

    // Last accepted payload per resource, so a repeated registration costs nothing.
    private static readonly Dictionary<string, string> LastAccepted = new(StringComparer.OrdinalIgnoreCase);

    private static bool _handlersRegistered;

    private static bool _refreshPending;

    public static IReadOnlyCollection<RegisteredServerPlugin> Plugins => Registered.Values;

    public static void RegisterEventHandlers()
    {
        if (_handlersRegistered)
        {
            return;
        }

        _handlersRegistered = true;

        API.OnEvent(PluginEvents.ServerProbe, new Action(OnProbe), false);
        API.OnEvent(PluginEvents.ServerRegister, new Action<string>(OnRegister), false);
        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);
    }

    // Its templates stay on disk, there being no native that deletes a file. They say on the first line
    // that they are only written while the plugin runs, which is what tells an owner reading a stale one
    // that the plugin behind it is gone.
    private static void OnResourceStop(string stopped)
    {
        if (!Registered.Remove(stopped, out var plugin))
        {
            return;
        }

        LastAccepted.Remove(stopped);
        IdOwners.Remove(plugin.Id);

        var removed = PermissionRegistry.UnregisterDynamic(PluginPermissions.AllFor(plugin.Id));

        Log.Info($"[Plugins] '{stopped}' stopped, dropping its {removed} permission(s).");

        WebhookLog.Event(
            "Plugin '" + plugin.DisplayName + "' unregistered.",
            WebhookActor.Server,
            ("resource", stopped));

        ScheduleRefresh();
    }

    // Call once startup is done, so plugins that started first know to register now.
    public static void AnnounceReady() =>
        NativeFixer.EmitLocal(PluginEvents.ServerReady, PluginProtocol.Version);

    private static void OnProbe()
    {
        if (Sender() is { } resource)
        {
            NativeFixer.EmitLocal(PluginEvents.ServerReadyFor(resource), PluginProtocol.Version);
        }
    }

    private static void OnRegister(string json)
    {
        if (Sender() is not { } resource)
        {
            return;
        }

        try
        {
            Register(resource, json);
        }
        catch (Exception exception)
        {
            Log.Error($"[Plugins] Registration from '{resource}' failed: {exception.Message}");
            Reply(resource, Refused($"vMenu hit an internal error: {exception.Message}"));
        }
    }

    private static void Register(string resource, string json)
    {
        if (LastAccepted.TryGetValue(resource, out var previous) && previous == json)
        {
            Reply(resource, new RegisterResult { Accepted = true });
            return;
        }

        if (!ServerJson.TryDeserialize<ServerRegisterRequest>(json, out var request, out var error) || request is null)
        {
            Reply(resource, Refused($"The registration payload did not parse: {error ?? "empty payload"}."));
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

        if (!PermissionPath.IsValidSegment(id))
        {
            Reply(resource, Refused($"The resource name '{resource}' cannot be turned into a usable identity."));
            return;
        }

        if (IdOwners.TryGetValue(id, out var owner) && !owner.Equals(resource, StringComparison.OrdinalIgnoreCase))
        {
            Reply(resource, Refused(
                $"The identity '{id}' is already taken by resource '{owner}'. Rename one of the two resources."));
            return;
        }

        var result = new RegisterResult { Accepted = true };
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName) ? resource : request.DisplayName.Trim();

        var permissions = RegisterPermissions(resource, id, request.Permissions, result);
        var settings = BuildSettings(id, request.Settings, result);

        var plugin = new RegisteredServerPlugin
        {
            Resource = resource,
            Id = id,
            DisplayName = displayName,
            Settings = settings,
            Permissions = permissions,
        };

        IdOwners[id] = resource;
        Registered[resource] = plugin;
        LastAccepted[resource] = json;

        PluginTemplates.Write(plugin);
        ScheduleRefresh();

        Log.Info(
            $"[Plugins] Registered '{displayName}' from resource '{resource}' with "
            + $"{permissions.Count} permission(s) and {settings.Count} setting(s).");

        WebhookLog.Event(
            "Plugin '" + displayName + "' registered.",
            WebhookActor.Server,
            ("resource", resource),
            ("permissions", permissions.Count.ToString(CultureInfo.InvariantCulture)),
            ("settings", settings.Count.ToString(CultureInfo.InvariantCulture)));

        Reply(resource, result);
    }

    private static List<string> RegisterPermissions(
        string resource,
        string id,
        List<PermissionDeclaration>? declarations,
        RegisterResult result)
    {
        var permissions = new List<string>();

        // Dropped first, and before the empty check: a plugin that renamed a permission, changed one to staff
        // only or took every last one away gets exactly what it declares now. RegisterDynamic leaves an
        // existing node alone, so without this the tree would keep handing out the previous registration.
        PermissionRegistry.UnregisterDynamic(PluginPermissions.AllFor(id));

        if (declarations is null || declarations.Count == 0)
        {
            return permissions;
        }

        var source = $"plugin {resource}";

        if (!PermissionRegistry.RegisterDynamic(PluginPermissions.AllFor(id), source))
        {
            result.Warnings.Add("The plugin's own container grant could not be registered, so none of its permissions were.");
            return permissions;
        }

        foreach (var declaration in declarations)
        {
            if (!PermissionPath.IsValidSegment(declaration.Name))
            {
                result.Warnings.Add(
                    $"Permission '{declaration.Name}' was skipped: names may only contain letters, digits and underscores.");
                continue;
            }

            var permission = PluginPermissions.For(id, declaration.Name);

            if (PermissionRegistry.RegisterDynamic(permission, source, declaration.StaffOnly))
            {
                permissions.Add(permission);
            }
            else
            {
                result.Warnings.Add($"Permission '{declaration.Name}' was refused by the permission registry.");
            }
        }

        return permissions;
    }

    private static List<Setting> BuildSettings(string id, List<SettingNode>? nodes, RegisterResult result)
    {
        var settings = new List<Setting>();

        if (nodes is null || nodes.Count == 0)
        {
            return settings;
        }

        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in nodes)
        {
            if (!ConfigPath.IsValidSegment(node.Name))
            {
                result.Warnings.Add(
                    $"Setting '{node.Name}' was skipped: names may only contain letters, digits and underscores.");
                continue;
            }

            if (!taken.Add(node.Name))
            {
                result.Warnings.Add($"Setting '{node.Name}' was declared twice, the second one was skipped.");
                continue;
            }

            var fullName = PluginPermissions.Prefix + ConfigPath.Separator + id + ConfigPath.Separator + node.Name;
            var setting = PluginSettingFactory.Create(node, fullName, out var problem);

            if (setting is null)
            {
                result.Warnings.Add(problem ?? $"Setting '{node.Name}' was skipped.");
                continue;
            }

            settings.Add(setting);
        }

        return settings;
    }

    // One resend for a whole burst of boot time registrations instead of one per plugin.
    private static void ScheduleRefresh()
    {
        if (_refreshPending)
        {
            return;
        }

        _refreshPending = true;

        _ = FlushAsync();

        static async Task FlushAsync()
        {
            await API.Delay(RefreshDebounceMs);

            _refreshPending = false;

            PermissionsSync.RefreshAll();
        }
    }

    private static void Reply(string resource, RegisterResult result) =>
        NativeFixer.EmitLocal(PluginEvents.ServerRegisterResultFor(resource), ServerJson.Serialize(result));

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

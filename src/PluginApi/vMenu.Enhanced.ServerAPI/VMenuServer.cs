using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ServerAPI;

/// <summary>The server side entry point for a plugin. Call <see cref="RegisterAsync"/> once at
/// startup; re-registration after a vMenu restart happens automatically. Then use
/// <see cref="IsPlayerAllowed"/> to check the permissions you declared.</summary>
public static class VMenuServer
{
    private const string PermissionPrefix = "vMenu.Enhanced.Plugins";

    private const string Everything = "vMenu.Enhanced.Everything";

    private static ServerPluginDeclaration? _declaration;

    private static TaskCompletionSource<RegisterResult>? _firstResult;

    private static bool _handlersRegistered;

    private static string _resource = string.Empty;

    private static string _pluginId = string.Empty;

    /// <summary>Fires on every registration answer, including automatic re-registrations.</summary>
    public static event Action<RegisterResult>? RegistrationAnswered;

    /// <summary>Declares the plugin with vMenu. The task completes on vMenu's first answer, which can be
    /// a while when vMenu starts later than the plugin. It never throws: a refusal arrives as a result
    /// with <c>Accepted</c> false.</summary>
    public static Task<RegisterResult> RegisterAsync(ServerPluginDeclaration declaration)
    {
        _declaration = declaration;
        _firstResult ??= new TaskCompletionSource<RegisterResult>();

        EnsureHandlers();
        SendRegistration();
        PluginEmit.Local(PluginEvents.ServerProbe);

        return _firstResult.Task;
    }

    /// <summary>Whether a player holds one of the plugin's own permissions, by its short name. Also
    /// honours the container grants a server owner may have used instead of the exact name.</summary>
    public static bool IsPlayerAllowed(string playerSource, string permissionName)
    {
        if (string.IsNullOrEmpty(playerSource))
        {
            return false;
        }

        EnsureIdentity();

        var scope = PermissionPrefix + "." + _pluginId;

        return Native.IsPlayerAceAllowed(playerSource, scope + "." + permissionName)
            || Native.IsPlayerAceAllowed(playerSource, scope + ".All")
            || Native.IsPlayerAceAllowed(playerSource, PermissionPrefix + ".All")
            || Native.IsPlayerAceAllowed(playerSource, Everything);
    }

    private static void EnsureIdentity()
    {
        if (_resource.Length == 0)
        {
            _resource = Native.GetCurrentResourceName();
            _pluginId = PluginId.Sanitize(_resource);
        }
    }

    private static void EnsureHandlers()
    {
        if (_handlersRegistered)
        {
            return;
        }

        _handlersRegistered = true;

        EnsureIdentity();

        API.OnEvent(PluginEvents.ServerReady, new Action<int>(OnReady), false);
        API.OnEvent(PluginEvents.ServerReadyFor(_resource), new Action<int>(OnReady), false);
        API.OnEvent(PluginEvents.ServerRegisterResultFor(_resource), new Action<string>(OnResult), false);
    }

    private static void OnReady(int protocolVersion) => SendRegistration();

    private static void SendRegistration()
    {
        if (_declaration is { } declaration)
        {
            PluginEmit.Local(PluginEvents.ServerRegister, PluginJson.Serialize(declaration.ToRequest()));
        }
    }

    private static void OnResult(string json)
    {
        if (!PluginJson.TryDeserialize<RegisterResult>(json, out var result) || result is null)
        {
            SharedAPI.Log.Warn($"[{_resource}] vMenu sent a registration answer that did not parse.");
            return;
        }

        foreach (var error in result.Errors)
        {
            SharedAPI.Log.Error($"[{_resource}] vMenu refused the plugin registration: {error}");
        }

        foreach (var warning in result.Warnings)
        {
            SharedAPI.Log.Warn($"[{_resource}] vMenu accepted the plugin registration with a note: {warning}");
        }

        _firstResult?.TrySetResult(result);

        try
        {
            RegistrationAnswered?.Invoke(result);
        }
        catch (Exception exception)
        {
            SharedAPI.Log.Error($"[{_resource}] A RegistrationAnswered handler threw: {exception}");
        }
    }
}

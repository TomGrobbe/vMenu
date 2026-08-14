using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Live ACE checks, and computing the minimal set of permissions to hand a client.
/// </summary>
public static class ServerPermissions
{
    public static bool IsReady { get; private set; }

    /// <summary>Call once, first, from the server entry point.</summary>
    public static void Initialize()
    {
        IsReady = false;

        PermissionRegistry.Build(typeof(Global).Assembly);
        ModelWhitelist.LoadAndRegister();
        VehicleCategories.LoadAndRegister();
        PedCategories.LoadAndRegister();
        WeaponCatalog.LoadAndRegister();

        IsReady = true;
    }

    /// <summary>
    /// Inheritance is applied here, so callers never name a parent themselves. Nothing about the
    /// result is cached, so an <c>add_ace</c> takes effect on the next call.
    /// </summary>
    public static bool IsPlayerAllowed(string source, string permission)
    {
        if (string.IsNullOrEmpty(source) || !Native.DoesPlayerExist(source))
        {
            return false;
        }

        foreach (var ace in PermissionRegistry.GetAncestorChain(permission))
        {
            if (Native.IsPlayerAceAllowed(source, ace))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc cref="IsPlayerAllowed(string, string)"/>
    public static bool IsPlayerAllowed(Player player, string permission) =>
        IsPlayerAllowed(ToSource(player), permission);

    /// <summary>
    /// The smallest set describing what a player may do. The walk stops descending the moment a
    /// node is granted, because a granted parent is absolute: nothing below it can take the grant
    /// away. That is also why the client can rebuild the same answers from names alone.
    /// </summary>
    public static string[] GetGrantedPermissions(string source)
    {
        if (string.IsNullOrEmpty(source) || !Native.DoesPlayerExist(source))
        {
            return [];
        }

        var probed = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (IsGranted(source, Global.Everything, probed))
        {
            return [Global.Everything];
        }

        var granted = new List<string>();

        foreach (var root in PermissionRegistry.Roots)
        {
            Collect(source, root, granted, probed);
        }

        return [.. granted];
    }

    /// <inheritdoc cref="GetGrantedPermissions(string)"/>
    public static string[] GetGrantedPermissions(Player player) =>
        GetGrantedPermissions(ToSource(player));

    /// <summary>Must run on the main thread; the underlying emit asserts it.</summary>
    public static void SendPermissions(Player player, int latentBytesPerSecond = 0) =>
        SendPermissions(player.Handle, latentBytesPerSecond);

    /// <inheritdoc cref="SendPermissions(Player, int)"/>
    public static void SendPermissions(int handle, int latentBytesPerSecond = 0)
    {
        var source = handle.ToString(CultureInfo.InvariantCulture);

        if (!IsReady)
        {
            Log.Error($"[Permissions] Refusing to send permissions to {source}: the registry is not ready yet.");
            return;
        }

        var granted = GetGrantedPermissions(source);
        var whitelistedVehicles = ModelWhitelist.GetModels(SupplementalModelKind.Vehicle);
        var categorisedVehicles = VehicleCategories.GetCategorisedModels();
        var vehicleCategories = VehicleCategories.GetCategoryNames();
        var whitelistedPeds = ModelWhitelist.GetModels(SupplementalModelKind.Ped);
        var whitelistedWeapons = ModelWhitelist.GetModels(SupplementalModelKind.Weapon);

        if (latentBytesPerSecond > 0)
        {
            API.EmitClientLatent(handle, latentBytesPerSecond, PermissionEvents.Set, granted, whitelistedVehicles, categorisedVehicles, vehicleCategories, whitelistedPeds, whitelistedWeapons);
        }
        else
        {
            API.EmitClient(handle, PermissionEvents.Set, granted, whitelistedVehicles, categorisedVehicles, vehicleCategories, whitelistedPeds, whitelistedWeapons);
        }

        Log.Debug($"[Permissions] Sent {granted.Length} permission(s) to {Native.GetPlayerName(source)}: {string.Join(", ", granted)}");
    }

    public static void LogTree()
    {
        Log.Info($"[Permissions] {PermissionRegistry.Count} permission(s):");

        foreach (var (node, depth) in PermissionRegistry.EnumerateTree())
        {
            var indent = new string(' ', (depth + 1) * 2);
            var extras = node.ExtraParents.Count > 0 ? $"  (also granted by {string.Join(", ", node.ExtraParents)})" : string.Empty;
            var dynamic = node.Source is not null ? $" [runtime, from {node.Source}]" : string.Empty;

            Log.Info($"{indent}{node.Name}{dynamic}{extras}");
        }
    }

    private static void Collect(string source, PermissionNode node, List<string> granted, Dictionary<string, bool> probed)
    {
        if (IsGranted(source, node.Name, probed))
        {
            granted.Add(node.Name);
            return;
        }

        foreach (var child in node.StructuralChildren)
        {
            Collect(source, child, granted, probed);
        }
    }

    /// <summary>
    /// Tests the whole chain, not just this node, because a cross-tree parent may itself be held
    /// only through its own container, which the walk has not descended into. Ancestors already
    /// found denied cost nothing to re-test, being in <paramref name="probed"/>.
    /// </summary>
    private static bool IsGranted(string source, string permission, Dictionary<string, bool> probed)
    {
        foreach (var ace in PermissionRegistry.GetAncestorChain(permission))
        {
            if (probed.TryGetValue(ace, out var cached))
            {
                if (cached)
                {
                    return true;
                }

                continue;
            }

            var allowed = Native.IsPlayerAceAllowed(source, ace);
            probed[ace] = allowed;

            if (allowed)
            {
                return true;
            }
        }

        return false;
    }

    private static string ToSource(Player player) =>
        player.Handle.ToString(CultureInfo.InvariantCulture);
}

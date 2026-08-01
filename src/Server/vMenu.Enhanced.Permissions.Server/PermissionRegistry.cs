using System.Reflection;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Permissions;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Every known permission: discovered from the data assembly at startup, extended at runtime by
/// configuration, and shaped into the tree <see cref="ServerPermissions"/> walks.
/// </summary>
public static class PermissionRegistry
{
    private const string DataNamespaceMarker = ".Data.Permissions";

    private static readonly Dictionary<string, PermissionNode> Nodes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<PermissionNode> RootNodes = [];

    /// <summary>
    /// Caches tree topology, which never changes after startup, and never a permission result, so
    /// a live ACE change is picked up by the next check.
    /// </summary>
    private static readonly Dictionary<string, string[]> ChainCache = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<PermissionNode> Roots => RootNodes;

    public static int Count => Nodes.Count;

    /// <param name="assembly">
    /// Passed explicitly rather than scanning loaded assemblies, so discovery does not depend on
    /// load order.
    /// </param>
    public static void Build(Assembly assembly)
    {
        Nodes.Clear();
        RootNodes.Clear();
        ChainCache.Clear();

        var discovered = Discover(assembly);

        // Metadata order is stable per build but not contractual, so sort for a deterministic tree.
        foreach (var (name, extraParents) in discovered.OrderBy(entry => entry.Name, StringComparer.Ordinal))
        {
            Nodes[name] = new PermissionNode
            {
                Name = name,
                ExtraParents = extraParents,
            };
        }

        LinkNodes();
        ValidateExtraParents();

        API.Log.Info($"[Permissions] Registered {Nodes.Count} permissions across {RootNodes.Count} roots.");
    }

    /// <summary>Registers a permission whose name comes from configuration.</summary>
    public static bool RegisterDynamic(string permission)
    {
        if (!PermissionPath.IsValidPermission(permission))
        {
            API.Log.Warn($"[Permissions] Ignoring runtime permission '{permission}': not a valid permission name.");
            return false;
        }

        if (Nodes.ContainsKey(permission))
        {
            return true;
        }

        var parent = FindStructuralParent(permission);

        if (parent is null)
        {
            API.Log.Warn($"[Permissions] Ignoring runtime permission '{permission}': no registered container grant above it.");
            return false;
        }

        var node = new PermissionNode
        {
            Name = permission,
            IsDynamic = true,
            ExtraParents = parent.ExtraParents,
            StructuralParent = parent,
        };

        parent.StructuralChildren.Add(node);
        Nodes[permission] = node;

        ChainCache.Clear();

        return true;
    }

    public static bool TryGet(string permission, out PermissionNode? node) =>
        Nodes.TryGetValue(permission, out node);

    /// <summary>
    /// Every permission that grants <paramref name="permission"/>, itself included: container
    /// grants nearest first, then cross-tree parents, then <see cref="Global.Everything"/> last so
    /// the grant a server owner actually wrote is usually found within a probe or two.
    /// </summary>
    public static string[] GetAncestorChain(string permission)
    {
        if (ChainCache.TryGetValue(permission, out var cached))
        {
            return cached;
        }

        var chain = BuildChain(permission);
        ChainCache[permission] = chain;

        return chain;
    }

    public static IEnumerable<(PermissionNode Node, int Depth)> EnumerateTree()
    {
        foreach (var root in RootNodes)
        {
            foreach (var entry in Walk(root, 0))
            {
                yield return entry;
            }
        }

        static IEnumerable<(PermissionNode Node, int Depth)> Walk(PermissionNode node, int depth)
        {
            yield return (node, depth);

            foreach (var child in node.StructuralChildren)
            {
                foreach (var entry in Walk(child, depth + 1))
                {
                    yield return entry;
                }
            }
        }
    }

    private static List<(string Name, string[] ExtraParents)> Discover(Assembly assembly)
    {
        var discovered = new List<(string Name, string[] ExtraParents)>();
        var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in assembly.GetTypes())
        {
            var category = type.GetCustomAttribute<PermissionCategoryAttribute>();

            if (category is null)
            {
                continue;
            }

            var prefix = category.Prefix ?? DerivePrefix(type);
            var hasContainerGrant = false;
            var declaredInCategory = 0;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                // A const string is a literal, non-readonly field.
                if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                {
                    continue;
                }

                if (field.GetRawConstantValue() is not string value)
                {
                    continue;
                }

                // Constants no deeper than the prefix are helpers, not permissions.
                if (!value.StartsWith(prefix + PermissionPath.Separator, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!PermissionPath.IsValidPermission(value))
                {
                    API.Log.Error($"[Permissions] {type.Name}.{field.Name} is not a valid permission name: '{value}'. Segments may only contain letters, digits and underscores.");
                    continue;
                }

                if (owners.TryGetValue(value, out var existing))
                {
                    API.Log.Error($"[Permissions] Duplicate permission '{value}' declared by both {existing} and {type.Name}.{field.Name}.");
                    continue;
                }

                owners[value] = $"{type.Name}.{field.Name}";
                hasContainerGrant |= PermissionPath.IsContainerGrant(value);
                declaredInCategory++;
                discovered.Add((value, category.AdditionalParents));
            }

            // A single-permission category has nothing to group, so no container grant is expected.
            if (!hasContainerGrant && declaredInCategory > 1)
            {
                API.Log.Warn($"[Permissions] Category {type.Name} has no '{PermissionPath.All}' permission, so it cannot be granted as a whole.");
            }
        }

        return discovered;
    }

    private static string DerivePrefix(Type type)
    {
        var declaringNamespace = type.Namespace ?? string.Empty;
        var markerIndex = declaringNamespace.IndexOf(DataNamespaceMarker, StringComparison.Ordinal);

        var basePath = markerIndex >= 0
            ? declaringNamespace.Remove(markerIndex, DataNamespaceMarker.Length)
            : declaringNamespace;

        return $"{basePath}{PermissionPath.Separator}{type.Name}";
    }

    private static void LinkNodes()
    {
        foreach (var node in Nodes.Values)
        {
            var parent = FindStructuralParent(node.Name);

            if (parent is null)
            {
                RootNodes.Add(node);
                continue;
            }

            node.StructuralParent = parent;
            parent.StructuralChildren.Add(node);
        }

        RootNodes.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));

        foreach (var node in Nodes.Values)
        {
            node.StructuralChildren.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));
        }
    }

    private static PermissionNode? FindStructuralParent(string permission)
    {
        foreach (var candidate in PermissionPath.EnumerateContainerGrants(permission))
        {
            if (Nodes.TryGetValue(candidate, out var parent))
            {
                return parent;
            }
        }

        return null;
    }

    private static void ValidateExtraParents()
    {
        foreach (var node in Nodes.Values)
        {
            foreach (var extra in node.ExtraParents)
            {
                if (!Nodes.ContainsKey(extra))
                {
                    API.Log.Error($"[Permissions] '{node.Name}' names unregistered permission '{extra}' as an additional parent.");
                }
            }
        }
    }

    private static string[] BuildChain(string permission)
    {
        var ordered = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<string>();

        Enqueue(permission);

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();

            foreach (var grant in PermissionPath.EnumerateContainerGrants(current))
            {
                Enqueue(grant);
            }

            if (Nodes.TryGetValue(current, out var node))
            {
                foreach (var extra in node.ExtraParents)
                {
                    Enqueue(extra);
                }
            }
        }

        if (seen.Add(Global.Everything))
        {
            ordered.Add(Global.Everything);
        }

        return [.. ordered];

        void Enqueue(string name)
        {
            if (seen.Add(name))
            {
                ordered.Add(name);
                pending.Enqueue(name);
            }
        }
    }
}

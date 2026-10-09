using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

internal static class PluginDiff
{
    internal static bool Same(TextRef? left, TextRef? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return string.Equals(left.Text, right.Text, StringComparison.Ordinal)
            && string.Equals(left.Key, right.Key, StringComparison.Ordinal)
            && Same(left.Args, right.Args);
    }

    internal static bool Same(List<TextRef>? left, List<TextRef>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!Same(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool Same(GateNode? left, GateNode? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return string.Equals(left.Permission, right.Permission, StringComparison.Ordinal)
            && string.Equals(left.Setting, right.Setting, StringComparison.Ordinal)
            && Same(left.All, right.All)
            && Same(left.Any, right.Any);
    }

    private static bool Same(List<GateNode>? left, List<GateNode>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!Same(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Same(Dictionary<string, TextRef>? left, Dictionary<string, TextRef>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var other) || !Same(pair.Value, other))
            {
                return false;
            }
        }

        return true;
    }
}

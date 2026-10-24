namespace vMenu.Enhanced.Actions.Server;

/// <summary>Finds an item in an owner edited list by the name players see.</summary>
public static class ListExtensions
{
    /// <summary>Ignores case and surrounding spaces, the way owners and players both type names. A blank name
    /// matches nothing.</summary>
    public static int IndexOfName<T>(this List<T> items, string? name, Func<T, string?> nameOf)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return -1;
        }

        for (var index = 0; index < items.Count; index++)
        {
            if (string.Equals(nameOf(items[index]), name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }
}

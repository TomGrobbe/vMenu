using System.Runtime.CompilerServices;

namespace vMenu.Enhanced.Plugins;

// Compares by reference. A local copy of the framework's internal comparer, for the same two
// reasons: MenuAPI items have no equality override, and EqualityComparer<T>.Default uses internal
// types the sandbox does not always permit.
internal sealed class ReferenceComparer<T> : IEqualityComparer<T>
    where T : class
{
    internal static ReferenceComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}

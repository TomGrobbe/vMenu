using System.Runtime.CompilerServices;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Compares by reference.
/// </summary>
// Handed to every dictionary and set in the framework, for two reasons. MenuAPI does not override
// equality today, and a value equality override landing upstream would collapse two identical
// looking items into one. It also keeps lookups away from EqualityComparer<T>.Default, whose
// internal types the sandbox does not always permit.
internal sealed class ReferenceComparer<T> : IEqualityComparer<T>
    where T : class
{
    internal static ReferenceComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}

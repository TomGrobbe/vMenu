using System.Runtime.CompilerServices;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Compares by reference.
/// </summary>
/// <remarks>
/// Handed to every dictionary and set in the framework, for two reasons. It pins down the assumption
/// that menus and items are identified by identity — MenuAPI does not override equality today, and a
/// value-equality override landing upstream would otherwise collapse two identical-looking items
/// into one. And it keeps every lookup away from <c>EqualityComparer&lt;T&gt;.Default</c>, whose
/// internal implementation types the FiveM sandbox does not always permit.
/// </remarks>
internal sealed class ReferenceComparer<T> : IEqualityComparer<T>
    where T : class
{
    internal static ReferenceComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}

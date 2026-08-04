using System.Globalization;

namespace vMenu.Enhanced.Storage;

/// <summary>
/// One preference belonging to the player, declared once and persisted on change.
/// </summary>
/// <remarks>
/// Mirrors <c>Setting</c> in the configuration module, which does the same job for values a server
/// owner controls. There is no save step: assigning <see cref="UserDefault{T}.Value"/> persists
/// immediately.
/// </remarks>
public abstract class UserDefault(string name)
{
    /// <summary>
    /// Namespaces preferences away from bulk saves, so <see cref="KvpStore.Keys"/> can enumerate one
    /// without walking the other.
    /// </summary>
    public const string KeyPrefix = "vmenu_default_";

    public string Name { get; } = name;

    public string Key { get; } = KeyPrefix + name;

    /// <summary>One of <see cref="KvpValueType"/>.</summary>
    public abstract string TypeName { get; }

    public abstract string DefaultText { get; }

    public abstract string CurrentText { get; }

    /// <summary>Forgets the stored value, so the declared default applies again.</summary>
    public abstract void Reset();
}

/// <summary>
/// A <see cref="UserDefault"/> that knows its own type.
/// </summary>
/// <remarks>
/// Gating belongs to whoever applies the value, never here: a preference stays whatever the player
/// chose even while they lack the permission to use it, so being re-granted it restores their choice
/// instead of handing back a silently erased one. Read a gated preference as
/// <c>SomeDefault.Value &amp;&amp; ClientPermissions.IsAllowed(...)</c> — the existing
/// <c>PermissionsChanged</c> to <c>MenuRegistry.RefreshAll</c> path re-reads it, so a demoted
/// player's checkbox unticks itself with no extra plumbing.
/// </remarks>
public abstract class UserDefault<T>(string name) : UserDefault(name)
{
    public required T Default { get; init; }

    /// <summary>Raised when the stored value actually moves. Never on a read.</summary>
    public event Action? Changed;

    /// <summary>
    /// Reading a preference the player has never set writes the declared default to the store, so a
    /// dump lists everything vMenu knows about rather than only what has been touched. The cost is
    /// that a default improved in a later release does not reach anyone who has already read it.
    /// </summary>
    public T Value
    {
        get
        {
            if (KvpStore.TryRead<T>(Key, TypeName, KvpStore.InitialVersion, out var stored, out _))
            {
                return stored!;
            }

            KvpStore.TryWrite(Key, TypeName, KvpStore.InitialVersion, Default);

            return Default;
        }

        set
        {
            if (AreEqual(Value, value))
            {
                return;
            }

            if (!KvpStore.TryWrite(Key, TypeName, KvpStore.InitialVersion, value))
            {
                return;
            }

            Changed?.Invoke();
        }
    }

    public override string CurrentText => Describe(Value);

    public override string DefaultText => Describe(Default);

    public override void Reset()
    {
        KvpStore.Delete(Key);

        Changed?.Invoke();
    }

    /// <remarks>
    /// Written per type rather than through <c>EqualityComparer&lt;T&gt;.Default</c>, whose internal
    /// comparer the client sandbox refuses to load. Same rule as <c>LanguageId</c>.
    /// </remarks>
    protected abstract bool AreEqual(T left, T right);

    protected abstract string Describe(T value);
}

public sealed class BoolDefault(string name) : UserDefault<bool>(name)
{
    public override string TypeName => KvpValueType.Bool;

    protected override bool AreEqual(bool left, bool right) => left == right;

    protected override string Describe(bool value) => value ? "true" : "false";
}

public sealed class IntDefault(string name) : UserDefault<int>(name)
{
    public override string TypeName => KvpValueType.Int;

    protected override bool AreEqual(int left, int right) => left == right;

    protected override string Describe(int value) => value.ToString(CultureInfo.InvariantCulture);
}

public sealed class FloatDefault(string name) : UserDefault<float>(name)
{
    public override string TypeName => KvpValueType.Float;

    /// <remarks>
    /// Bitwise, deliberately. These are values stored and read back verbatim, with no arithmetic in
    /// between for a tolerance to absorb — and a tolerance would make the smallest step of a fine
    /// slider unsaveable.
    /// </remarks>
    protected override bool AreEqual(float left, float right) => left.Equals(right);

    protected override string Describe(float value) => value.ToString("0.0###", CultureInfo.InvariantCulture);
}

public sealed class StringDefault(string name) : UserDefault<string>(name)
{
    public override string TypeName => KvpValueType.String;

    protected override bool AreEqual(string left, string right) =>
        string.Equals(left, right, StringComparison.Ordinal);

    protected override string Describe(string value) => $"'{value}'";
}

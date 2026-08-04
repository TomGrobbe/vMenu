using System.Globalization;

namespace vMenu.Enhanced.Storage;

/// <summary>One preference belonging to the player, declared once and persisted on change.</summary>
public abstract class UserDefault(string name)
{
    public const string KeyPrefix = "vmenu_default_";

    public string Name { get; } = name;

    public string Key { get; } = KeyPrefix + name;

    public abstract string TypeName { get; }

    public abstract string DefaultText { get; }

    public abstract string CurrentText { get; }

    /// <summary>Forgets the stored value, so the declared default applies again.</summary>
    public abstract void Reset();
}

// Gating belongs to whoever applies the value, never here, so a player who is re-granted a
// permission gets their choice back instead of a silently erased one. Read a gated preference as
// SomeDefault.Value && ClientPermissions.IsAllowed(...).
public abstract class UserDefault<T>(string name) : UserDefault(name)
{
    public required T Default { get; init; }

    /// <summary>Raised when the stored value moves. Never on a read.</summary>
    public event Action? Changed;

    /// <summary>
    /// Reading a preference that was never set writes the declared default, so a dump lists
    /// everything vMenu knows about. A default improved later will not reach anyone who has already
    /// read it.
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

    // Per type rather than EqualityComparer<T>.Default, whose comparer the sandbox refuses.
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

    // Bitwise on purpose: a tolerance would make the smallest step of a fine slider unsaveable.
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

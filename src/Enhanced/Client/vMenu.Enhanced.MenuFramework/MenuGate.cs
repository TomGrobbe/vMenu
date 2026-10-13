using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>Decides whether one entry is available to the player. Not a bare Func&lt;bool&gt;, so the permission
/// form can be written as a declaration (Gate = SomePermission.Name) rather than a lambda.</summary>
public sealed class MenuGate
{
    private readonly Func<bool> _evaluate;

    private MenuGate(Func<bool> evaluate) => _evaluate = evaluate;

    public static MenuGate Always { get; } = new(static () => true);

    public static MenuGate Never { get; } = new(static () => false);

    public static MenuGate Permission(string permission) =>
        new(() => ClientPermissions.IsAllowed(permission));

    /// <summary>A gate the server owner controls through a convar rather than through ACEs.</summary>
    public static MenuGate Setting(BoolSetting setting) =>
        new(() => ClientConfig.Value(setting));

    public static MenuGate When(Func<bool> predicate) => new(predicate);

    /// <summary>No Func&lt;bool&gt; conversion on purpose: an implicitly typed lambda has no natural type, so a user
    /// defined conversion would never apply. Use When.</summary>
    public static implicit operator MenuGate(string permission) => Permission(permission);

    public static MenuGate operator &(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() && right.Evaluate());

    public static MenuGate operator |(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() || right.Evaluate());

    /// <summary>Fails closed: a refresh pass walks every entry in every menu, so one throwing predicate must not
    /// abort it.</summary>
    public bool Evaluate()
    {
        try
        {
            return _evaluate();
        }
        catch (Exception exception)
        {
            Log.Error($"[Menu] A gate threw and is being treated as denied: {exception}");

            return false;
        }
    }
}

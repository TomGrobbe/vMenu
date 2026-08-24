using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.MenuFramework;

// Decides whether one entry is available to the player. Not a bare Func<bool>, so the permission
// form can be written as a declaration (Gate = SomePermission.Name) rather than a lambda.
public sealed class MenuGate
{
    private readonly Func<bool> _evaluate;

    private MenuGate(Func<bool> evaluate) => _evaluate = evaluate;

    public static MenuGate Always { get; } = new(static () => true);

    public static MenuGate Never { get; } = new(static () => false);

    public static MenuGate Permission(string permission) =>
        new(() => ClientPermissions.IsAllowed(permission));

    // A gate the server owner controls through a convar rather than through ACEs.
    public static MenuGate Setting(BoolSetting setting) =>
        new(() => ClientConfig.Value(setting));

    public static MenuGate When(Func<bool> predicate) => new(predicate);

    // No Func<bool> conversion on purpose: an implicitly typed lambda has no natural type, so a user
    // defined conversion would never apply. Use When.
    public static implicit operator MenuGate(string permission) => Permission(permission);

    public static MenuGate operator &(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() && right.Evaluate());

    public static MenuGate operator |(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() || right.Evaluate());

    // Fails closed: a refresh pass walks every entry in every menu, so one throwing predicate must not
    // abort it.
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

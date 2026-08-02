using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Permissions;

namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// Decides whether one entry is available to the player.
/// </summary>
/// <remarks>
/// Deliberately not a bare <see cref="Func{TResult}"/>: the permission form is what almost every
/// entry needs, and the implicit conversion lets it be written as a declaration
/// (<c>Gate = SomePermission.Name</c>) rather than a lambda. Composition covers the rest, because
/// some checks are not a permission name at all — see
/// <see cref="ClientVehiclePermissions.CanSpawnVehicle(string, int)"/>.
/// </remarks>
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

    /// <summary>
    /// No matching conversion from <see cref="Func{TResult}"/> exists on purpose: an implicitly
    /// typed lambda has no natural type, so a user defined conversion would never apply and the
    /// call site would have to spell out the delegate type anyway. Use <see cref="When"/>.
    /// </summary>
    public static implicit operator MenuGate(string permission) => Permission(permission);

    public static MenuGate operator &(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() && right.Evaluate());

    public static MenuGate operator |(MenuGate left, MenuGate right) =>
        new(() => left.Evaluate() || right.Evaluate());

    /// <summary>
    /// Fails closed. A refresh pass walks every entry in every menu, so one throwing predicate must
    /// not abort it and leave the rest of the menu showing stale state.
    /// </summary>
    public bool Evaluate()
    {
        try
        {
            return _evaluate();
        }
        catch (Exception exception)
        {
            API.Log.Error($"[Menu] A gate threw and is being treated as denied: {exception}");

            return false;
        }
    }
}

namespace vMenu.Enhanced.Data.Diagnostics;

// The gate every vMenu dump command sits behind. Handed its setting at startup rather than reading
// one itself, because the tick registries register commands and cannot reference the configuration
// modules: on the server that module already references the tick one, so reading the setting there
// would be a reference cycle. Until Source is called, every gated command stays shut.
public static class DebugCommands
{
    private static Func<bool> _enabled = static () => false;

    private static Action<string> _write = static _ => { };

    private static string _convar = "the debugging convar";

    public static void Source(Func<bool> enabled, string convar, Action<string> write)
    {
        _enabled = enabled;
        _convar = convar;
        _write = write;
    }

    // Read when the command runs rather than when it is registered, so an owner flipping the convar
    // takes effect immediately and registration order stops mattering.
    public static Action Gate(Action dump) => () =>
    {
        if (!_enabled())
        {
            _write($"This command only reports while {_convar} is set to true.");

            return;
        }

        dump();
    };

    public static Action<T> Gate<T>(Action<T> dump) => argument =>
    {
        if (!_enabled())
        {
            _write($"This command only reports while {_convar} is set to true.");

            return;
        }

        dump(argument);
    };
}

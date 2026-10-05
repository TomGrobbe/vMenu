namespace vMenu.Enhanced.Menus.Bully;

public static class BullyState
{
    private static readonly HashSet<string> Personal = new(StringComparer.Ordinal);

    private static readonly HashSet<string> ServerWide = new(StringComparer.Ordinal);

    public static event Action? Changed;

    public static event Action? Stopped;

    public static int Generation { get; private set; }

    public static bool IsOn(string toggle) => Personal.Contains(toggle) || ServerWide.Contains(toggle);

    public static void Set(string toggle, bool on, bool serverWide)
    {
        var set = serverWide ? ServerWide : Personal;
        var changed = on ? set.Add(toggle) : set.Remove(toggle);

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    public static void StopAll()
    {
        Generation++;

        Personal.Clear();
        ServerWide.Clear();

        Stopped?.Invoke();
        Changed?.Invoke();
    }
}

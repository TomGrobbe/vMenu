using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Menus.Bully;

internal static class BullyTask
{
    public static async void Run(Func<Task> work, string name)
    {
        try
        {
            await work();
        }
        catch (Exception exception)
        {
            Log.Error($"[Bully] {name} failed: {exception}");
        }
    }
}

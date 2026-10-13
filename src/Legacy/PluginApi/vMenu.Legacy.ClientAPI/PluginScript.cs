using System.Globalization;

using CitizenFX.Core;

namespace vMenu.Enhanced.ClientAPI;

internal sealed class PluginScript : BaseScript
{
    private static readonly List<KeyValuePair<string, Delegate>> Pending = [];

    private static PluginScript? _instance;

    public PluginScript()
    {
        _instance = this;

        foreach (var pending in Pending)
        {
            EventHandlers[pending.Key] += pending.Value;
        }

        Pending.Clear();
    }

    internal static void On(string eventName, Action<string> handler) =>
        Add(eventName, new Action<string, object>(([FromSource] string source, object value) =>
        {
            if (!IsFromNetwork(source))
            {
                handler(value?.ToString() ?? string.Empty);
            }
        }));

    internal static void On(string eventName, Action<int> handler) =>
        Add(eventName, new Action<string, object>(([FromSource] string source, object value) =>
        {
            if (!IsFromNetwork(source))
            {
                handler(Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
        }));

    private static bool IsFromNetwork(string? source) => source is not null && source.StartsWith("net:", StringComparison.Ordinal);

    private static void Add(string eventName, Delegate handler)
    {
        if (_instance is null)
        {
            Pending.Add(new KeyValuePair<string, Delegate>(eventName, handler));

            return;
        }

        _instance.EventHandlers[eventName] += handler;
    }
}

using vMenu.Enhanced.Configuration.Server;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

internal sealed class WebhookQueue
{
    private readonly List<WebhookEntry> _entries = [];

    private int _dropped;

    public int Count => _entries.Count;

    public bool IsEmpty => _entries.Count == 0;

    public int Dropped => _dropped;

    public void Add(WebhookEntry entry)
    {
        _entries.Add(entry);

        var limit = Math.Max(1, ServerConfig.Value(LoggingSettings.QueueLimit));

        if (_entries.Count <= limit)
        {
            return;
        }

        var excess = _entries.Count - limit;

        _entries.RemoveRange(0, excess);
        _dropped += excess;
    }

    public WebhookEntry At(int index) => _entries[index];

    public List<WebhookEntry> Take(int count)
    {
        var taken = _entries.GetRange(0, count);

        _entries.RemoveRange(0, count);

        return taken;
    }

    public void PutBack(List<WebhookEntry> entries) => _entries.InsertRange(0, entries);

    public void Forgive(int count) => _dropped = Math.Max(0, _dropped - count);

    public void Clear()
    {
        _entries.Clear();

        _dropped = 0;
    }
}

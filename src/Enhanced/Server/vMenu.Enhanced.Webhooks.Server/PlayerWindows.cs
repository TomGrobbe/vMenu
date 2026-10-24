namespace vMenu.Enhanced.Webhooks.Server;

/// <summary>A sliding window of timestamps per player, the shape every rate limit and report throttle shares. Times
/// are milliseconds from one clock, passed in, so this needs no native and is the same in every flavor.</summary>
public sealed class PlayerWindows
{
    private readonly Dictionary<int, Window> _byPlayer = [];

    /// <summary>The player's window, after dropping every stamp older than <paramref name="windowMs"/> and every
    /// player left with nothing to remember.</summary>
    public Window For(int serverId, int now, int windowMs)
    {
        Sweep(now, windowMs);

        if (!_byPlayer.TryGetValue(serverId, out var window))
        {
            window = new Window();

            _byPlayer[serverId] = window;
        }

        return window;
    }

    public Window? Find(int serverId) => _byPlayer.TryGetValue(serverId, out var window) ? window : null;

    public void Clear() => _byPlayer.Clear();

    public void Forget(int serverId) => _byPlayer.Remove(serverId);

    private void Sweep(int now, int windowMs)
    {
        List<int>? finished = null;

        foreach (var pair in _byPlayer)
        {
            var window = pair.Value;

            window.Trim(now, windowMs);

            if (window.Count == 0 && window.Suppressed == 0)
            {
                finished ??= [];

                finished.Add(pair.Key);
            }
        }

        if (finished is null)
        {
            return;
        }

        foreach (var serverId in finished)
        {
            _byPlayer.Remove(serverId);
        }
    }

    /// <summary>One player's stamps inside the window, plus what the limit built on it has already said.</summary>
    public sealed class Window
    {
        private readonly List<int> _stamps = [];

        private bool _reported;

        private int _reportedAt;

        public int Count => _stamps.Count;

        /// <summary>Only meaningful while <see cref="Count"/> is above zero.</summary>
        public int Oldest => _stamps[0];

        /// <summary>Calls refused without a mention. A window holding any is kept by the sweep, or the count of what
        /// was left out would go missing along with it.</summary>
        public int Suppressed { get; set; }

        public void Add(int now) => _stamps.Add(now);

        /// <summary>True at most once per window, so a player over a limit is reported once rather than on every
        /// refusal.</summary>
        public bool FirstReportIn(int now, int windowMs)
        {
            if (_reported && now - _reportedAt < windowMs)
            {
                return false;
            }

            _reported = true;
            _reportedAt = now;

            return true;
        }

        internal void Trim(int now, int windowMs)
        {
            while (_stamps.Count > 0 && now - _stamps[0] >= windowMs)
            {
                _stamps.RemoveAt(0);
            }
        }
    }
}

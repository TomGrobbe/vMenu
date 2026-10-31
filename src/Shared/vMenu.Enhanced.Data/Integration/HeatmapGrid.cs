namespace vMenu.Enhanced.Data.Integration;

public sealed class HeatmapBatch
{
    public required long Minute { get; init; }

    public required int Cell { get; init; }

    public required int Samples { get; init; }

    public required int[] Cells { get; init; }
}

public sealed class HeatmapGrid(int cellSize)
{
    private readonly Dictionary<long, int> _counts = [];

    private int _samples;

    public bool IsEmpty => _counts.Count == 0;

    public void BeginSample() => _samples++;

    public void Add(float x, float y)
    {
        var key = Pack(Index(x), Index(y));
        _counts[key] = _counts.TryGetValue(key, out var count) ? count + 1 : 1;
    }

    public HeatmapBatch Drain(long minute)
    {
        var cells = new int[_counts.Count * 3];
        var i = 0;

        foreach (var pair in _counts)
        {
            cells[i++] = (int)(pair.Key >> 32);
            cells[i++] = (int)pair.Key;
            cells[i++] = pair.Value;
        }

        var batch = new HeatmapBatch
        {
            Minute = minute,
            Cell = cellSize,
            Samples = _samples,
            Cells = cells,
        };

        Clear();

        return batch;
    }

    public void Clear()
    {
        _counts.Clear();
        _samples = 0;
    }

    private int Index(float coordinate) => (int)Math.Floor(coordinate / cellSize);

    private static long Pack(int x, int y) => ((long)x << 32) | (uint)y;
}

using System.Globalization;
using System.Text;

namespace vMenu.Enhanced.Data.VehicleData;

public sealed class PersonalVehicleEntry(int networkId, float x, float y, float z, int heading, uint model, bool inRange, string occupants)
{
    public int NetworkId { get; } = networkId;

    public float X { get; } = x;

    public float Y { get; } = y;

    public float Z { get; } = z;

    public int Heading { get; } = heading;

    public uint Model { get; } = model;

    public bool InRange { get; } = inRange;

    public string Occupants { get; } = occupants;
}

public static class PersonalVehicleRow
{
    public const char FieldSeparator = (char)31;

    public const char OccupantSeparator = (char)30;

    private const int FieldCount = 8;

    public static string Format(
        int networkId,
        float x,
        float y,
        float z,
        int heading,
        uint model,
        bool inRange,
        IReadOnlyList<string> occupants)
    {
        var row = new StringBuilder();

        row.Append(networkId.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(Round(x)).Append(FieldSeparator)
            .Append(Round(y)).Append(FieldSeparator)
            .Append(Round(z)).Append(FieldSeparator)
            .Append(heading.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(model.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(inRange ? '1' : '0').Append(FieldSeparator);

        for (var index = 0; index < occupants.Count; index++)
        {
            if (index > 0)
            {
                row.Append(OccupantSeparator);
            }

            row.Append(Clean(occupants[index]));
        }

        return row.ToString();
    }

    public static PersonalVehicleEntry? Parse(string? row)
    {
        if (string.IsNullOrEmpty(row))
        {
            return null;
        }

        var fields = row.Split(FieldSeparator);

        if (fields.Length != FieldCount)
        {
            return null;
        }

        if (!int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId)
            || !int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
            || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
            || !int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z)
            || !int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var heading)
            || !uint.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var model))
        {
            return null;
        }

        return new PersonalVehicleEntry(networkId, x, y, z, heading, model, fields[6] == "1", fields[7]);
    }

    public static List<string> Occupants(string? packed)
    {
        var names = new List<string>();

        if (string.IsNullOrEmpty(packed))
        {
            return names;
        }

        foreach (var name in packed.Split(OccupantSeparator))
        {
            if (!string.IsNullOrEmpty(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

    private static string Round(float value) =>
        ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static string Clean(string? name) =>
        string.IsNullOrEmpty(name)
            ? string.Empty
            : name.Replace(FieldSeparator, ' ').Replace(OccupantSeparator, ' ');
}

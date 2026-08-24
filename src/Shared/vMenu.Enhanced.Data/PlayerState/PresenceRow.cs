using System.Globalization;
using System.Text;

namespace vMenu.Enhanced.Data.PlayerState;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class PresenceEntry(int serverId, float x, float y, float z, int heading, uint vehicleModel, int flags, string name)
{
    public int ServerId { get; } = serverId;

    // Sent every time rather than once and cached. Caching it would mean the server remembering which of
    // its clients it had already told about which player, a table the size of the player count squared,
    // to save a dozen bytes on a message that is already being sent.
    public string Name { get; } = name;

    public float X { get; } = x;

    public float Y { get; } = y;

    public float Z { get; } = z;

    // Whole degrees, which is all a blip's rotation can show anyway.
    public int Heading { get; } = heading;

    // A model rather than a blip sprite, because working out the sprite needs IsThisModelAPlane and
    // friends, which are client natives the server does not have. Sending the model means the client runs
    // the same lookup for a streamed player and a remote one, so the two cannot disagree.
    public uint VehicleModel { get; } = vehicleModel;

    public int Flags { get; } = flags;

    public bool IsNoClipping => (Flags & PresenceRow.FlagNoClip) != 0;

    public bool IsDead => (Flags & PresenceRow.FlagDead) != 0;

    // Sent in the row rather than left to the player's state bag, which was the original design and did
    // not survive contact with OneSync. The game only replicates a bag to clients that have that player
    // in scope, so the marking came off their blip the moment they left streaming range. This is a
    // snapshot of players you cannot see, so it must never depend on being able to see them.
    public bool IsStaff => (Flags & PresenceRow.FlagStaff) != 0;
}

// One string carries the whole snapshot rather than a string per player, because the payload is sent
// to every subscribed client several times a second and the per-message overhead would dominate it.
public static class PresenceRow
{
    // ASCII 31, "unit separator", between the fields of one player.
    public const char FieldSeparator = (char)31;

    // ASCII 30, "record separator", between players.
    public const char RecordSeparator = (char)30;

    public const int FlagNoClip = 1;

    public const int FlagDead = 2;

    public const int FlagInVehicle = 4;

    public const int FlagStaff = 8;

    private const int FieldCount = 8;

    // Coordinates are rounded to whole metres. A blip is a dot on a map thousands of metres across, so a
    // metre is already more precision than anybody can see, and dropping the decimals halves the payload.
    public static void Append(
        StringBuilder snapshot,
        int serverId,
        float x,
        float y,
        float z,
        int heading,
        uint vehicleModel,
        int flags,
        string name)
    {
        if (snapshot.Length > 0)
        {
            snapshot.Append(RecordSeparator);
        }

        snapshot.Append(serverId.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(Round(x)).Append(FieldSeparator)
            .Append(Round(y)).Append(FieldSeparator)
            .Append(Round(z)).Append(FieldSeparator)
            .Append(heading.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(vehicleModel.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)
            .Append(flags.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator)

            // Last, and stripped of both separators first. Everything before it is a number, so this is the one
            // field that could otherwise carry a character that breaks the split.
            .Append(Clean(name));
    }

    public static List<PresenceEntry> Parse(string? snapshot)
    {
        var entries = new List<PresenceEntry>();

        if (string.IsNullOrEmpty(snapshot))
        {
            return entries;
        }

        foreach (var row in snapshot.Split(RecordSeparator))
        {
            if (TryParseRow(row, out var entry))
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static bool TryParseRow(string row, out PresenceEntry entry)
    {
        entry = null!;

        var fields = row.Split(FieldSeparator);

        if (fields.Length != FieldCount)
        {
            return false;
        }

        if (!int.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId)
            || !int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
            || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
            || !int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z)
            || !int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var heading)
            || !uint.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var model)
            || !int.TryParse(fields[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out var flags))
        {
            return false;
        }

        entry = new PresenceEntry(serverId, x, y, z, heading, model, flags, fields[7]);

        return true;
    }

    private static string Round(float value) =>
        ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static string Clean(string? name) =>
        string.IsNullOrEmpty(name)
            ? string.Empty
            : name.Replace(FieldSeparator, ' ').Replace(RecordSeparator, ' ');
}

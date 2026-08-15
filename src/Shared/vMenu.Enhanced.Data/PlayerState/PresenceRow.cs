using System.Globalization;
using System.Text;

namespace vMenu.Enhanced.Data.PlayerState;

/// <summary>What the server last knew about one player, as it travels over the wire.</summary>
// A plain class rather than a record, matching the rest of this codebase: the generated equality
// routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class PresenceEntry(int serverId, float x, float y, float z, int heading, uint vehicleModel, int flags, string name)
{
    public int ServerId { get; } = serverId;

    /// <summary>Their name, so a blip drawn for somebody you cannot see is not an anonymous dot.</summary>
    // Sent every time rather than once and cached. Caching it would mean the server remembering
    // which of its clients it had already told about which player, which is a table the size of the
    // player count squared, to save a dozen bytes on a message that is already being sent.
    public string Name { get; } = name;

    public float X { get; } = x;

    public float Y { get; } = y;

    public float Z { get; } = z;

    /// <summary>Whole degrees, which is all a blip's rotation can show anyway.</summary>
    public int Heading { get; } = heading;

    /// <summary>The model they are sitting in, or zero on foot.</summary>
    // A model rather than a blip sprite, because working out the sprite needs IsThisModelAPlane and
    // friends, which are client natives the server does not have. Sending the model means the client
    // runs the same lookup for a streamed player and a remote one, so the two cannot disagree.
    public uint VehicleModel { get; } = vehicleModel;

    public int Flags { get; } = flags;

    public bool IsNoClipping => (Flags & PresenceRow.FlagNoClip) != 0;

    public bool IsDead => (Flags & PresenceRow.FlagDead) != 0;

    /// <summary>Whether this player holds the staff permission.</summary>
    // Sent in the row rather than left to the player's state bag, which was the original design and
    // did not survive contact with OneSync. The game only replicates a player's bag to clients that
    // have that player in scope, so the moment somebody walked out of streaming range their staff
    // key disappeared from everybody else's copy and the marking came off their blip. This is a
    // snapshot of players you cannot see, so the one thing it must never depend on is being able to
    // see them.
    public bool IsStaff => (Flags & PresenceRow.FlagStaff) != 0;
}

/// <summary>
/// Packing for the presence snapshot, so both ends agree on the format.
/// </summary>
/// <remarks>
/// One string carries the whole snapshot rather than a string per player, because the payload is
/// sent to every subscribed client several times a second and the per-message overhead would
/// dominate it.
/// </remarks>
public static class PresenceRow
{
    /// <summary>ASCII 31, "unit separator", between the fields of one player.</summary>
    public const char FieldSeparator = (char)31;

    /// <summary>ASCII 30, "record separator", between players.</summary>
    public const char RecordSeparator = (char)30;

    public const int FlagNoClip = 1;

    public const int FlagDead = 2;

    public const int FlagInVehicle = 4;

    public const int FlagStaff = 8;

    private const int FieldCount = 8;

    /// <summary>
    /// Appends one player to a snapshot being built.
    /// </summary>
    /// <remarks>
    /// Coordinates are rounded to whole metres. A blip is a dot on a map that is thousands of metres
    /// across, so a metre is already far more precision than anybody can see, and dropping the
    /// decimals roughly halves what goes over the wire.
    /// </remarks>
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

            // Last, and stripped of both separators first. Everything before it is a number, so this
            // is the one field that could otherwise carry a character that breaks the split.
            .Append(Clean(name));
    }

    /// <summary>Reads a whole snapshot back, skipping anything malformed rather than throwing.</summary>
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

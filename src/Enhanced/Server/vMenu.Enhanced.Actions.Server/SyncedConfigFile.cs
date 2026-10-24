using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>An owner edited config file every client holds a copy of. Clients are told rather than asked, so they
/// never fetch it again, and any change here goes back out to everybody, which keeps their copies from
/// drifting.</summary>
public sealed class SyncedConfigFile<T>
    where T : class, new()
{
    private readonly string _file;

    private readonly string _tag;

    private readonly string _setEvent;

    private readonly string? _permission;

    private readonly string _unwritten;

    private volatile string _payload;

    /// <param name="permission">Null sends the file to everybody.</param>
    /// <param name="unwritten">What a failed write means for the player, for the log.</param>
    public SyncedConfigFile(string file, string tag, string setEvent, string? permission, string unwritten)
    {
        _file = file;
        _tag = tag;
        _setEvent = setEvent;
        _permission = permission;
        _unwritten = unwritten;
        _payload = ServerJson.Serialize(Value);
    }

    public T Value { get; } = new();

    /// <summary>What the clients receive, rebuilt only when the file changes.</summary>
    public string Payload => _payload;

    /// <summary>Call after filling <see cref="Value"/> from the file.</summary>
    public void Rebuild() => _payload = ServerJson.Serialize(Value);

    /// <summary>Writes the file, then republishes it. Runs <paramref name="undo"/> if the write failed, so what the
    /// clients hold always matches what is on disk.</summary>
    public ActionResponse Commit(Player source, string what, Action undo)
    {
        if (!ResourceFile.TryWrite(_file, _tag, Value, _unwritten))
        {
            undo();

            return ActionResponse.Failed();
        }

        Rebuild();

        foreach (var connected in ConnectedPlayers.All())
        {
            SendTo(new Player(connected.ServerId));
        }

        Log.Debug($"[{_tag}] {source} {what}.");

        return ActionResponse.Ok();
    }

    /// <summary>Gated on the way out as well as on the way in: a player who may not use the file has no business
    /// holding it.</summary>
    public void SendTo(Player player)
    {
        if (_permission is null || ServerPermissions.IsPlayerAllowed(player, _permission))
        {
            API.EmitClient(player.Handle, _setEvent, _payload);
        }
    }
}

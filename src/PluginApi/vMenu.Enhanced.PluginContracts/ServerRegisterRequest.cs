namespace vMenu.Enhanced.PluginContracts;

public class PermissionDeclaration
{
    /// <summary>Short name, vMenu composes <c>vMenu.Enhanced.Plugins.&lt;Id&gt;.&lt;Name&gt;</c> from it.</summary>
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Marks the permission as staff only in the generated permissions example.</summary>
    public bool StaffOnly { get; set; }
}

/// <summary>The server side registration payload: the permission names and convar settings a plugin
/// brings. Sent once at plugin startup, and again whenever vMenu announces it restarted.
/// Registration is idempotent.</summary>
public class ServerRegisterRequest
{
    public int ProtocolVersion { get; set; } = PluginProtocol.Version;

    /// <summary>Used in the generated example files so owners see which plugin a section belongs to.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public List<PermissionDeclaration>? Permissions { get; set; }

    public List<SettingNode>? Settings { get; set; }
}

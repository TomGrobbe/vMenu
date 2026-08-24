namespace vMenu.Enhanced.PluginContracts;

/// <summary>vMenu's answer to a registration, on either side. Errors mean the registration was
/// refused, warnings mean it was accepted with parts skipped.</summary>
public class RegisterResult
{
    public bool Accepted { get; set; }

    public int ProtocolVersion { get; set; } = PluginProtocol.Version;

    public List<string> Errors { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
}

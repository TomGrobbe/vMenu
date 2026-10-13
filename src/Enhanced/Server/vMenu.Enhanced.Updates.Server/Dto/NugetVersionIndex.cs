using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class NugetVersionIndex
{
    [JsonPropertyName("versions")]
    public string[]? Versions { get; set; }
}

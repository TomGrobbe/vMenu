using Newtonsoft.Json;

namespace vMenu.Enhanced.Updates.Server.Dto;

internal sealed class NugetVersionIndex
{
    [JsonProperty("versions")]
    public string[]? Versions { get; set; }
}

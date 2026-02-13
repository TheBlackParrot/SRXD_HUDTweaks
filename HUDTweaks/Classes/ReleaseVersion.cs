using Newtonsoft.Json;

namespace HUDTweaks.Classes;

internal class ReleaseVersion
{
    [JsonProperty("tag_name")] public string? Version { get; set; }
    [JsonProperty("prerelease")] public bool IsPreRelease { get; set; }
    
    [JsonConstructor]
    public ReleaseVersion() {}
}
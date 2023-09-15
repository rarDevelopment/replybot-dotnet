using System.Text.Json.Serialization;

namespace Replybot.Models;

public class CountryConfig
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("triggerNames")]
    public List<string>? TriggerNames { get; set; }
    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }
    [JsonPropertyName("urlSearchWord")]
    public string? UrlSearchWord { get; set; }
}
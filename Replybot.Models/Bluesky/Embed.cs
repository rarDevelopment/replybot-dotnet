using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Embed
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }

    [JsonPropertyName("media")]
    public Media? Media { get; set; }
}
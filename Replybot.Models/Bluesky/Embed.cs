using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Embed
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }

    [JsonPropertyName("media")]
    public Media? Media { get; set; }

    [JsonPropertyName("images")]
    public List<Image>? Images { get; set; }

    [JsonPropertyName("video")]
    public Video Video { get; set; }

    [JsonPropertyName("record")]
    public QuotedRecord? Record { get; set; }
}
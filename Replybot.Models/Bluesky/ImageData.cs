using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class ImageData
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }

    [JsonPropertyName("ref")]
    public Ref Ref { get; set; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Video
{
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("ref")]
    public Ref Ref { get; set; }
}
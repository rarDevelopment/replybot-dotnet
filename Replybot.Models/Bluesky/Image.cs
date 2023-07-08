using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Image
{
    [JsonPropertyName("alt")]
    public string Alt { get; set; }

    [JsonPropertyName("image")]
    public ImageData ImageData { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Media
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }
    [JsonPropertyName("images")]
    public List<Image> Images { get; set; }
}
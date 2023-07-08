using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Value
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("$type")]
    public string Type { get; set; }

    [JsonPropertyName("embed")]
    public Embed? Embed { get; set; }

    [JsonPropertyName("langs")]
    public List<string> Langs { get; } = new List<string>();

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class Ref
{
    [JsonPropertyName("$link")]
    public string Link { get; set; }
}
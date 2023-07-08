using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class BlueskyRecord
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("cid")]
    public string Cid { get; set; }

    [JsonPropertyName("value")]
    public Value Value { get; set; }
}
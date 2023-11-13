using System.Text.Json.Serialization;

namespace Replybot.Models.Bluesky;

public class QuotedRecord
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("cid")]
    public string Cid { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatApiSearchInfo
{
    [JsonPropertyName("hltbUrlPath")]
    public string? UrlPath { get; set; }
}

using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatApiSearchInfo
{
    [JsonPropertyName("hltbKey")]
    public string? ApiSearchKey { get; set; }
    [JsonPropertyName("hltbUrlPath")]
    public string? UrlPath { get; set; }
    public DateTime DateUpdated { get; set; }
}
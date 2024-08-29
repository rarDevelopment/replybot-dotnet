using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatApiSearchInfo
{
    [JsonPropertyName("hltbKey")]
    public string? Key { get; set; }
    public DateTime DateUpdated { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsGamesRangeTime
{
    [JsonPropertyName("min")]
    public int Min { get; set; }
    [JsonPropertyName("max")]
    public int Max { get; set; }
}
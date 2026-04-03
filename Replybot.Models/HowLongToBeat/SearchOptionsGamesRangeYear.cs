using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsGamesRangeYear
{
    [JsonPropertyName("min")]
    public string Min { get; set; } = "";
    [JsonPropertyName("max")]
    public string Max { get; set; } = "";
}

using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsGames
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    [JsonPropertyName("platform")]
    public string Platform { get; set; }
    [JsonPropertyName("sortCategory")]
    public string SortCategory { get; set; }
    [JsonPropertyName("rangeCategory")]
    public string RangeCategory { get; set; }
    [JsonPropertyName("rangeTime")]
    public SearchOptionsGamesRangeTime RangeTime { get; set; }
    [JsonPropertyName("gameplay")]
    public SearchOptionsGamesGameplay Gameplay { get; set; }
    [JsonPropertyName("modifier")]
    public string Modifier { get; set; }
}
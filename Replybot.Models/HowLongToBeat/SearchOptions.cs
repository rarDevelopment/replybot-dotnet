using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptions
{
    [JsonPropertyName("games")]
    public SearchOptionsGames Games { get; set; }
    [JsonPropertyName("users")]
    public SearchOptionsUsers Users { get; set; }
    [JsonPropertyName("filter")]
    public string Filter { get; set; }
    [JsonPropertyName("sort")]
    public int Sort { get; set; }
    [JsonPropertyName("randomizer")]
    public int Randomizer { get; set; }
    [JsonPropertyName("lists")]
    public SearchOptionsLists Lists { get; set; }
}
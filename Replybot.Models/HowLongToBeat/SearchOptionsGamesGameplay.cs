using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsGamesGameplay
{
    [JsonPropertyName("perspective")]
    public string Perspective { get; set; }
    [JsonPropertyName("flow")]
    public string Flow { get; set; }
    [JsonPropertyName("genre")]
    public string Genre { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsUsers
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("sortCategory")]
    public string SortCategory { get; set; }
}
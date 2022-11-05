using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class SearchOptionsUsers
{
    [JsonPropertyName("sortCategory")]
    public string SortCategory { get; set; }
}
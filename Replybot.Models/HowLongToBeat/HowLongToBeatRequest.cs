using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatRequest
{
    [JsonPropertyName("searchType")]
    public string SearchType { get; set; }
    [JsonPropertyName("searchTerms")]
    public string[] SearchTerms { get; set; }
    [JsonPropertyName("searchPage")]
    public int SearchPage { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("searchOptions")]
    public SearchOptions SearchOptions { get; set; }
}
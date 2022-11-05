using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatResponse
{
    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("pageCurrent")]
    public int PageCurrent { get; set; }

    [JsonPropertyName("pageTotal")]
    public int? PageTotal { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("data")]
    public List<HowLongToBeatResponseGameData> Data { get; set; }

    [JsonPropertyName("displayModifier")]
    public object? DisplayModifier { get; set; }
}


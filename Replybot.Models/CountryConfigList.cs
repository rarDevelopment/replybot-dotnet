using System.Text.Json.Serialization;

namespace Replybot.Models;

public class CountryConfigList
{
    [JsonPropertyName("countries")]
    public IReadOnlyList<CountryConfig>? Countries { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.FreeDictionary;

public class Definition
{
    [JsonPropertyName("definition")]
    public string DefinitionText { get; set; }

    [JsonPropertyName("example")]
    public string Example { get; set; }

    [JsonPropertyName("synonyms")]
    public List<object> Synonyms { get; set; }

    [JsonPropertyName("antonyms")]
    public List<object> Antonyms { get; set; }
}
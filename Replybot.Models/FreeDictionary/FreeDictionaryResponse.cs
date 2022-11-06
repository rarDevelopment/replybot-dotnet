using System.Text.Json.Serialization;

namespace Replybot.Models.FreeDictionary;

public class FreeDictionaryResponse
{
    [JsonPropertyName("word")]
    public string Word { get; set; }

    [JsonPropertyName("phonetic")]
    public string Phonetic { get; set; }

    [JsonPropertyName("phonetics")]
    public List<Phonetic> Phonetics { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    [JsonPropertyName("meanings")]
    public List<Meaning> Meanings { get; set; }
}
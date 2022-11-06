using System.Text.Json.Serialization;

namespace Replybot.Models.FreeDictionary;

public class Phonetic
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("audio")]
    public string Audio { get; set; }
}
using System.Text.Json.Serialization;

namespace Replybot.Models.HowLongToBeat;

public class HowLongToBeatAuthResponse
{
    [JsonPropertyName("token")] 
    public string? Token { get; set; }
    [JsonPropertyName("hpKey")]
    public string? HpKey { get; set; }
    [JsonPropertyName("hpVal")]
    public string? HpVal { get; set; }
}

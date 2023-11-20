namespace Replybot.Models;

public class DictionarySettings(string baseUrl)
{
    public string BaseUrl { get; set; } = baseUrl;
}
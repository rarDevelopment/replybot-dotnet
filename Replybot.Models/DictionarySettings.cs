namespace Replybot.Models;

public class DictionarySettings
{
    public DictionarySettings(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; set; }
}
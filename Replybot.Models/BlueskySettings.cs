namespace Replybot.Models;

public class BlueskySettings
{
    public BlueskySettings(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; set; }
}
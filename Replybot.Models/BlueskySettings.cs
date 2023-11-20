namespace Replybot.Models;

public class BlueskySettings(string baseUrl)
{
    public string BaseUrl { get; set; } = baseUrl;
}
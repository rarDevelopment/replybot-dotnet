namespace Replybot.Models;

public class WebsiteApiSettings(string baseUrl)
{
    public string BaseUrl { get; } = baseUrl;
}
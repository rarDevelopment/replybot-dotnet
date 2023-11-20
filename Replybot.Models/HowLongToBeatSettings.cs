namespace Replybot.Models;

public class HowLongToBeatSettings(string baseUrl, string referer)
{
    public string BaseUrl { get; set; } = baseUrl;
    public string Referer { get; set; } = referer;
}
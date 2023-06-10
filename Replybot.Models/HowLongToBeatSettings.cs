namespace Replybot.Models;

public class HowLongToBeatSettings
{
    public HowLongToBeatSettings(string baseUrl, string referer)
    {
        BaseUrl = baseUrl;
        Referer = referer;
    }

    public string BaseUrl { get; set; }
    public string Referer { get; set; }
}
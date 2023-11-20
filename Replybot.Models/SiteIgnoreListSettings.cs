namespace Replybot.Models;

public class SiteIgnoreListSettings(string url)
{
    public string Url { get; } = url;
}
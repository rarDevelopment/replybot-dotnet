namespace Replybot.Models;

public class SiteIgnoreListSettings
{
    public string Url { get; }

    public SiteIgnoreListSettings(string url)
    {
        Url = url;
    }
}
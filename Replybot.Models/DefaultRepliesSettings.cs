namespace Replybot.Models;

public class DefaultRepliesSettings
{
    public string Url { get; }

    public DefaultRepliesSettings(string url)
    {
        Url = url;
    }
}
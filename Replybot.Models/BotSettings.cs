namespace Replybot.Models;

public class BotSettings
{
    public BotSettings(int regexTimeoutMilliseconds)
    {
        RegexTimeoutMilliseconds = regexTimeoutMilliseconds;
    }

    public int RegexTimeoutMilliseconds { get; set; }
}
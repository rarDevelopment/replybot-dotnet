namespace Replybot.Models;

public class BotSettings
{
    public BotSettings(int regexTimeoutTicks)
    {
        RegexTimeoutTicks = regexTimeoutTicks;
    }

    public int RegexTimeoutTicks { get; set; }
}
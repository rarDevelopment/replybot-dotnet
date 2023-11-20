namespace Replybot.Models;

public class BotSettings(int regexTimeoutTicks)
{
    public int RegexTimeoutTicks { get; set; } = regexTimeoutTicks;
}
namespace Replybot.TextCommands.Models;

public class TextCommandReplyCriteria(string messageText)
{
    public string MessageText { get; set; } = messageText;
    public bool IsBotNameMentioned { get; set; }
}
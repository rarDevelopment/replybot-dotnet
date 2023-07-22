namespace Replybot.TextCommands.Models;

public class TextCommandReplyCriteria
{
    public TextCommandReplyCriteria(string messageText)
    {
        MessageText = messageText;
    }

    public string MessageText { get; set; }
    public bool IsBotNameMentioned { get; set; }
}
namespace Replybot.Models;

public class UserReply
{
    public UserReply(ulong userId, string[] replies)
    {
        UserId = userId;
        Replies = replies;
    }

    public ulong UserId { get; set; }
    public string[] Replies { get; set; }
}
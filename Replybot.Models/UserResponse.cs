namespace Replybot.Models;

public class UserResponse
{
    public UserResponse(ulong userId, string[] responses)
    {
        UserId = userId;
        Responses = responses;
    }

    public ulong UserId { get; set; }
    public string[] Responses { get; set; }
}
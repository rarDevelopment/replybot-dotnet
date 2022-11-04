namespace Replybot.Models;

public class PersonResponse
{
    public PersonResponse(ulong userId, string[] responses)
    {
        UserId = userId;
        Responses = responses;
    }

    public ulong UserId { get; set; }
    public string[] Responses { get; set; }
}
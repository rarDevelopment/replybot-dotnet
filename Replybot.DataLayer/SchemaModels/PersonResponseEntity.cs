using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels;

public class PersonResponseEntity
{
    public PersonResponseEntity(ulong userId, string[] personResponses)
    {
        UserId = userId;
        PersonResponses = personResponses;
    }

    public ulong UserId { get; set; }
    public string[] PersonResponses { get; set; }

    public PersonResponse ToDomain()
    {
        return new PersonResponse(UserId, PersonResponses);
    }
}
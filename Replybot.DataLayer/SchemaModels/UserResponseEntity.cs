using MongoDB.Bson.Serialization.Attributes;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels;

public class UserResponseEntity
{
    public UserResponseEntity(ulong userId, string[] responses)
    {
        UserId = userId;
        Responses = responses;
    }

    [BsonElement("userId")]
    public ulong UserId { get; set; }
    [BsonElement("responses")]
    public string[] Responses { get; set; }

    public UserResponse ToDomain()
    {
        return new UserResponse(UserId, Responses);
    }
}
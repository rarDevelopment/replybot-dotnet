using MongoDB.Bson.Serialization.Attributes;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels;

public class UserReplyEntity
{
    public UserReplyEntity(ulong userId, string[] replies)
    {
        UserId = userId;
        Replies = replies;
    }

    [BsonElement("userId")]
    public ulong UserId { get; set; }
    [BsonElement("replies")]
    public string[] Replies { get; set; }
    [BsonElement("reactions")]
    public string[]? Reactions { get; set; }

    public UserReply ToDomain()
    {
        return new UserReply(UserId, Replies);
    }
}
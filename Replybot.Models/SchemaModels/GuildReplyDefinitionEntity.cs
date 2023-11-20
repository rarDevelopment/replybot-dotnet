using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Replybot.Models.SchemaModels;

[BsonIgnoreExtraElements]
public class GuildReplyDefinitionEntity(ulong guildId, string[] triggers, decimal priority, string[]? replies = null,
    bool mentionAuthor = false, bool requiresBotName = false, string[]? reactions = null)
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("guildId")]
    public ulong GuildId { get; set; } = guildId;

    [BsonElement("triggers")]
    public string[] Triggers { get; set; } = triggers;

    [BsonElement("replies")]
    public string[]? Replies { get; set; } = replies;

    [BsonElement("mentionAuthor")]
    public bool MentionAuthor { get; set; } = mentionAuthor;

    [BsonElement("requiresBotName")]
    public bool RequiresBotName { get; set; } = requiresBotName;

    [BsonElement("reactions")]
    public string[]? Reactions { get; set; } = reactions;

    [BsonElement("userIds")]
    public string[]? UserIds { get; set; }
    [BsonElement("channelIds")]
    public string[]? ChannelIds { get; set; }
    [BsonElement("priority")]
    public decimal Priority { get; set; } = priority;

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    public GuildReplyDefinition ToDomain()
    {
        return new GuildReplyDefinition(GuildId,
            Triggers,
            Replies,
            ChannelIds,
            UserIds,
            MentionAuthor,
            RequiresBotName,
            Reactions,
            Priority,
            IsActive)
        {
            Id = Id
        };
    }
}
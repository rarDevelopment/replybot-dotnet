using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels;

public class GuildConfigurationEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("guildId")]
    public string GuildId { get; set; }
    [BsonElement("guildName")]
    public string? GuildName { get; set; }
    [BsonElement("enableAvatarAnnounce")]
    public bool EnableAvatarAnnouncements { get; set; }
    [BsonElement("enableAvatarMentions")]
    public bool EnableAvatarMentions { get; set; }
    [BsonElement("logChannelId")]
    public string? LogChannelId { get; set; }
    [BsonElement("adminUserIds")]
    public List<string> AdminUserIds { get; set; } = new();
    [BsonElement("enableFixTweetReactions")]
    public bool EnableFixTweetReactions { get; set; }
    [BsonElement("enableFixInstagramReactions")]
    public bool EnableFixInstagramReactions { get; set; }
    [BsonElement("enableDefaultReplies")]
    public bool EnableDefaultReplies { get; set; }

    public GuildConfiguration ToDomain()
    {
        return new GuildConfiguration
        {
            GuildId = GuildId,
            GuildName = GuildName,
            EnableAvatarAnnouncements = EnableAvatarAnnouncements,
            EnableAvatarMentions = EnableAvatarMentions,
            LogChannelId = LogChannelId,
            AdminUserIds = AdminUserIds,
            EnableDefaultReplies = EnableDefaultReplies,
            EnableFixTweetReactions = EnableFixTweetReactions,
            EnableFixInstagramReactions = EnableFixInstagramReactions
};
    }
}
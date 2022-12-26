using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels
{
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

        public ulong? LogChannelId { get; set; }

        public GuildConfiguration ToDomain()
        {
            return new GuildConfiguration
            {
                GuildId = (ulong)Convert.ToInt64(GuildId),
                GuildName = GuildName,
                EnableAvatarAnnouncements = EnableAvatarAnnouncements,
                EnableAvatarMentions = EnableAvatarMentions,
                LogChannelId = LogChannelId
            };
        }
    }
}

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
        public ulong GuildId { get; set; }
        [BsonElement("guildName")]
        public string? GuildName { get; set; }
        [BsonElement("enableAvatarAnnouncements")]
        public bool EnableAvatarAnnouncements { get; set; }
        [BsonElement("enableAvatarMentions")]
        public bool EnableAvatarMentions { get; set; }

        public GuildConfiguration ToDomain()
        {
            return new GuildConfiguration
            {
                GuildId = GuildId,
                GuildName = GuildName,
                EnableAvatarAnnouncements = EnableAvatarAnnouncements,
                EnableAvatarMentions = EnableAvatarMentions
            };
        }
    }
}

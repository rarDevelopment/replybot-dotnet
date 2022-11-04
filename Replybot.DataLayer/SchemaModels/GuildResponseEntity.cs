using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels
{
    public class GuildResponseEntity
    {
        public GuildResponseEntity(ulong guildId, TriggerResponseEntity[] responses)
        {
            GuildId = guildId;
            Responses = responses;
        }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("guildId")]
        public ulong GuildId { get; set; }
        [BsonElement("responses")]
        public TriggerResponseEntity[] Responses { get; set; }

        public GuildResponse ToDomain()
        {
            return new GuildResponse(GuildId, Responses.Select(r => r.ToDomain()).ToArray());
        }
    }
}

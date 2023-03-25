using MongoDB.Bson.Serialization.Attributes;
using Replybot.Models;

namespace Replybot.DataLayer.SchemaModels;

public class TriggerResponseEntity
{
    public TriggerResponseEntity(string[] triggers, string[]? responses = null, bool mentionAuthor = false, bool requiresBotName = false, string[]? reactions = null)
    {
        Triggers = triggers;
        Responses = responses;
        MentionAuthor = mentionAuthor;
        RequiresBotName = requiresBotName;
        Reactions = reactions;
    }

    [BsonElement("triggers")]
    public string[] Triggers { get; set; }
    [BsonElement("responses")]
    public string[]? Responses { get; set; }
    [BsonElement("userResponses")]
    public UserResponseEntity[]? UserResponses { get; set; }
    [BsonElement("mentionAuthor")]
    public bool MentionAuthor { get; set; }
    [BsonElement("requiresBotName")]
    public bool RequiresBotName { get; set; }
    [BsonElement("reactions")]
    public string[]? Reactions { get; set; }

    public TriggerResponse ToDomain()
    {
        var peopleResponses = UserResponses?.Select(p => p.ToDomain()).ToArray();
        return new TriggerResponse(Triggers, Responses, peopleResponses, MentionAuthor, RequiresBotName, Reactions);
    }
}
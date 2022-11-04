using System.Text.Json;
using MongoDB.Driver;
using Replybot.DataLayer.SchemaModels;
using Replybot.Models;

namespace Replybot.DataLayer;

public class ResponseDataLayer : IResponseDataLayer
{
    private readonly IMongoCollection<GuildResponseEntity>? _guildResponsesCollection;

    public ResponseDataLayer(DatabaseSettings databaseSettings)
    {
        var connectionString = $"mongodb+srv://{databaseSettings.User}:{databaseSettings.Password}@{databaseSettings.Cluster}.mongodb.net/{databaseSettings.Name}?w=majority";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseSettings.Name);
        _guildResponsesCollection = database.GetCollection<GuildResponseEntity>("responses");
    }

    public async Task<IList<TriggerResponse>> GetResponsesForGuild(ulong guildId)
    {
        var filter = Builders<GuildResponseEntity>.Filter.Eq("guildId", guildId.ToString());
        var guildResponses = await _guildResponsesCollection.Find(filter).FirstOrDefaultAsync();
        var triggerResponses = guildResponses.Responses;
        return triggerResponses.Select(tr => tr.ToDomain()).ToList();
    }
    
    public IList<TriggerResponse>? GetDefaultResponses()
    {
        var filePath = Path.GetFullPath("DefaultResponses.json");
        using var r = new StreamReader(filePath);
        var json = r.ReadToEnd();
        var defaultResponseData = JsonSerializer.Deserialize<DefaultResponseData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return defaultResponseData?.DefaultResponses.Select(tr => tr.ToDomain()).ToList();
    }
}
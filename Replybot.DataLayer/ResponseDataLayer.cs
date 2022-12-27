using System.Text.Json;
using MongoDB.Driver;
using Replybot.DataLayer.SchemaModels;
using Replybot.Models;

namespace Replybot.DataLayer;

public class ResponseDataLayer : IResponseDataLayer
{
    private readonly IMongoCollection<GuildResponseEntity>? _guildResponsesCollection;
    private readonly IMongoCollection<GuildConfigurationEntity> _guildConfigurationsCollection;

    public ResponseDataLayer(DatabaseSettings databaseSettings)
    {
        var connectionString = $"mongodb+srv://{databaseSettings.User}:{databaseSettings.Password}@{databaseSettings.Cluster}.mongodb.net/{databaseSettings.Name}?w=majority";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseSettings.Name);
        _guildResponsesCollection = database.GetCollection<GuildResponseEntity>("responses");
        _guildConfigurationsCollection = database.GetCollection<GuildConfigurationEntity>("configuration");
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

    public async Task<IList<TriggerResponse>?> GetResponsesForGuild(ulong guildId)
    {
        var filter = Builders<GuildResponseEntity>.Filter.Eq("guildId", guildId.ToString());
        var guildResponses = await _guildResponsesCollection.Find(filter).FirstOrDefaultAsync();
        var triggerResponses = guildResponses?.Responses;
        return triggerResponses?.Select(tr => tr.ToDomain()).ToList();
    }

    public async Task<GuildConfiguration> GetConfigurationForGuild(ulong guildId, string guildName)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var guildConfig = await _guildConfigurationsCollection.Find(filter).FirstOrDefaultAsync();
        if (guildConfig != null)
        {
            return guildConfig.ToDomain();
        }

        await InitGuildConfiguration(guildId, guildName);

        guildConfig = await _guildConfigurationsCollection.Find(filter).FirstOrDefaultAsync();
        return guildConfig.ToDomain();
    }

    private async Task InitGuildConfiguration(ulong guildId, string guildName)
    {
        await _guildConfigurationsCollection.InsertOneAsync(new GuildConfigurationEntity
        {
            GuildId = guildId.ToString(),
            GuildName = guildName,
            EnableAvatarAnnouncements = true,
            EnableAvatarMentions = true
        });
    }

    public async Task<bool> UpdateGuildConfiguration(ulong guildId, string guildName)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.GuildName, guildName);
        var updateResult = await _guildConfigurationsCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarAnnouncements(ulong guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarAnnouncements, isEnabled);
        var updateResult = await _guildConfigurationsCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarMentions(ulong guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarMentions, isEnabled);
        var updateResult = await _guildConfigurationsCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetLogChannel(ulong guildId, ulong? channelId)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.LogChannelId, channelId);
        var updateResult = await _guildConfigurationsCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }
}
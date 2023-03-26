using System.Text.Json;
using MongoDB.Driver;
using Replybot.DataLayer.SchemaModels;
using Replybot.Models;

namespace Replybot.DataLayer;

public class ReplyDataLayer : IReplyDataLayer
{
    private readonly IMongoCollection<GuildReplyDefinitionEntity>? _guildRepliesCollection;
    private readonly IMongoCollection<GuildConfigurationEntity> _guildConfigurationCollection;

    public ReplyDataLayer(DatabaseSettings databaseSettings)
    {
        var connectionString = $"mongodb+srv://{databaseSettings.User}:{databaseSettings.Password}@{databaseSettings.Cluster}.mongodb.net/{databaseSettings.Name}?w=majority";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseSettings.Name);
        _guildRepliesCollection = database.GetCollection<GuildReplyDefinitionEntity>("guildReplyDefinitions");
        _guildConfigurationCollection = database.GetCollection<GuildConfigurationEntity>("configuration");
    }
    public IList<GuildReplyDefinition>? GetDefaultReplies()
    {
        var filePath = Path.GetFullPath("DefaultReplies.json");
        using var r = new StreamReader(filePath);
        var json = r.ReadToEnd();
        var defaultReplyData = JsonSerializer.Deserialize<DefaultReplyData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return defaultReplyData?.DefaultReplies.Select(tr => tr.ToDomain()).ToList();
    }

    public async Task<IList<GuildReplyDefinition>?> GetRepliesForGuild(ulong guildId)
    {
        var filter = Builders<GuildReplyDefinitionEntity>.Filter.Eq("guildId", guildId.ToString());
        var guildReplyDefinitionEntities = await _guildRepliesCollection.Find(filter).ToListAsync();
        return guildReplyDefinitionEntities.Select(r => r.ToDomain()).ToList();
    }

    public async Task<GuildConfiguration> GetConfigurationForGuild(ulong guildId, string guildName)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var guildConfig = await _guildConfigurationCollection.Find(filter).FirstOrDefaultAsync();
        if (guildConfig != null)
        {
            return guildConfig.ToDomain();
        }

        await InitGuildConfiguration(guildId, guildName);

        guildConfig = await _guildConfigurationCollection.Find(filter).FirstOrDefaultAsync();
        return guildConfig.ToDomain();
    }

    private async Task InitGuildConfiguration(ulong guildId, string guildName)
    {
        await _guildConfigurationCollection.InsertOneAsync(new GuildConfigurationEntity
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
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarAnnouncements(ulong guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarAnnouncements, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarMentions(ulong guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarMentions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetLogChannel(ulong guildId, ulong? channelId)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId.ToString());
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.LogChannelId, channelId);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }
}
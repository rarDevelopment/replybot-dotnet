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

    public async Task<IList<GuildReplyDefinition>?> GetRepliesForGuild(string guildId)
    {
        var filter = Builders<GuildReplyDefinitionEntity>.Filter.Eq("guildId", guildId);
        var guildReplyDefinitionEntities = await _guildRepliesCollection.Find(filter).ToListAsync();
        return guildReplyDefinitionEntities.Select(r => r.ToDomain()).OrderBy(gr => gr.Priority).ToList();
    }

    public async Task<GuildConfiguration> GetConfigurationForGuild(string guildId, string guildName)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var guildConfig = await _guildConfigurationCollection.Find(filter).FirstOrDefaultAsync();
        if (guildConfig != null)
        {
            return guildConfig.ToDomain();
        }

        await InitGuildConfiguration(guildId, guildName);

        guildConfig = await _guildConfigurationCollection.Find(filter).FirstOrDefaultAsync();
        return guildConfig.ToDomain();
    }

    private async Task InitGuildConfiguration(string guildId, string guildName)
    {
        await _guildConfigurationCollection.InsertOneAsync(new GuildConfigurationEntity
        {
            GuildId = guildId,
            GuildName = guildName,
            EnableAvatarAnnouncements = true,
            EnableAvatarMentions = true,
            AdminUserIds = new List<string>()
        });
    }

    public async Task<bool> UpdateGuildConfiguration(string guildId, string guildName)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.GuildName, guildName);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1;
    }

    public async Task<bool> DeleteGuildConfiguration(string guildId)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var deleteResult = await _guildConfigurationCollection.DeleteOneAsync(filter);
        return deleteResult.DeletedCount == 1;
    }

    public async Task<bool> AddAllowedUserIds(string guildId, string guildName, List<string> userIds)
    {
        var existingConfig = await GetConfigurationForGuild(guildId, guildName);
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var updatedAllowedUserIds = existingConfig.AdminUserIds;
        updatedAllowedUserIds.AddRange(userIds);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.AdminUserIds, updatedAllowedUserIds);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> RemoveAllowedUserIds(string guildId, string guildName, List<string> userIds)
    {
        var existingConfig = await GetConfigurationForGuild(guildId, guildName);
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var updatedAllowedUserIds = existingConfig.AdminUserIds;
        updatedAllowedUserIds.RemoveAll(userIds.Contains);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.AdminUserIds, updatedAllowedUserIds);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarAnnouncements(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarAnnouncements, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableAvatarMentions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableAvatarMentions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetLogChannel(string guildId, string? channelId)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.LogChannelId, channelId);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableFixTweetReactions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableFixTweetReactions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableDefaultReplies(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableDefaultReplies, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }
}
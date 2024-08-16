using MongoDB.Driver;
using Replybot.Models;
using Replybot.Models.SchemaModels;

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

    public async Task<IList<GuildReplyDefinition>?> GetActiveRepliesForGuild(string guildId)
    {
        var filter = Builders<GuildReplyDefinitionEntity>.Filter.Eq("guildId", guildId);
        var guildReplyDefinitionEntities = await _guildRepliesCollection.Find(filter).ToListAsync();
        return guildReplyDefinitionEntities.Where(r => r.IsActive)
            .Select(r => r.ToDomain())
            .OrderBy(gr => gr.Priority)
            .ToList();
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
            EnableDefaultReplies = true,
            EnableFixTweetReactions = true,
            EnableFixInstagramReactions = true,
            EnableFixTikTokReactions = true,
            EnableFixRedditReactions = true,
            AdminUserIds = []
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

    public async Task<bool> AddIgnoreAvatarChangesUserIds(string guildId, string guildName, List<string> userIds)
    {
        var existingConfig = await GetConfigurationForGuild(guildId, guildName);
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var updatedIgnoreAvatarUsers = existingConfig.IgnoreAvatarChangesUserIds;
        updatedIgnoreAvatarUsers.AddRange(userIds);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.IgnoreAvatarChangesUserIds, updatedIgnoreAvatarUsers);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> RemoveIgnoreAvatarChangesUserIds(string guildId, string guildName, List<string> userIds)
    {
        var existingConfig = await GetConfigurationForGuild(guildId, guildName);
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var updatedIgnoreAvatarUsers = existingConfig.IgnoreAvatarChangesUserIds;
        updatedIgnoreAvatarUsers.RemoveAll(userIds.Contains);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.IgnoreAvatarChangesUserIds, updatedIgnoreAvatarUsers);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
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

    public async Task<bool> SetEnableLoggingUserJoins(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingUserJoins, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableLoggingUserDepartures(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingUserDepartures, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableLoggingMessageEdits(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingMessageEdits, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableLoggingMessageDeletes(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingMessageDeletes, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableLoggingUserBans(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingUserBans, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableLoggingUserUnBans(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableLoggingUserUnBans, isEnabled);
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

    public async Task<bool> SetEnableFixInstagramReactions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableFixInstagramReactions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableFixTikTokReactions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableFixTikTokReactions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableFixRedditReactions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableFixRedditReactions, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableWelcomeMessage(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableWelcomeMessage, isEnabled);
        var updateResult = await _guildConfigurationCollection.UpdateOneAsync(filter, update);
        return updateResult.ModifiedCount == 1 || updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetEnableFixBlueskyReactions(string guildId, bool isEnabled)
    {
        var filter = Builders<GuildConfigurationEntity>.Filter.Eq("guildId", guildId);
        var update = Builders<GuildConfigurationEntity>.Update.Set(config => config.EnableFixBlueskyReactions, isEnabled);
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

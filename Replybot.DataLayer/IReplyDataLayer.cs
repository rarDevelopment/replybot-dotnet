using Replybot.Models;

namespace Replybot.DataLayer;

public interface IReplyDataLayer
{
    Task<IList<GuildReplyDefinition>?> GetActiveRepliesForGuild(string guildId);
    Task<GuildConfiguration?> GetConfigurationForGuild(string guildId, string guildName);
    Task<bool> UpdateGuildConfiguration(string guildId, string guildName);
    Task<bool> SetEnableAvatarAnnouncements(string guildId, bool isEnabled);
    Task<bool> SetEnableAvatarMentions(string guildId, bool isEnabled);
    Task<bool> SetLogChannel(string guildId, string? channelId);
    Task<bool> DeleteGuildConfiguration(string guildId);
    Task<bool> RemoveAllowedUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> SetEnableFixTweetReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableDefaultReplies(string guildId, bool isEnabled);
    Task<bool> SetEnableFixInstagramReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableFixBlueskyReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableFixTikTokReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableWelcomeMessage(string guildId, bool isEnabled);
    Task<bool> SetEnableDepartureMessage(string guildId, bool isEnabled);
    Task<bool> AddIgnoreAvatarChangesUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> AddAllowedUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> RemoveIgnoreAvatarChangesUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> SetEnableFixRedditReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingUserJoins(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingUserDepartures(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingMessageEdits(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingMessageDeletes(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingUserBans(string guildId, bool isEnabled);
    Task<bool> SetEnableLoggingUserUnBans(string guildId, bool isEnabled);
    Task<bool> SetEnableFixThreadsReactions(string guildId, bool isEnabled);
}
﻿using Replybot.Models;

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
    Task<bool> AddAllowedUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> RemoveAllowedUserIds(string guildId, string guildName, List<string> userIds);
    Task<bool> SetEnableFixTweetReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableDefaultReplies(string guildId, bool isEnabled);
    Task<bool> SetEnableFixInstagramReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableFixBlueskyReactions(string guildId, bool isEnabled);
    Task<bool> SetEnableFixTikTokReactions(string guildId, bool isEnabled);
}
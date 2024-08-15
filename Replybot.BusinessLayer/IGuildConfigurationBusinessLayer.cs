﻿using Discord;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public interface IGuildConfigurationBusinessLayer
{
    Task<GuildConfiguration?> GetGuildConfiguration(IGuild? guild);
    Task<bool> UpdateGuildConfiguration(IGuild guild);
    Task<bool> SetEnableAvatarAnnouncements(IGuild guild, bool isEnabled);
    Task<bool> SetEnableAvatarMentions(IGuild guild, bool isEnabled);
    Task<bool> SetLogChannel(IGuild guild, string? channelId);
    Task<bool> DeleteGuildConfiguration(IGuild guild);
    Task<bool> SetApprovedUsers(IGuild guild, List<string> userIds, bool setAllowed);
    Task<bool> CanUserAdmin(IGuild guild, IGuildUser user);
    Task<bool> SetEnableFixTweets(IGuild guild, bool isEnabled);
    Task<bool> SetEnableDefaultReplies(IGuild guild, bool isEnabled);
    Task<bool> SetEnableFixInstagram(IGuild guild, bool isEnabled);
    Task<bool> SetEnableFixBluesky(IGuild guild, bool isEnabled);
    Task<bool> SetEnableFixTikTok(IGuild guild, bool isEnabled);
    Task<bool> SetEnableFixReddit(IGuild guild, bool isEnabled);
    Task<bool> SetEnableWelcomeMessage(IGuild guild, bool isEnabled);
    Task<bool> SetIgnoreUsersForAvatarAnnouncements(IGuild guild, List<string> userIds, bool setAllowed);
}
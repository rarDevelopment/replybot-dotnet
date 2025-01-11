using Discord;
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
    Task<bool> SetEnableDepartureMessage(IGuild guild, bool isEnabled);
    Task<bool> SetIgnoreUsersForAvatarAnnouncements(IGuild guild, List<string> userIds, bool setAllowed);
    Task<bool> SetEnableLoggingUserJoins(IGuild guild, bool isEnabled);
    Task<bool> SetEnableLoggingUserDepartures(IGuild guild, bool isEnabled);
    Task<bool> SetEnableLoggingMessageEdits(IGuild guild, bool isEnabled);
    Task<bool> SetEnableLoggingMessageDeletes(IGuild guild, bool isEnabled);
    Task<bool> SetEnableLoggingUserBans(IGuild guild, bool isEnabled);
    Task<bool> SetEnableLoggingUserUnBans(IGuild guild, bool isEnabled);
    Task<bool> SetEnableFixThreads(IGuild guild, bool isEnabled);
    Task<bool> SetFortniteMapOnlyNamedLocations(IGuild guild, bool isEnabled);
}
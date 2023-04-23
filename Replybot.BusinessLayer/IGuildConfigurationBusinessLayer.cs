using Discord;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public interface IGuildConfigurationBusinessLayer
{
    Task<GuildConfiguration> GetGuildConfiguration(IGuild guild);
    Task<bool> UpdateGuildConfiguration(IGuild guild);
    Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled);
    Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled);
    Task<bool> SetLogChannel(IGuild guild, string? channelId);
    Task<bool> DeleteGuildConfiguration(IGuild guild);
    Task<bool> SetApprovedUsers(IGuild guild, List<string> userIds, bool setAllowed);
    Task<bool> CanUserAdmin(IGuild guild, IGuildUser user);
}
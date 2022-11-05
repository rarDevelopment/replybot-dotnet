using Discord;

namespace Replybot.BusinessLayer;

public interface IGuildConfigurationBusinessLayer
{
    Task<bool> IsAvatarAnnouncementEnabled(IGuild guild);
    Task<bool> IsAvatarMentionEnabled(IGuild guild);
    Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled);
    Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled);
}
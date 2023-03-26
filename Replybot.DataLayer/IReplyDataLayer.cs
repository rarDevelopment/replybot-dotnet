using Replybot.Models;

namespace Replybot.DataLayer
{
    public interface IReplyDataLayer
    {
        IList<GuildReplyDefinition>? GetDefaultReplies();
        Task<IList<GuildReplyDefinition>?> GetRepliesForGuild(ulong guildId);
        Task<GuildConfiguration> GetConfigurationForGuild(ulong guildId, string guildName);
        Task<bool> UpdateGuildConfiguration(ulong guildId, string guildName);
        Task<bool> SetEnableAvatarAnnouncements(ulong guildId, bool isEnabled);
        Task<bool> SetEnableAvatarMentions(ulong guildId, bool isEnabled);
        Task<bool> SetLogChannel(ulong guildId, ulong? channelId);
    }
}

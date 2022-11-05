using Discord;
using Replybot.DataLayer;

namespace Replybot.BusinessLayer
{
    public class GuildConfigurationBusinessLayer : IGuildConfigurationBusinessLayer
    {
        private readonly IResponseDataLayer _responseDataLayer;

        public GuildConfigurationBusinessLayer(IResponseDataLayer responseDataLayer)
        {
            _responseDataLayer = responseDataLayer;
        }

        public async Task<bool> IsAvatarAnnouncementEnabled(IGuild guild)
        {
            var guildConfig = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
            return guildConfig.EnableAvatarAnnouncements;
        }

        public async Task<bool> IsAvatarMentionEnabled(IGuild guild)
        {
            var guildConfig = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
            return guildConfig.EnableAvatarMentions;
        }

        public async Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled)
        {
            try
            {
                var config = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
                return await _responseDataLayer.SetEnableAvatarAnnouncements(guild.Id, isEnabled);

            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled)
        {
            return await _responseDataLayer.SetEnableAvatarMentions(guild.Id, isEnabled);
        }
    }
}

using Replybot.BusinessLayer;

namespace Replybot.Events
{
    public class UserUpdatedEventHandler
    {
        private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;

        public UserUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
        {
            _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        }

        public async Task HandleEvent(SocketUser oldUser, SocketUser newUser)
        {
            foreach (var guild in newUser.MutualGuilds)
            {
                var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
                var announceChange = guildConfig.EnableAvatarAnnouncements;
                var tagUserInChange = guildConfig.EnableAvatarMentions;

                if (!announceChange)
                {
                    return;
                }

                if (newUser.Username != oldUser.Username)
                {
                    await guild.SystemChannel.SendMessageAsync(
                        $"WOWIE! For your awareness, {oldUser.Username} is now {newUser.Username}! {newUser.Mention}`");
                }

                if (newUser.AvatarId != oldUser.AvatarId)
                {
                    if (guild.CurrentUser.Id == newUser.Id)
                    {
                        await guild.SystemChannel.SendMessageAsync(
                            $"Hey everyone! Check out my new look: ${newUser.GetAvatarUrl(ImageFormat.Jpeg)}");
                    }
                    else
                    {
                        await guild.SystemChannel.SendMessageAsync(
                            $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look! Check it out: {newUser.GetAvatarUrl(ImageFormat.Jpeg)}");
                    }
                }
            }
        }
    }
}

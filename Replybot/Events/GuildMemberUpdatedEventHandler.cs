using Replybot.BusinessLayer;

namespace Replybot.Events;

public class GuildMemberUpdatedEventHandler
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly SystemChannelPoster _systemChannelPoster;

    public GuildMemberUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _systemChannelPoster = systemChannelPoster;
    }

    public async Task HandleEvent(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
    {
        if (!cachedOldUser.HasValue)
        {
            return;
        }

        var oldUser = cachedOldUser.Value;

        var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(newUser.Guild);
        var announceChange = guildConfig.EnableAvatarAnnouncements;
        var tagUserInChange = guildConfig.EnableAvatarMentions;

        if (!announceChange)
        {
            return;
        }
        if (newUser.GuildAvatarId != oldUser.GuildAvatarId)
        {
            var avatarUrl = newUser.GetGuildAvatarUrl(ImageFormat.Jpeg);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = newUser.GetDisplayAvatarUrl(ImageFormat.Jpeg);
            }

            await _systemChannelPoster.PostToGuildSystemChannel(newUser.Guild,
                $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look in this server! Check it out: {avatarUrl}",
                $"Guild: {newUser.Guild.Name} ({newUser.Guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                typeof(GuildMemberUpdatedEventHandler));
        }
    }
}
using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class GuildMemberUpdatedNotificationHandler : INotificationHandler<GuildMemberUpdatedNotification>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly SystemChannelPoster _systemChannelPoster;

    public GuildMemberUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _systemChannelPoster = systemChannelPoster;
    }

    public Task Handle(GuildMemberUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var cachedOldUser = notification.CachedOldUser;
            var newUser = notification.NewUser;

            if (!cachedOldUser.HasValue)
            {
                return Task.CompletedTask;
            }

            var oldUser = cachedOldUser.Value;

            var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(newUser.Guild);
            var announceChange = guildConfig.EnableAvatarAnnouncements;
            var tagUserInChange = guildConfig.EnableAvatarMentions;

            if (!announceChange)
            {
                return Task.CompletedTask;
            }
            if (newUser.GuildAvatarId == oldUser.GuildAvatarId)
            {
                return Task.CompletedTask;
            }

            var avatarUrl = newUser.GetGuildAvatarUrl(ImageFormat.Jpeg);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = newUser.GetDisplayAvatarUrl(ImageFormat.Jpeg);
            }

            await _systemChannelPoster.PostToGuildSystemChannel(newUser.Guild,
                $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look in this server! Check it out: {avatarUrl}",
                $"Guild: {newUser.Guild.Name} ({newUser.Guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                typeof(GuildMemberUpdatedNotificationHandler));

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
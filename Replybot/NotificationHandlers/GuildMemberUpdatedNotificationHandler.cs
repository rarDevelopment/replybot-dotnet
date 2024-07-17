using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class GuildMemberUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster, ILogger<DiscordBot> logger)
    : INotificationHandler<GuildMemberUpdatedNotification>
{
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

            var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(newUser.Guild);

            if (guildConfig == null)
            {
                logger.LogError($"No guild configuration found for the guild with id {newUser.Guild.Id} ({newUser.Guild.Name})");
                return Task.CompletedTask;
            }

            var announceChange = guildConfig.EnableAvatarAnnouncements && !guildConfig.IgnoreAvatarChangesUserIds.Contains(newUser.Id.ToString());
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

            await systemChannelPoster.PostMessageToGuildSystemChannel(newUser.Guild,
                $"## Avatar Change\nHeads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a [new look]({avatarUrl}) in this server!",
                $"Guild: {newUser.Guild.Name} ({newUser.Guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                typeof(GuildMemberUpdatedNotificationHandler));

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
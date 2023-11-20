using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    : INotificationHandler<UserUpdatedNotification>
{
    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var oldUser = notification.OldUser;
            var newUser = notification.NewUser;

            foreach (var guild in newUser.MutualGuilds)
            {
                var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
                var announceChange = guildConfig.EnableAvatarAnnouncements;
                var tagUserInChange = guildConfig.EnableAvatarMentions;

                if (!announceChange)
                {
                    continue;
                }

                if (newUser.Username != oldUser.Username)
                {
                    await systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"WOWIE! For your awareness, {oldUser.Username} is now {newUser.Username}! {newUser.Mention}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                }

                if (newUser.AvatarId == oldUser.AvatarId)
                {
                    continue;
                }

                if (guild.CurrentUser.Id == newUser.Id)
                {
                    await systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"Hey everyone! Check out my new look: ${newUser.GetAvatarUrl(ImageFormat.Jpeg)}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                }
                else
                {
                    await systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look! Check it out: {newUser.GetAvatarUrl(ImageFormat.Jpeg)}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
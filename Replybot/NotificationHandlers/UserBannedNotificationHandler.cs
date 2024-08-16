using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserBannedNotificationHandler(LogChannelPoster logChannelPoster, IGuildConfigurationBusinessLayer configurationBusinessLayer) : INotificationHandler<UserBannedNotification>
{
    public Task Handle(UserBannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var config = await configurationBusinessLayer.GetGuildConfiguration(notification.Guild);
            if (config is not { EnableLoggingUserBans: true })
            {
                return Task.CompletedTask;
            }

            await logChannelPoster.SendToLogChannel(notification.Guild, $"Banned User: **{notification.UserWhoWasBanned.Mention}** ({notification.UserWhoWasBanned.Username}) was banned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
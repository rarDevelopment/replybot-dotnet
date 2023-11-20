using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserBannedNotificationHandler(LogChannelPoster logChannelPoster) : INotificationHandler<UserBannedNotification>
{
    public Task Handle(UserBannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.Guild, $"Banned User: **{notification.UserWhoWasBanned.Mention}** ({notification.UserWhoWasBanned.Username}) was banned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserUnbannedNotificationHandler(LogChannelPoster logChannelPoster) : INotificationHandler<UserUnbannedNotification>
{
    public Task Handle(UserUnbannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.Guild, $"Unbanned User: **{notification.UserWhoWasUnbanned.Mention}** ({notification.UserWhoWasUnbanned.Username}) was unbanned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserLeftNotificationHandler(LogChannelPoster logChannelPoster) : INotificationHandler<UserLeftNotification>
{
    public Task Handle(UserLeftNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.Guild, $"User Departure: **{notification.UserWhoLeft.Mention}** ({notification.UserWhoLeft.Username}) has left the server.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
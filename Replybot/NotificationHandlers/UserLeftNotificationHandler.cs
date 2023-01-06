using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserLeftNotificationHandler : INotificationHandler<UserLeftNotification>
{
    private readonly LogChannelPoster _logChannelPoster;

    public UserLeftNotificationHandler(LogChannelPoster logChannelPoster)
    {
        _logChannelPoster = logChannelPoster;
    }
    public Task Handle(UserLeftNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _logChannelPoster.SendToLogChannel(notification.Guild, $"User Departure: **{notification.UserWhoLeft.Mention}** ({notification.UserWhoLeft.Username}) has left the server.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
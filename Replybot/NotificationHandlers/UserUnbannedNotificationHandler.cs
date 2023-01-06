using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserUnbannedNotificationHandler : INotificationHandler<UserUnbannedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;

    public UserUnbannedNotificationHandler(LogChannelPoster logChannelPoster)
    {
        _logChannelPoster = logChannelPoster;
    }
    public Task Handle(UserUnbannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _logChannelPoster.SendToLogChannel(notification.Guild, $"Unbanned User: **{notification.UserWhoWasUnbanned.Mention}** ({notification.UserWhoWasUnbanned.Username}) was unbanned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
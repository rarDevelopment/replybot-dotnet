using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserBannedNotificationHandler : INotificationHandler<UserBannedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;

    public UserBannedNotificationHandler(LogChannelPoster logChannelPoster)
    {
        _logChannelPoster = logChannelPoster;
    }
    public Task Handle(UserBannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _logChannelPoster.SendToLogChannel(notification.Guild, $"Banned User: **{notification.UserWhoWasBanned.Mention}** ({notification.UserWhoWasBanned.Username}) was banned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
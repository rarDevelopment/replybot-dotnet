using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserJoinedNotificationHandler(LogChannelPoster logChannelPoster) : INotificationHandler<UserJoinedNotification>
{
    public Task Handle(UserJoinedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.UserWhoJoined.Guild, $"User Joined: **{notification.UserWhoJoined.Mention}** ({notification.UserWhoJoined.Username}) has joined the server.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
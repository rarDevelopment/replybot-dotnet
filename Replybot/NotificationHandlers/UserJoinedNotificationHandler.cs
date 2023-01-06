using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserJoinedNotificationHandler : INotificationHandler<UserJoinedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;

    public UserJoinedNotificationHandler(LogChannelPoster logChannelPoster)
    {
        _logChannelPoster = logChannelPoster;
    }
    public Task Handle(UserJoinedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _logChannelPoster.SendToLogChannel(notification.UserWhoJoined.Guild, $"User Joined: **{notification.UserWhoJoined.Mention}** ({notification.UserWhoJoined.Username}) has joined the server.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
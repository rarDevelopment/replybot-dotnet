using MediatR;

namespace Replybot.Notifications;

public class UserUnbannedNotification : INotification
{
    public SocketUser UserWhoWasUnbanned { get; }
    public SocketGuild Guild { get; }

    public UserUnbannedNotification(SocketUser userWhoWasUnbanned, SocketGuild guild)
    {
        UserWhoWasUnbanned = userWhoWasUnbanned;
        Guild = guild;
    }
}
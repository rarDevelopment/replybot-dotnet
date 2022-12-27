using MediatR;

namespace Replybot.Notifications;

public class UserJoinedNotification : INotification
{
    public SocketGuildUser UserWhoJoined { get; }

    public UserJoinedNotification(SocketGuildUser userWhoJoined)
    {
        UserWhoJoined = userWhoJoined;
    }
}
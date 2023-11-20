using MediatR;

namespace Replybot.Notifications;

public class UserJoinedNotification(SocketGuildUser userWhoJoined) : INotification
{
    public SocketGuildUser UserWhoJoined { get; } = userWhoJoined;
}
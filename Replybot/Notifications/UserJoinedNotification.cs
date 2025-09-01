

namespace Replybot.Notifications;

public class UserJoinedNotification(SocketGuildUser userWhoJoined)
{
    public SocketGuildUser UserWhoJoined { get; } = userWhoJoined;
}
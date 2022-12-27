using MediatR;

namespace Replybot.Notifications;

public class UserBannedNotification : INotification
{
    public SocketUser UserWhoWasBanned { get; }
    public SocketGuild Guild { get; }

    public UserBannedNotification(SocketUser userWhoWasBanned, SocketGuild guild)
    {
        UserWhoWasBanned = userWhoWasBanned;
        Guild = guild;
    }
}
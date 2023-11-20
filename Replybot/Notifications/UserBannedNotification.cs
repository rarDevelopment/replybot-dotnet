using MediatR;

namespace Replybot.Notifications;

public class UserBannedNotification(SocketUser userWhoWasBanned, SocketGuild guild) : INotification
{
    public SocketUser UserWhoWasBanned { get; } = userWhoWasBanned;
    public SocketGuild Guild { get; } = guild;
}
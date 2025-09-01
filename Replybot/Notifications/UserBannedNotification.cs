

namespace Replybot.Notifications;

public class UserBannedNotification(SocketUser userWhoWasBanned, SocketGuild guild)
{
    public SocketUser UserWhoWasBanned { get; } = userWhoWasBanned;
    public SocketGuild Guild { get; } = guild;
}


namespace Replybot.Notifications;

public class UserUnbannedNotification(SocketUser userWhoWasUnbanned, SocketGuild guild)
{
    public SocketUser UserWhoWasUnbanned { get; } = userWhoWasUnbanned;
    public SocketGuild Guild { get; } = guild;
}
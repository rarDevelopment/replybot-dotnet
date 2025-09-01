

namespace Replybot.Notifications;

public class UserLeftNotification(SocketGuild guild, SocketUser userWhoLeft)
{
    public SocketGuild Guild { get; } = guild;
    public SocketUser UserWhoLeft { get; } = userWhoLeft;
}
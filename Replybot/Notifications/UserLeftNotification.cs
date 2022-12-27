using MediatR;

namespace Replybot.Notifications;

public class UserLeftNotification : INotification
{
    public SocketGuild Guild { get; }
    public SocketUser UserWhoLeft { get; }

    public UserLeftNotification(SocketGuild guild, SocketUser userWhoLeft)
    {
        Guild = guild;
        UserWhoLeft = userWhoLeft;
    }
}
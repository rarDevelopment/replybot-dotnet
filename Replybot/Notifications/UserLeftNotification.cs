using MediatR;

namespace Replybot.Notifications;

public class UserLeftNotification(SocketGuild guild, SocketUser userWhoLeft) : INotification
{
    public SocketGuild Guild { get; } = guild;
    public SocketUser UserWhoLeft { get; } = userWhoLeft;
}
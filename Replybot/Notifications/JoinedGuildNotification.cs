using MediatR;

namespace Replybot.Notifications;

public class JoinedGuildNotification : INotification
{
    public SocketGuild JoinedGuild { get; }

    public JoinedGuildNotification(SocketGuild joinedGuild)
    {
        JoinedGuild = joinedGuild;
    }
}
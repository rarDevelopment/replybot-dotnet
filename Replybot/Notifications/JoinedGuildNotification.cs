using MediatR;

namespace Replybot.Notifications;

public class JoinedGuildNotification(SocketGuild joinedGuild) : INotification
{
    public SocketGuild JoinedGuild { get; } = joinedGuild;
}
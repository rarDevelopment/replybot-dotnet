using MediatR;

namespace Replybot.Notifications;

public class GuildUpdatedNotification(SocketGuild oldGuild, SocketGuild newGuild) : INotification
{
    public SocketGuild OldGuild { get; } = oldGuild;
    public SocketGuild NewGuild { get; } = newGuild;
}
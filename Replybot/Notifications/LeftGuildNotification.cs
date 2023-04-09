using MediatR;

namespace Replybot.Notifications;

public class LeftGuildNotification : INotification
{
    public SocketGuild GuildLeft { get; }

    public LeftGuildNotification(SocketGuild guildLeft)
    {
        GuildLeft = guildLeft;
    }
}
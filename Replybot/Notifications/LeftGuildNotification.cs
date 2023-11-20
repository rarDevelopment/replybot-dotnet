using MediatR;

namespace Replybot.Notifications;

public class LeftGuildNotification(SocketGuild guildLeft) : INotification
{
    public SocketGuild GuildLeft { get; } = guildLeft;
}
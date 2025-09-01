

namespace Replybot.Notifications;

public class LeftGuildNotification(SocketGuild guildLeft)
{
    public SocketGuild GuildLeft { get; } = guildLeft;
}
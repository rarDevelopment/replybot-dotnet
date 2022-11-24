using MediatR;

namespace Replybot.Notifications
{
    public class GuildUpdatedNotification : INotification
    {
        public SocketGuild OldGuild { get; }
        public SocketGuild NewGuild { get; }

        public GuildUpdatedNotification(SocketGuild oldGuild, SocketGuild newGuild)
        {
            OldGuild = oldGuild;
            NewGuild = newGuild;
        }
    }
}

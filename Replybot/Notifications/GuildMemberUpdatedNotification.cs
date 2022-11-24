using MediatR;

namespace Replybot.Notifications
{
    public class GuildMemberUpdatedNotification : INotification
    {
        public Cacheable<SocketGuildUser, ulong> CachedOldUser { get; }
        public SocketGuildUser NewUser { get; }

        public GuildMemberUpdatedNotification(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
        {
            CachedOldUser = cachedOldUser;
            NewUser = newUser;
        }
    }
}

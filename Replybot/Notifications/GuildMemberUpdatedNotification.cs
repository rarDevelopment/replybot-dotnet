

namespace Replybot.Notifications;

public class GuildMemberUpdatedNotification(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
   
{
    public Cacheable<SocketGuildUser, ulong> CachedOldUser { get; } = cachedOldUser;
    public SocketGuildUser NewUser { get; } = newUser;
}
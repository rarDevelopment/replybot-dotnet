

namespace Replybot.Notifications;

public class UserUpdatedNotification(SocketUser oldUser, SocketUser newUser)
{
    public SocketUser OldUser { get; } = oldUser;
    public SocketUser NewUser { get; } = newUser;
}
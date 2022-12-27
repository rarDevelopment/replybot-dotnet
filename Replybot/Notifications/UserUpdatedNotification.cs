using MediatR;

namespace Replybot.Notifications;

public class UserUpdatedNotification : INotification
{
    public SocketUser OldUser { get; }
    public SocketUser NewUser { get; }

    public UserUpdatedNotification(SocketUser oldUser, SocketUser newUser)
    {
        OldUser = oldUser;
        NewUser = newUser;
    }
}
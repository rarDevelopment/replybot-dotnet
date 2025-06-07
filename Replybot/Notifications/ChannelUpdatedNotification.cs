using MediatR;

namespace Replybot.Notifications;

public class ChannelUpdatedNotification(SocketChannel oldChannel, SocketChannel newChannel) : INotification
{
    public SocketChannel OldChannel { get; } = oldChannel;
    public SocketChannel NewChannel { get; } = newChannel;
}
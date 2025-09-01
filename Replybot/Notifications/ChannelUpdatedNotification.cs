

namespace Replybot.Notifications;

public class ChannelUpdatedNotification(SocketChannel oldChannel, SocketChannel newChannel)
{
    public SocketChannel OldChannel { get; } = oldChannel;
    public SocketChannel NewChannel { get; } = newChannel;
}
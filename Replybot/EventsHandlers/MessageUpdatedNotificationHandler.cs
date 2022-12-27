using MediatR;
using Replybot.Notifications;

namespace Replybot.EventsHandlers;
public class MessageUpdatedNotificationHandler : INotificationHandler<MessageUpdatedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;
    private readonly LogMessageBuilder _logMessageBuilder;

    public MessageUpdatedNotificationHandler(LogChannelPoster logChannelPoster, LogMessageBuilder logMessageBuilder)
    {
        _logChannelPoster = logChannelPoster;
        _logMessageBuilder = logMessageBuilder;
    }
    public Task Handle(MessageUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var channel = notification.Channel;
            if (channel is not SocketGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var updatedMessage = notification.NewMessage;
            var originalMessageContent = notification.OldMessage.HasValue ? notification.OldMessage.Value.Content : null;

            if (originalMessageContent == null)
            {
                return Task.CompletedTask;
            }

            var embedBuilder = _logMessageBuilder.CreateEmbedBuilderWithFields(notification.NewMessage, originalMessageContent,
                $"Message Updated: Message from {notification.NewMessage.Author} edited in #{updatedMessage.Channel}");

            await _logChannelPoster.SendToLogChannel(guildChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
using MediatR;
using Replybot.Notifications;

namespace Replybot.EventsHandlers;
public class MessageDeletedNotificationHandler : INotificationHandler<MessageDeletedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;
    private readonly LogMessageBuilder _logMessageBuilder;

    public MessageDeletedNotificationHandler(LogChannelPoster logChannelPoster, LogMessageBuilder logMessageBuilder)
    {
        _logChannelPoster = logChannelPoster;
        _logMessageBuilder = logMessageBuilder;
    }
    public Task Handle(MessageDeletedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var channel = await notification.Channel.GetOrDownloadAsync();
            if (channel is not SocketGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var deletedMessage = await notification.DeletedMessage.GetOrDownloadAsync();

            var embedBuilder = _logMessageBuilder.CreateEmbedBuilder(deletedMessage, $"Message Deleted: Message from {deletedMessage.Author} deleted in <#{deletedMessage.Channel.Id}>");

            await _logChannelPoster.SendToLogChannel(guildChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
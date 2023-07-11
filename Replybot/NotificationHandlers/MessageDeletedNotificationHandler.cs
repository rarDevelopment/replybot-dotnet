using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class MessageDeletedNotificationHandler : INotificationHandler<MessageDeletedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;
    private readonly ExistingMessageEmbedBuilder _logMessageBuilder;
    private readonly DiscordSocketClient _client;

    public MessageDeletedNotificationHandler(LogChannelPoster logChannelPoster, ExistingMessageEmbedBuilder logMessageBuilder, DiscordSocketClient client)
    {
        _logChannelPoster = logChannelPoster;
        _logMessageBuilder = logMessageBuilder;
        _client = client;
    }
    public Task Handle(MessageDeletedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var channel = await notification.Channel.GetOrDownloadAsync();
            if (channel is not SocketTextChannel textChannel)
            {
                return Task.CompletedTask;
            }

            var deletedMessage = await notification.DeletedMessage.GetOrDownloadAsync();

            if (deletedMessage == null)
            {
                await _logChannelPoster.SendToLogChannel(textChannel.Guild,
                    "Message Deleted: [could not retrieve contents of message from cache]");
                return Task.CompletedTask;
            }

            if (deletedMessage.Author.Id == _client.CurrentUser.Id)
            {
                return Task.CompletedTask;
            }

            var embedBuilder = _logMessageBuilder.CreateEmbedBuilder("Message Deleted", $"Message from {deletedMessage.Author.Mention} deleted in {textChannel.Mention}", deletedMessage);

            await _logChannelPoster.SendToLogChannel(textChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
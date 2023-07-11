using MediatR;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class MessageUpdatedNotificationHandler : INotificationHandler<MessageUpdatedNotification>
{
    private readonly LogChannelPoster _logChannelPoster;
    private readonly ExistingMessageEmbedBuilder _logMessageBuilder;
    private readonly DiscordSocketClient _client;

    public MessageUpdatedNotificationHandler(LogChannelPoster logChannelPoster, ExistingMessageEmbedBuilder logMessageBuilder, DiscordSocketClient client)
    {
        _logChannelPoster = logChannelPoster;
        _logMessageBuilder = logMessageBuilder;
        _client = client;
    }

    public Task Handle(MessageUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            if (notification.NewMessage.Author.Id == _client.CurrentUser.Id)
            {
                return Task.CompletedTask;
            }

            var channel = notification.Channel;
            if (channel is not SocketTextChannel textChannel)
            {
                return Task.CompletedTask;
            }

            var originalMessageContent = (notification.OldMessage.HasValue ? notification.OldMessage.Value.Content : null)
                                         ?? "[could not retrieve contents of message from cache]";

            var messages = new Dictionary<string, string>
            {
                {"Original:", originalMessageContent},
                {"Edited:", notification.NewMessage.Content}
            };

            if (originalMessageContent == notification.NewMessage.Content)
            {
                return Task.CompletedTask;
            }

            var embedBuilder = _logMessageBuilder.CreateEmbedBuilderWithFields("Message Updated", $"[Message]({notification.NewMessage.GetJumpUrl()}) from {notification.NewMessage.Author.Mention} edited in {textChannel.Mention}", messages);

            await _logChannelPoster.SendToLogChannel(textChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
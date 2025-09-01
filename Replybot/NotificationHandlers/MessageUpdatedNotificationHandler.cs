using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class MessageUpdatedNotificationHandler(LogChannelPoster logChannelPoster,
        ExistingMessageEmbedBuilder logMessageBuilder, IGuildConfigurationBusinessLayer configurationBusinessLayer, DiscordSocketClient client)
    : IEventHandler<MessageUpdatedNotification>
{
    public Task HandleAsync(MessageUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            if (notification.NewMessage.Author.Id == client.CurrentUser.Id)
            {
                return Task.CompletedTask;
            }

            var channel = notification.Channel;
            if (channel is not SocketTextChannel textChannel)
            {
                return Task.CompletedTask;
            }

            var config = await configurationBusinessLayer.GetGuildConfiguration(textChannel.Guild);
            if (config is not { EnableLoggingMessageEdits: true })
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

            var embedBuilder = logMessageBuilder.CreateEmbedBuilderWithFields("Message Updated", $"[Message]({notification.NewMessage.GetJumpUrl()}) from {notification.NewMessage.Author.Mention} edited in {textChannel.Mention}", messages);

            await logChannelPoster.SendToLogChannel(textChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
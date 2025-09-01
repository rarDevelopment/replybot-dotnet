using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class MessageDeletedNotificationHandler(LogChannelPoster logChannelPoster,
        ExistingMessageEmbedBuilder logMessageBuilder, IGuildConfigurationBusinessLayer configurationBusinessLayer, DiscordSocketClient client)
    : IEventHandler<MessageDeletedNotification>
{
    public Task HandleAsync(MessageDeletedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var channel = await notification.Channel.GetOrDownloadAsync();
            if (channel is not SocketTextChannel textChannel)
            {
                return Task.CompletedTask;
            }

            var config = await configurationBusinessLayer.GetGuildConfiguration(textChannel.Guild);
            if (config is not { EnableLoggingMessageDeletes: true })
            {
                return Task.CompletedTask;
            }

            var deletedMessage = await notification.DeletedMessage.GetOrDownloadAsync();

            if (deletedMessage == null)
            {
                await logChannelPoster.SendToLogChannel(textChannel.Guild,
                    "Message Deleted: [could not retrieve contents of message from cache]");
                return Task.CompletedTask;
            }

            if (deletedMessage.Author.Id == client.CurrentUser.Id)
            {
                return Task.CompletedTask;
            }

            var embedBuilder = logMessageBuilder.CreateEmbedBuilder("Message Deleted", $"Message from {deletedMessage.Author.Mention} deleted in {textChannel.Mention}", deletedMessage);

            await logChannelPoster.SendToLogChannel(textChannel.Guild, embedBuilder.Build());

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
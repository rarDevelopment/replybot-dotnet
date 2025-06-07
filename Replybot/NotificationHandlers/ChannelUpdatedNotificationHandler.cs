using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class ChannelUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster, ILogger<DiscordBot> logger)
    : INotificationHandler<ChannelUpdatedNotification>
{
    public Task Handle(ChannelUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            if (notification.NewChannel is not SocketTextChannel newChannel)
            {
                return Task.CompletedTask;
            }

            var guild = newChannel.Guild;
            var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
            if (guildConfig == null)
            {
                logger.LogError($"No guild configuration found for the guild with id {guild.Id} ({guild.Name})");
                return Task.CompletedTask;
            }

            if (!guildConfig.EnableChannelUpdateAnnouncements)
            {
                return Task.CompletedTask;
            }

            var oldChannel = notification.OldChannel as SocketTextChannel;

            var messageText = "";

            if (oldChannel?.Name != null && oldChannel.Name != newChannel.Name)
            {
                messageText += "\n## Channel Name Update\n";
                messageText += $"This channel has been renamed to **{newChannel.Name}**";
                messageText += oldChannel.Name != null
                    ? $" (previously **{oldChannel.Name ?? "[name could not be found]"}**)."
                    : ".";
            }

            if (oldChannel?.Topic != null && oldChannel.Topic != newChannel.Topic)
            {
                messageText += "\n## Channel Topic Update\n";
                messageText += $"This channel's topic has been updated to: **{newChannel.Topic ?? "[no topic]"}**";
            }

            await newChannel.SendMessageAsync(messageText);

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
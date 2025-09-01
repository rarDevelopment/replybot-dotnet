using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class JoinedGuildNotificationHandler(
    IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
    SystemChannelPoster systemChannelPoster,
    ILogger<DiscordBot> logger)
    : IEventHandler<JoinedGuildNotification>
{
    public Task HandleAsync(JoinedGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(notification.JoinedGuild);
            if (guildConfig == null)
            {
                logger.LogError($"No guild configuration found for the guild with id {notification.JoinedGuild.Id} ({notification.JoinedGuild.Name})");
                return Task.CompletedTask;
            }
            await systemChannelPoster.PostMessageToGuildSystemChannel(notification.JoinedGuild,
                $"Hello, people of {guildConfig.GuildName}! Glad to be here!",
                "Couldn't post to the system channel on JoinedGuild",
                typeof(JoinedGuildNotificationHandler));
            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
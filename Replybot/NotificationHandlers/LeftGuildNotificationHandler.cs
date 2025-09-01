using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class LeftGuildNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    : IEventHandler<LeftGuildNotification>
{
    public Task HandleAsync(LeftGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await guildConfigurationBusinessLayer.DeleteGuildConfiguration(notification.GuildLeft);
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
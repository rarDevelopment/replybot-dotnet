using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class LeftGuildNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    : INotificationHandler<LeftGuildNotification>
{
    public Task Handle(LeftGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await guildConfigurationBusinessLayer.DeleteGuildConfiguration(notification.GuildLeft);
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
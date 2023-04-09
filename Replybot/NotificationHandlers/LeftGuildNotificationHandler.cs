using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class LeftGuildNotificationHandler : INotificationHandler<LeftGuildNotification>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;

    public LeftGuildNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
    }

    public Task Handle(LeftGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await _guildConfigurationBusinessLayer.DeleteGuildConfiguration(notification.GuildLeft);
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
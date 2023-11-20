using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class JoinedGuildNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    : INotificationHandler<JoinedGuildNotification>
{
    public Task Handle(JoinedGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(notification.JoinedGuild);
            await systemChannelPoster.PostToGuildSystemChannel(notification.JoinedGuild,
                $"Hello, people of {guildConfig.GuildName}! Glad to be here!",
                "Couldn't post to the system channel on JoinedGuild",
                typeof(JoinedGuildNotificationHandler));
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
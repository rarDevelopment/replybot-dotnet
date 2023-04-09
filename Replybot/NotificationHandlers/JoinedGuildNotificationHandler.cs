using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class JoinedGuildNotificationHandler : INotificationHandler<JoinedGuildNotification>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly SystemChannelPoster _systemChannelPoster;

    public JoinedGuildNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _systemChannelPoster = systemChannelPoster;
    }

    public Task Handle(JoinedGuildNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(notification.JoinedGuild);
            await _systemChannelPoster.PostToGuildSystemChannel(notification.JoinedGuild,
                $"Hello, people of {guildConfig.GuildName}! Glad to be here!",
                "Couldn't post to the system channel on JoinedGuild",
                typeof(JoinedGuildNotificationHandler));
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
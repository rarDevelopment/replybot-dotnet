using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class GuildUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    : INotificationHandler<GuildUpdatedNotification>
{
    public Task Handle(GuildUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var oldGuild = notification.OldGuild;
            var newGuild = notification.NewGuild;

            if (newGuild.Name != oldGuild.Name)
            {
                await systemChannelPoster.PostToGuildSystemChannel(
                    newGuild,
                    $"Wow, a server name change! This server has been renamed from **{oldGuild.Name}** to **{newGuild.Name}**.",
                    $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedNotificationHandler));
                await guildConfigurationBusinessLayer.UpdateGuildConfiguration(newGuild);
            }

            if (newGuild.IconId != oldGuild.IconId)
            {
                await systemChannelPoster.PostToGuildSystemChannel(
                    newGuild,
                    $"Hey look! A new server icon! {newGuild.IconUrl}",
                    $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedNotificationHandler));
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
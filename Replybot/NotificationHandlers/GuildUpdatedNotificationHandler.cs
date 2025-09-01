﻿using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class GuildUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    : IEventHandler<GuildUpdatedNotification>
{
    public Task HandleAsync(GuildUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var oldGuild = notification.OldGuild;
            var newGuild = notification.NewGuild;

            if (newGuild.Name != oldGuild.Name)
            {
                await systemChannelPoster.PostMessageToGuildSystemChannel(
                    newGuild,
                    $"## Server Name Change\nWow, a server name change! This server has been renamed from **{oldGuild.Name}** to **{newGuild.Name}**.",
                    $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedNotificationHandler));
                await guildConfigurationBusinessLayer.UpdateGuildConfiguration(newGuild);
            }

            if (newGuild.IconId != oldGuild.IconId)
            {
                var guildIcon = CDN.GetGuildIconUrl(newGuild.Id, newGuild.IconId, format: newGuild.IconId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png);
                await systemChannelPoster.PostMessageToGuildSystemChannel(
                    newGuild,
                    $"## Server Icon Change\nHey look! A [new server icon]({guildIcon})!",
                    $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedNotificationHandler));
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
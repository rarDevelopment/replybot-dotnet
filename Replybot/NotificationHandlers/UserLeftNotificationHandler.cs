using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserLeftNotificationHandler(
    LogChannelPoster logChannelPoster,
    SystemChannelPoster systemChannelPoster,
    IGuildConfigurationBusinessLayer configurationBusinessLayer) : IEventHandler<UserLeftNotification>
{
    public Task HandleAsync(UserLeftNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var guildConfiguration = await configurationBusinessLayer.GetGuildConfiguration(notification.Guild);
            if (guildConfiguration is { EnableLoggingUserDepartures: true })
            {
                await logChannelPoster.SendToLogChannel(notification.Guild,
                    $"User Departure: **{notification.UserWhoLeft.Mention}** ({notification.UserWhoLeft.Username}) has left the server.");
            }

            if (guildConfiguration is not { EnableDepartureMessage: true })
            {
                return Task.CompletedTask;
            }

            var messageToSend = $"## Member Departure:\n🏃🏼💨 {notification.UserWhoLeft.Mention} has left the server.";

            await systemChannelPoster.PostMessageToGuildSystemChannel(
                notification.Guild,
                messageToSend,
                $"User: {notification.UserWhoLeft.Id} {notification.UserWhoLeft.GlobalName}",
                typeof(UserLeftNotificationHandler));

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
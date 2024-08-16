using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserLeftNotificationHandler(LogChannelPoster logChannelPoster, IGuildConfigurationBusinessLayer configurationBusinessLayer) : INotificationHandler<UserLeftNotification>
{
    public Task Handle(UserLeftNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var config = await configurationBusinessLayer.GetGuildConfiguration(notification.Guild);
            if (config is not { EnableLoggingUserDepartures: true })
            {
                return Task.CompletedTask;
            }

            await logChannelPoster.SendToLogChannel(notification.Guild, $"User Departure: **{notification.UserWhoLeft.Mention}** ({notification.UserWhoLeft.Username}) has left the server.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserUnbannedNotificationHandler(LogChannelPoster logChannelPoster, IGuildConfigurationBusinessLayer configurationBusinessLayer) : INotificationHandler<UserUnbannedNotification>
{
    public Task Handle(UserUnbannedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var config = await configurationBusinessLayer.GetGuildConfiguration(notification.Guild);
            if (config is not { EnableLoggingUserUnBans: true })
            {
                return Task.CompletedTask;
            }
            await logChannelPoster.SendToLogChannel(notification.Guild, $"Unbanned User: **{notification.UserWhoWasUnbanned.Mention}** ({notification.UserWhoWasUnbanned.Username}) was unbanned.");

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
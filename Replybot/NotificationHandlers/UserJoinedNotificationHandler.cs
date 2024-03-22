using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserJoinedNotificationHandler(
    LogChannelPoster logChannelPoster,
    SystemChannelPoster systemChannelPoster,
    IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer) : INotificationHandler<UserJoinedNotification>
{
    public Task Handle(UserJoinedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.UserWhoJoined.Guild, $"User Joined: **{notification.UserWhoJoined.Mention}** ({notification.UserWhoJoined.Username}) has joined the server.");

            var guildConfiguration =
                await guildConfigurationBusinessLayer.GetGuildConfiguration(notification.UserWhoJoined.Guild);

            if (guildConfiguration is { EnableWelcomeMessage: true })
            {
                const string usernameKeyword = "{{USER_WHO_JOINED}}";
                var welcomeMessages = new List<string>
                {
                    $"Look out everyone, {usernameKeyword} is here!",
                    $"Yo {usernameKeyword}!",
                    $"Howdy {usernameKeyword}!",
                    $"Did someone invite {usernameKeyword}?",
                    $"Well well well, look who it is. Hello {usernameKeyword}.",
                    $"*checks the list* Yep, you're on the list. Come on in, {usernameKeyword}!",
                    $"Good day to you {usernameKeyword}!",
                    $"I spy with my little eye, {usernameKeyword}",
                };

                var randomIndex = new Random().Next(welcomeMessages.Count);
                var welcomeMessageWithMention = welcomeMessages[randomIndex].Replace(usernameKeyword, notification.UserWhoJoined.Mention);

                await systemChannelPoster.PostToGuildSystemChannel(
                    notification.UserWhoJoined.Guild,
                    welcomeMessageWithMention,
                    $"User: {notification.UserWhoJoined.Id} {notification.UserWhoJoined.GlobalName}",
                    typeof(UserJoinedNotificationHandler));
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
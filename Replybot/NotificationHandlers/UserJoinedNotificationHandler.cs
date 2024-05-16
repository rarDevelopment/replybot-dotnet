using DiscordDotNetUtilities.Interfaces;
using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserJoinedNotificationHandler(
    LogChannelPoster logChannelPoster,
    SystemChannelPoster systemChannelPoster,
    IDiscordFormatter discordFormatter,
    IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer) : INotificationHandler<UserJoinedNotification>
{
    public Task Handle(UserJoinedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await logChannelPoster.SendToLogChannel(notification.UserWhoJoined.Guild, $"User Joined: **{notification.UserWhoJoined.Mention}** ({notification.UserWhoJoined.Username}) has joined the server.");

            var guildConfiguration =
                await guildConfigurationBusinessLayer.GetGuildConfiguration(notification.UserWhoJoined.Guild);

            if (guildConfiguration is not { EnableWelcomeMessage: true })
            {
                return Task.CompletedTask;
            }

            const string usernameKeyword = "{{USER_WHO_JOINED}}";
            var welcomeMessages = new List<string>
            {
                $"Look out everyone, {usernameKeyword} is here! Welcome!",
                $"Welcome in, {usernameKeyword}!",
                $"Someone new is here! Welcome {usernameKeyword}!",
                $"Did someone invite {usernameKeyword}? Welcome!",
                $"Well well well, look who it is. Welcome, {usernameKeyword}!",
                $"*checks the list* Yep, you're on the list. Come on in, {usernameKeyword}! Welcome!",
                $"Good day to you {usernameKeyword}! Welcome!",
                $"I spy with my little eye, {usernameKeyword}! Welcome!",
            };

            var randomIndex = new Random().Next(welcomeMessages.Count);
            var welcomeMessageWithMention = welcomeMessages[randomIndex].Replace(usernameKeyword, notification.UserWhoJoined.Mention);

            await systemChannelPoster.PostEmbedToGuildSystemChannel(
                notification.UserWhoJoined.Guild,
                discordFormatter.BuildRegularEmbed("New Member Has Arrived!", welcomeMessageWithMention),
                $"User: {notification.UserWhoJoined.Id} {notification.UserWhoJoined.GlobalName}",
                typeof(UserJoinedNotificationHandler));

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
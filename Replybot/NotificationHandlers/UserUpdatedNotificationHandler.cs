using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.NotificationHandlers;
public class UserUpdatedNotificationHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster, ILogger<DiscordBot> logger)
    : INotificationHandler<UserUpdatedNotification>
{
    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var oldUser = notification.OldUser;
            var newUser = notification.NewUser;

            foreach (var guild in newUser.MutualGuilds)
            {
                var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
                if (guildConfig == null)
                {
                    logger.LogError($"No guild configuration found for the guild with id {guild.Id} ({guild.Name})");
                    continue;
                }
                var announceChange = guildConfig.EnableAvatarAnnouncements && !guildConfig.IgnoreAvatarChangesUserIds.Contains(newUser.Id.ToString());
                var tagUserInChange = guildConfig.EnableAvatarMentions;

                if (!announceChange)
                {
                    continue;
                }

                if (newUser.Username != oldUser.Username)
                {
                    await systemChannelPoster.PostMessageToGuildSystemChannel(
                        guild,
                        $"WOWIE! For your awareness, {oldUser.Username} is now {newUser.Username}! {newUser.Mention}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                }

                if (newUser.AvatarId == oldUser.AvatarId)
                {
                    continue;
                }

                if (guild.CurrentUser.Id == newUser.Id)
                {
                    await systemChannelPoster.PostMessageToGuildSystemChannel(
                        guild,
                        $"Hey everyone! Check out my new look: ${newUser.GetAvatarUrl()}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                }
                else
                {
                    await systemChannelPoster.PostMessageToGuildSystemChannel(
                        guild,
                        $"## Avatar Change\nHeads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a [new look]({newUser.GetAvatarUrl()})!",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedNotificationHandler));
                    logger.Log(LogLevel.Information, $"User Avatar Change: {newUser.Username} ({newUser.Id}) | new avatar id: {newUser.AvatarId} ({newUser.GetAvatarUrl()}) | old avatar id: {oldUser.AvatarId} ({oldUser.GetAvatarUrl()}) |");
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
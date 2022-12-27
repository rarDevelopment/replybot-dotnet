using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;

namespace Replybot.EventsHandlers;
public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedNotification>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly SystemChannelPoster _systemChannelPoster;

    public UserUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _systemChannelPoster = systemChannelPoster;
    }
    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var oldUser = notification.OldUser;
            var newUser = notification.NewUser;

            foreach (var guild in newUser.MutualGuilds)
            {
                var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
                var announceChange = guildConfig.EnableAvatarAnnouncements;
                var tagUserInChange = guildConfig.EnableAvatarMentions;

                if (!announceChange)
                {
                    continue;
                }

                if (newUser.Username != oldUser.Username)
                {
                    await _systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"WOWIE! For your awareness, {oldUser.Username} is now {newUser.Username}! {newUser.Mention}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedEventHandler));
                }

                if (newUser.AvatarId == oldUser.AvatarId)
                {
                    continue;
                }

                if (guild.CurrentUser.Id == newUser.Id)
                {
                    await _systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"Hey everyone! Check out my new look: ${newUser.GetAvatarUrl(ImageFormat.Jpeg)}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedEventHandler));
                }
                else
                {
                    await _systemChannelPoster.PostToGuildSystemChannel(
                        guild,
                        $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look! Check it out: {newUser.GetAvatarUrl(ImageFormat.Jpeg)}",
                        $"Guild: {guild.Name} ({guild.Id}) - User: {newUser.Username} ({newUser.Id})",
                        typeof(UserUpdatedEventHandler));
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}
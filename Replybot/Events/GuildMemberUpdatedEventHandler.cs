using Replybot.BusinessLayer;

namespace Replybot.Events;

public class GuildMemberUpdatedEventHandler
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly ILogger<DiscordBot> _logger;

    public GuildMemberUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer, ILogger<DiscordBot> logger)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _logger = logger;
    }

    public async Task HandleEvent(Cacheable<SocketGuildUser, ulong> cachedOldUser, SocketGuildUser newUser)
    {
        if (!cachedOldUser.HasValue)
        {
            return;
        }

        var oldUser = cachedOldUser.Value;

        var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(newUser.Guild);
        var announceChange = guildConfig.EnableAvatarAnnouncements;
        var tagUserInChange = guildConfig.EnableAvatarMentions;

        if (!announceChange)
        {
            return;
        }
        if (newUser.GuildAvatarId != oldUser.GuildAvatarId)
        {
            var avatarUrl = newUser.GetGuildAvatarUrl(ImageFormat.Jpeg);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = newUser.GetDisplayAvatarUrl(ImageFormat.Jpeg);
            }

            try
            {
                await newUser.Guild.SystemChannel.SendMessageAsync(
                    $"Heads up! {(tagUserInChange ? newUser.Mention : newUser.Username)} has a new look! Check it out: {avatarUrl}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Error Sending User ({0}:{1}) Guild-Specific Avatar Change Alert to Guild {2} (id: {3}): {4}", newUser.Username, newUser.Id, newUser.Guild.Name, newUser.Guild.Id, ex.Message);
            }
        }
    }
}
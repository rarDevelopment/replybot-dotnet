using Replybot.BusinessLayer;

namespace Replybot;

public class LogChannelPoster
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;

    public LogChannelPoster(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
    }

    private async Task<SocketTextChannel?> GetLogChannel(IGuild guild)
    {
        var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(guild);
        if (guildConfig.LogChannelId == null)
        {
            return null;
        }

        var logChannel = await guild.GetChannelAsync(guildConfig.LogChannelId.Value);

        return logChannel as SocketTextChannel ?? null;
    }

    public async Task SendToLogChannel(IGuild guild, string textToSend)
    {
        var logChannel = await GetLogChannel(guild);
        if (logChannel == null)
        {
            return;
        }

        await logChannel.SendMessageAsync(textToSend);
    }

    public async Task SendToLogChannel(IGuild guild, Embed embedToSend)
    {
        var logChannel = await GetLogChannel(guild);
        if (logChannel == null)
        {
            return;
        }

        await logChannel.SendMessageAsync(embed: embedToSend);
    }
}
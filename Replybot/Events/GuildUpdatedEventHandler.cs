using Replybot.BusinessLayer;

namespace Replybot.Events;

public class GuildUpdatedEventHandler
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;

    public GuildUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
    }
    public async Task HandleEvent(SocketGuild oldGuild, SocketGuild newGuild)
    {
        if (newGuild.Name != oldGuild.Name)
        {
            await newGuild.SystemChannel.SendMessageAsync(
                $"Wow, a server name change! This server has been renamed from **{oldGuild.Name}** to **{newGuild.Name}**.");
            await _guildConfigurationBusinessLayer.UpdateGuildConfiguration(newGuild);
        }

        if (newGuild.IconId != oldGuild.IconId)
        {
            await newGuild.SystemChannel.SendMessageAsync($"Hey look! A new server icon! {newGuild.IconUrl}");
        }
    }
}
using Replybot.BusinessLayer;

namespace Replybot.Events;

public class GuildUpdatedEventHandler
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly SystemChannelPoster _systemChannelPoster;

    public GuildUpdatedEventHandler(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        SystemChannelPoster systemChannelPoster)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _systemChannelPoster = systemChannelPoster;
    }
    public async Task HandleEvent(SocketGuild oldGuild, SocketGuild newGuild)
    {
        if (newGuild.Name != oldGuild.Name)
        {
            await _systemChannelPoster.PostToGuildSystemChannel(
                newGuild,
                $"Wow, a server name change! This server has been renamed from **{oldGuild.Name}** to **{newGuild.Name}**.",
                $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedEventHandler));
            await _guildConfigurationBusinessLayer.UpdateGuildConfiguration(newGuild);
        }

        if (newGuild.IconId != oldGuild.IconId)
        {
            await _systemChannelPoster.PostToGuildSystemChannel(
                newGuild,
                $"Hey look! A new server icon! {newGuild.IconUrl}",
                $"Guild: {newGuild.Name} ({newGuild.Id})", typeof(GuildUpdatedEventHandler));
        }
    }
}
using Discord;
using Replybot.DataLayer;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public class GuildConfigurationBusinessLayer : IGuildConfigurationBusinessLayer
{
    private readonly IResponseDataLayer _responseDataLayer;

    public GuildConfigurationBusinessLayer(IResponseDataLayer responseDataLayer)
    {
        _responseDataLayer = responseDataLayer;
    }

    public async Task<GuildConfiguration> GetGuildConfiguration(IGuild guild)
    {
        return await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
    }

    public async Task<bool> UpdateGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _responseDataLayer.UpdateGuildConfiguration(guild.Id, guild.Name);
        }

        return false;
    }

    public async Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _responseDataLayer.SetEnableAvatarAnnouncements(guild.Id, isEnabled);
        }

        return false;
    }

    public async Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _responseDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _responseDataLayer.SetEnableAvatarMentions(guild.Id, isEnabled);
        }

        return false;
    }
}
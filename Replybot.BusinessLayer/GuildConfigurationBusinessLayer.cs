using Discord;
using Replybot.DataLayer;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public class GuildConfigurationBusinessLayer : IGuildConfigurationBusinessLayer
{
    private readonly IReplyDataLayer _replyDataLayer;

    public GuildConfigurationBusinessLayer(IReplyDataLayer replyDataLayer)
    {
        _replyDataLayer = replyDataLayer;
    }

    public async Task<GuildConfiguration> GetGuildConfiguration(IGuild guild)
    {
        return await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
    }

    public async Task<bool> UpdateGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.UpdateGuildConfiguration(guild.Id, guild.Name);
        }

        return false;
    }

    public async Task<bool> DeleteGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.DeleteGuildConfiguration(guild.Id);
        }

        return true;
    }

    public async Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetEnableAvatarAnnouncements(guild.Id, isEnabled);
        }

        return false;
    }

    public async Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetEnableAvatarMentions(guild.Id, isEnabled);
        }

        return false;
    }

    public async Task<bool> SetLogChannel(IGuild guild, ulong? channelId)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id, guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetLogChannel(guild.Id, channelId);
        }

        return false;
    }
}
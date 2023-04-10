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
        return await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
    }

    public async Task<bool> UpdateGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.UpdateGuildConfiguration(guild.Id.ToString(), guild.Name);
        }

        return false;
    }

    public async Task<bool> DeleteGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.DeleteGuildConfiguration(guild.Id.ToString());
        }

        return true;
    }

    public async Task<bool> SetApprovedRole(IGuild guild, string roleId, bool setAllowed)
    {
        if (setAllowed)
        {
            return await _replyDataLayer.AddAllowedRoleId(guild.Id.ToString(), guild.Name, roleId);
        }
        return await _replyDataLayer.RemoveAllowedRoleId(guild.Id.ToString(), guild.Name, roleId);
    }

    public async Task<bool> SetAvatarAnnouncementEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetEnableAvatarAnnouncements(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetAvatarMentionEnabled(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetEnableAvatarMentions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetLogChannel(IGuild guild, string? channelId)
    {
        GuildConfiguration? config = await _replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _replyDataLayer.SetLogChannel(guild.Id.ToString(), channelId);
        }

        return false;
    }

    public async Task<bool> HasAdminRole(IGuild guild, IReadOnlyCollection<string> roleIds)
    {
        var config = await GetGuildConfiguration(guild);
        return config.AdminRoleIds.Any(roleIds.Contains);
    }
}
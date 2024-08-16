using Discord;
using Replybot.DataLayer;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public class GuildConfigurationBusinessLayer(IReplyDataLayer replyDataLayer) : IGuildConfigurationBusinessLayer
{
    public async Task<GuildConfiguration?> GetGuildConfiguration(IGuild? guild)
    {
        if (guild == null)
        {
            return null;
        }
        return await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
    }

    public async Task<bool> UpdateGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.UpdateGuildConfiguration(guild.Id.ToString(), guild.Name);
        }

        return false;
    }

    public async Task<bool> DeleteGuildConfiguration(IGuild guild)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.DeleteGuildConfiguration(guild.Id.ToString());
        }

        return true;
    }

    public async Task<bool> SetApprovedUsers(IGuild guild, List<string> userIds, bool setAllowed)
    {
        if (setAllowed)
        {
            return await replyDataLayer.AddAllowedUserIds(guild.Id.ToString(), guild.Name, userIds);
        }
        return await replyDataLayer.RemoveAllowedUserIds(guild.Id.ToString(), guild.Name, userIds);
    }

    public async Task<bool> SetIgnoreUsersForAvatarAnnouncements(IGuild guild, List<string> userIds, bool setAllowed)
    {
        if (setAllowed)
        {
            return await replyDataLayer.AddIgnoreAvatarChangesUserIds(guild.Id.ToString(), guild.Name, userIds);
        }
        return await replyDataLayer.RemoveIgnoreAvatarChangesUserIds(guild.Id.ToString(), guild.Name, userIds);
    }

    public async Task<bool> SetEnableAvatarAnnouncements(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableAvatarAnnouncements(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableAvatarMentions(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableAvatarMentions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableFixTweets(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableFixTweetReactions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableFixInstagram(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableFixInstagramReactions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableFixBluesky(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableFixBlueskyReactions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableFixTikTok(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableFixTikTokReactions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableFixReddit(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableFixRedditReactions(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableDefaultReplies(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableDefaultReplies(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetLogChannel(IGuild guild, string? channelId)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetLogChannel(guild.Id.ToString(), channelId);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingUserJoins(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingUserJoins(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingUserDepartures(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingUserDepartures(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingMessageEdits(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingMessageEdits(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingMessageDeletes(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingMessageDeletes(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingUserBans(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingUserBans(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableLoggingUserUnBans(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableLoggingUserUnBans(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> SetEnableWelcomeMessage(IGuild guild, bool isEnabled)
    {
        GuildConfiguration? config = await replyDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await replyDataLayer.SetEnableWelcomeMessage(guild.Id.ToString(), isEnabled);
        }

        return false;
    }

    public async Task<bool> CanUserAdmin(IGuild guild, IGuildUser user)
    {
        var config = await GetGuildConfiguration(guild);
        return config.AdminUserIds.Contains(user.Id.ToString());
    }
}
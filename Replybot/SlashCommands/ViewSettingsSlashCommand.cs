using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ViewSettingsSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        IDiscordFormatter discordFormatter)
    : InteractionModuleBase<SocketInteractionContext>
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("view-settings", "See the current settings for the bot.")]
    public async Task ViewSettings()
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }
        if (member.GuildPermissions.Administrator)
        {
            var guildConfig = await guildConfigurationBusinessLayer.GetGuildConfiguration(Context.Guild);

            if (guildConfig == null)
            {
                await RespondAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Oops!",
                    "There was a problem reading the configuration for this server. That shouldn't happen, so maybe try again later.",
                    Context.User));
                return;
            }

            var message = "";
            message += $"Default Replies: {GetEnabledText(guildConfig.EnableDefaultReplies)}\n";
            message += $"Avatar Announcements: {GetEnabledText(guildConfig.EnableAvatarAnnouncements)}\n";
            message += $"Mention User on Avatar Announcements: {GetEnabledText(guildConfig.EnableAvatarMentions)}\n";
            message += $"Log Channel: {(guildConfig.LogChannelId != null ? $"<#{guildConfig.LogChannelId}>" : "Not Set")}\n";
            message += $"Fix Tweet Reactions: {GetEnabledText(guildConfig.EnableFixTweetReactions)}\n";
            message += $"Fix Instagram Reactions: {GetEnabledText(guildConfig.EnableFixInstagramReactions)}\n";
            message += $"Fix Bluesky Reactions: {GetEnabledText(guildConfig.EnableFixBlueskyReactions)}\n";
            message += $"Bot Managers: {GetAdminUserDisplayText(guildConfig.AdminUserIds)} (+ any users with the Administrator permission)\n";

            await RespondAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"Settings for {Context.Guild.Name}",
                message,
                Context.User));
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }

    private static string GetAdminUserDisplayText(IEnumerable<string> adminUserIds)
    {
        return string.Join(", ", adminUserIds.Select(s => $"<@{s}>"));
    }

    private static string GetEnabledText(bool isEnabled)
    {
        return isEnabled ? "ON" : "OFF";
    }
}
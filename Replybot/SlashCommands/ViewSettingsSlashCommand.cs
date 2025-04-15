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

            var title = $"Settings for {Context.Guild.Name}";

            var message = "";
            message += MakeSectionTitle("Bot Behaviour", true);
            message += $"{GetEnabledText(guildConfig.EnableDefaultReplies)} Default Replies\n";
            message += $"{GetEnabledText(guildConfig.EnableAvatarAnnouncements)} Avatar Announcements\n";
            message += $"{GetEnabledText(guildConfig.EnableWelcomeMessage)} Member Welcome Message\n";
            message += $"{GetEnabledText(guildConfig.EnableDepartureMessage)} Member Departure Message\n";
            message += $"{GetEnabledText(guildConfig.EnableAvatarMentions)} Mention User on Avatar Announcements\n";
            message += $"{GetEnabledText(guildConfig.EnableRepeatLinkNotifications)} Recently Repeated Post Notifications\n";
            message += MakeSectionTitle("Link Preview Fixes (as reactions)");
            message += $"{GetEnabledText(guildConfig.EnableFixBlueskyReactions)} Bluesky\n";
            message += $"{GetEnabledText(guildConfig.EnableFixRedditReactions)} Reddit\n";
            message += $"{GetEnabledText(guildConfig.EnableFixTikTokReactions)} TikTok\n";
            message += $"{GetEnabledText(guildConfig.EnableFixTweetReactions)} Tweet\n";
            message += $"{GetEnabledText(guildConfig.EnableFixInstagramReactions)} Instagram\n";
            message += $"{GetEnabledText(guildConfig.EnableFixThreadsReactions)} Threads\n";
            message += MakeSectionTitle("Fortnite");
            message += $"{GetEnabledText(guildConfig.FortniteMapOnlyNamedLocations)} Map Command: Only Named Locations\n";
            message += MakeSectionTitle("Logging");
            message += $"Channel: {(guildConfig.LogChannelId != null ? $"<#{guildConfig.LogChannelId}>" : "Not Set")}\n";
            if (guildConfig.LogChannelId != null)
            {
                message += $"- {GetEnabledText(guildConfig.EnableLoggingUserJoins)} User Joins\n";
                message += $"- {GetEnabledText(guildConfig.EnableLoggingUserDepartures)} User Departures\n";
                message += $"- {GetEnabledText(guildConfig.EnableLoggingUserBans)} User Bans\n";
                message += $"- {GetEnabledText(guildConfig.EnableLoggingUserUnBans)} User Unbans\n";
                message += $"- {GetEnabledText(guildConfig.EnableLoggingMessageEdits)} Message Edits\n";
                message += $"- {GetEnabledText(guildConfig.EnableLoggingMessageDeletes)} Message Deletes\n";
            }
            message += MakeSectionTitle("Bot Managers in this Server");
            message += $"{GetAdminUserDisplayText(guildConfig.AdminUserIds)} (& any users with the Administrator permission)\n";

            await RespondAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter(title, message, Context.User));
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
        return isEnabled ? "✅" : "❌";
    }

    private static string MakeSectionTitle(string text, bool isFirst = false)
    {
        return $"{(isFirst ? "" : "\n")}**{text}**\n";
    }
}
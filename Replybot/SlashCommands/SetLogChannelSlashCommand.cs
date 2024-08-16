using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class SetLogChannelSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        RoleHelper roleHelper)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-logging", "Sets up a channel to use for logging activities in this Discord.")]
    public async Task SetLogChannel(
        [Summary("logging_enabled", "The on-off switch for logging. Set to false to clear the logging channel and disable all logging.")] bool? enableLogging = null,
        [Summary("channel", "The channel where you'd like logs to be stored. Leave it empty to turn off logging.")] IGuildChannel? channel = null,
        [Summary("log_joins_enabled", "Whether or not a user joining the server should be logged.")] bool? logUserJoins = null,
        [Summary("log_departures_enabled", "Whether or not a user leaving the server should be logged.")] bool? logUserDepartures = null,
        [Summary("log_edits_enabled", "Whether or not a user editing a message should be logged.")] bool? logMessageEdits = null,
        [Summary("log_deletes_enabled", "Whether or not a user deleting a message should be logged.")] bool? logMessageDeletes = null,
        [Summary("log_bans_enabled", "Whether or not a user being banned should be logged.")] bool? logUserBans = null,
        [Summary("log_unbans_enabled", "Whether or not a user being unbanned should be logged.")] bool? logUserUnBans = null
    )
    {
        if (enableLogging != null && enableLogging.Value && channel == null)
        {
            await RespondAsync("You must specify a channel to enable logging.");
            return;
        }

        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await roleHelper.CanAdministrate(Context.Guild, member))
        {
            var reportText = new List<string>();
            if (enableLogging != null && !enableLogging.Value)
            {
                //turning off logging, so clear all logging settings
                await guildConfigurationBusinessLayer.SetLogChannel(Context.Guild, null);
                await guildConfigurationBusinessLayer.SetEnableLoggingUserJoins(Context.Guild, false);
                await guildConfigurationBusinessLayer.SetEnableLoggingUserDepartures(Context.Guild, false);
                await guildConfigurationBusinessLayer.SetEnableLoggingUserBans(Context.Guild, false);
                await guildConfigurationBusinessLayer.SetEnableLoggingUserUnBans(Context.Guild, false);
                await guildConfigurationBusinessLayer.SetEnableLoggingMessageEdits(Context.Guild, false);
                await guildConfigurationBusinessLayer.SetEnableLoggingMessageDeletes(Context.Guild, false);
                reportText.Add("All logging has been turned off.");
            }
            else
            {
                //handle individual settings, which can all be specified individually
                if (channel != null)
                {
                    await guildConfigurationBusinessLayer.SetLogChannel(Context.Guild, channel.Id.ToString());
                    reportText.Add($"Activity logs will now be posted in <#{channel.Id}>.");
                }
                if (logUserJoins != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingUserJoins(Context.Guild, logUserJoins.Value);
                    reportText.Add($"User Joins **{(logUserJoins.Value ? "will be" : "will not be")}** logged.");
                }
                if (logUserDepartures != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingUserDepartures(Context.Guild, logUserDepartures.Value);
                    reportText.Add($"User Departures **{(logUserDepartures.Value ? "will be" : "will not be")}** logged.");
                }
                if (logMessageEdits != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingMessageEdits(Context.Guild, logMessageEdits.Value);
                    reportText.Add($"Message Edits **{(logMessageEdits.Value ? "will be" : "will not be")}** logged.");
                }
                if (logMessageDeletes != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingMessageDeletes(Context.Guild, logMessageDeletes.Value);
                    reportText.Add($"Message Deletes **{(logMessageDeletes.Value ? "will be" : "will not be")}** logged.");
                }
                if (logUserBans != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingUserBans(Context.Guild, logUserBans.Value);
                    reportText.Add($"User Bans **{(logUserBans.Value ? "will be" : "will not be")}** logged.");
                }
                if (logUserUnBans != null)
                {
                    await guildConfigurationBusinessLayer.SetEnableLoggingUserUnBans(Context.Guild, logUserUnBans.Value);
                    reportText.Add($"User Unbans **{(logUserUnBans.Value ? "will be" : "will not be")}** logged.");
                }
            }

            var text = $"Sure thing! {string.Join("\n", reportText)}";

            await RespondAsync(text);
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
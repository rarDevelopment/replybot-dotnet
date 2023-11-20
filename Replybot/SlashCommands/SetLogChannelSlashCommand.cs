using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class SetLogChannelSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        RoleHelper roleHelper)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-logs-channel", "Sets the channel to use for logging activities in this Discord.")]
    public async Task SetLogChannel(
        [Summary("channel", "The channel where you'd like logs to be stored. Leave it empty to turn off logging.")] IGuildChannel? channel = null)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await roleHelper.CanAdministrate(Context.Guild, member))
        {
            var success = await guildConfigurationBusinessLayer.SetLogChannel(Context.Guild, channel?.Id.ToString());
            if (success)
            {
                var text = "Consider it done! Activity will no longer be logged.";
                if (channel != null)
                {
                    text = $"Consider it done! Activity logs will now be posted in <#{channel.Id}>.";
                }
                await RespondAsync(text);
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
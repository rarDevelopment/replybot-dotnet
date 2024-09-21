using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleDepartureMessageSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        RoleHelper roleHelper)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-departure-message", "Set fix Departure Messages on or off (true or false).")]
    public async Task ToggleDepartureMessage(
        [Summary("is_enabled", "True for ON, False for OFF")] bool isEnabled)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await roleHelper.CanAdministrate(Context.Guild, member))
        {
            var success = await guildConfigurationBusinessLayer.SetEnableDepartureMessage(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! Departure Messages are now {(isEnabled ? "ON" : "OFF")}.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
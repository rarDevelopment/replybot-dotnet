using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleFortniteMapOnlyNamedLocationsSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        RoleHelper roleHelper)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-fortnite-named-locations", "Set whether or not the Fortnite Map command should only used named locations.")]
    public async Task ToggleFortniteMapOnlyNamedLocations(
        [Summary("only_named", "True for Only Named Locations, False for All Locations")] bool isEnabled)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await roleHelper.CanAdministrate(Context.Guild, member))
        {
            var success = await guildConfigurationBusinessLayer.SetFortniteMapOnlyNamedLocations(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! The Fortnite Map command will now pull from {(isEnabled ? "ONLY NAMED" : "ALL")} locations.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
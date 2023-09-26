using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleDefaultRepliesSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly RoleHelper _roleHelper;

    public ToggleDefaultRepliesSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer, RoleHelper roleHelper)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _roleHelper = roleHelper;
    }

    [SlashCommand("set-default-replies", "Set default replies to on or off (true or false).")]
    public async Task ToggleDefaultReplies(
        [Summary("is_enabled", "True for ON, False for OFF")] bool isEnabled)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await _roleHelper.CanAdministrate(Context.Guild, member))
        {
            var success = await _guildConfigurationBusinessLayer.SetEnableDefaultReplies(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! Default replies are now {(isEnabled ? "ON" : "OFF")}.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
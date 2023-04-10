using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleAvatarAnnounceSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly RoleHelper _roleHelper;

    public ToggleAvatarAnnounceSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer, RoleHelper roleHelper)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _roleHelper = roleHelper;
    }
    
    [SlashCommand("set-avatar-announcement", "Set avatar announcements to on or off (true or false).")]
    public async Task ToggleAvatarAnnounce(
        [Summary("is_enabled", "True for Enabled, False for Disabled")] bool isEnabled)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }

        if (await _roleHelper.CanAdministrate(Context.Guild, member))
        {
            var success = await _guildConfigurationBusinessLayer.SetAvatarAnnouncementEnabled(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! Avatar announcements are now {(isEnabled ? "ON" : "OFF")}.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
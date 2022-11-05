using Replybot.BusinessLayer;

namespace Replybot.Commands
{
    public class ToggleAvatarMentionSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;

        public ToggleAvatarMentionSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
        {
            _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        }

        [SlashCommand("set_avatar_mention", "Set avatar change mentions to on or off (true or false).")]
        public async Task Toggle(
            [Summary("is_enabled", "True for Enabled, False for Disabled")] bool isEnabled)
        {
            var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
            if (member == null)
            {
                await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
                return;
            }
            if (member.GuildPermissions.Administrator)
            {
                var success = await _guildConfigurationBusinessLayer.SetAvatarMentionEnabled(Context.Guild, isEnabled);
                if (success)
                {
                    await RespondAsync("Consider it done! Avatar mentions are now ");
                    return;
                }

                await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            }

            await RespondAsync("You aren't allowed to do that!");
        }
    }
}

using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.Commands
{
    public class ViewSettingsSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
        private readonly IDiscordFormatter _discordFormatter;

        public ViewSettingsSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer, IDiscordFormatter discordFormatter)
        {
            _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
            _discordFormatter = discordFormatter;
        }

        [SlashCommand("view_settings", "See the current settings for the bot (admins only).")]
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
                var guildConfig = await _guildConfigurationBusinessLayer.GetGuildConfiguration(Context.Guild);

                var message = $"Avatar Announcements: {GetEnabledText(guildConfig.EnableAvatarAnnouncements)}\n";
                message += $"Mention User on Avatar Announcements: {GetEnabledText(guildConfig.EnableAvatarMentions)}\n";
                message += $"Log Channel: {(guildConfig.LogChannelId != null ? $"<#{guildConfig.LogChannelId}>" : "Not Set")}\n";

                await RespondAsync(embed: _discordFormatter.BuildRegularEmbed($"Settings for {Context.Guild.Name}",
                    message,
                    Context.User));
                return;
            }

            await RespondAsync("You aren't allowed to do that!");
        }

        private static string GetEnabledText(bool isEnabled)
        {
            return isEnabled ? "ON" : "OFF";
        }
    }
}

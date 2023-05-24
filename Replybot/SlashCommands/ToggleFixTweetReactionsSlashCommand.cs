using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleFixTweetReactionsSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly RoleHelper _roleHelper;

    public ToggleFixTweetReactionsSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer, RoleHelper roleHelper)
    {
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _roleHelper = roleHelper;
    }
    
    [SlashCommand("set-fix-tweet-reactions", "Set fix tweet reactions on or off (true or false).")]
    public async Task ToggleAutoFixTweets(
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
            var success = await _guildConfigurationBusinessLayer.SetEnableAutoFixTweets(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! Fix tweet reactions, which allow you to react to convert twitter.com links to fxtwitter.com (or vice versa) are now {(isEnabled ? "ON" : "OFF")}.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
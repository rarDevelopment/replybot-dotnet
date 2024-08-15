﻿using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class ToggleFixTweetReactionsSlashCommand(IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        RoleHelper roleHelper)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-fix-tweet-reactions", "Set fix tweet reactions on or off (true or false).")]
    public async Task ToggleFixTweets(
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
            var success = await guildConfigurationBusinessLayer.SetEnableFixTweets(Context.Guild, isEnabled);
            if (success)
            {
                await RespondAsync($"Consider it done! Fix tweet reactions, which allow you to react to convert twitter.com links to vxtwitter.com (or vice versa) are now {(isEnabled ? "ON" : "OFF")}.");
                return;
            }

            await RespondAsync("Hmm, I wasn't able to do that. There was an issue, sorry.");
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }
}
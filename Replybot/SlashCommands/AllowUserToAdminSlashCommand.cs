using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class AllowUserToAdminSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly RoleHelper _roleHelper;
    private readonly IDiscordFormatter _discordFormatter;

    public AllowUserToAdminSlashCommand(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        RoleHelper roleHelper,
        IDiscordFormatter discordFormatter)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _roleHelper = roleHelper;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("allow-user-to-admin", "Set whether or not the specified user can manage the bot.")]
    public async Task AllowRoleToAdmin(
        [Summary("user", "The name of the user to allow to manage the bot")] IUser userToSet,
        [Summary("set-allowed", "Whether or not the user should be allowed to manage the bot")] bool setAllowed
        )
    {
        await DeferAsync();

        if (Context.User is not IGuildUser requestingUser)
        {
            await FollowupAsync(embed:
                _discordFormatter.BuildErrorEmbed("Invalid Action",
                    "Sorry, you need to be a valid user in a valid server to use this bot.",
                    Context.User));
            return;
        }

        if (!await _roleHelper.CanAdministrate(Context.Guild, requestingUser))
        {
            await FollowupAsync(embed:
                _discordFormatter.BuildErrorEmbed("Insufficient Permissions",
                    "Sorry, you do not have permission to manage the bot.",
                    Context.User));
            return;
        }

        var config = await _configurationBusinessLayer.GetGuildConfiguration(Context.Guild);
        if (config == null)
        {
            await RespondAsync(embed: _discordFormatter.BuildErrorEmbed("Oops!",
                "There was a problem reading the configuration for this server. That shouldn't happen, so maybe try again later.",
                Context.User));
            return;
        }

        var isUserAllowed = config.AdminUserIds.Contains(userToSet.Id.ToString());

        if (setAllowed)
        {
            if (isUserAllowed)
            {
                await FollowupAsync(embed:
                    _discordFormatter.BuildErrorEmbed("User Already Allowed",
                        "Sorry, this user is already allowed to manage the bot.",
                        Context.User));
                return;
            }
            await _configurationBusinessLayer.SetApprovedUsers(Context.Guild, new List<string> { userToSet.Id.ToString() }, true);
        }
        else
        {
            if (!isUserAllowed)
            {
                await FollowupAsync(embed:
                    _discordFormatter.BuildErrorEmbed("User Is Already Not Allowed",
                        "Sorry, this user is not allowed to manage the bot so there's nothing to change.",
                        Context.User));
                return;
            }
            await _configurationBusinessLayer.SetApprovedUsers(Context.Guild, new List<string> { userToSet.Id.ToString() }, false);
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Configuring Bot Permissions",
            $"The user {userToSet.Mention} can now {(setAllowed ? "" : "no longer")} manage the bot.",
            Context.User));
    }
}
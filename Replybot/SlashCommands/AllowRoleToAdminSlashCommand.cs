using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class AllowRoleToAdminSlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly RoleHelper _roleHelper;
    private readonly IDiscordFormatter _discordFormatter;

    public AllowRoleToAdminSlashCommand(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        RoleHelper roleHelper,
        IDiscordFormatter discordFormatter)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _roleHelper = roleHelper;
        _discordFormatter = discordFormatter;
    }

    [SlashCommand("allow-role-to-admin", "Set whether or not the specified role can use the bot to create roles and channels.")]
    public async Task AllowRoleToAdmin(
        [Summary("role", "The name of the role to manage")] IRole roleToSet,
        [Summary("set-allowed", "Whether or not the role should be allowed to use the bot to create roles and related channels")] bool setAllowed
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
        var guildRoles = Context.Guild.Roles.Where(r => config.AdminRoleIds.Contains(r.Id.ToString()));
        var isRoleAllowed = guildRoles.Contains(roleToSet);

        if (setAllowed)
        {
            if (isRoleAllowed)
            {
                await FollowupAsync(embed:
                    _discordFormatter.BuildErrorEmbed("Role Already Allowed",
                        "Sorry, this role is already allowed to manage the bot.",
                        Context.User));
                return;
            }
            await _configurationBusinessLayer.SetApprovedRole(Context.Guild, roleToSet.Id.ToString(), true);
        }
        else
        {
            if (!isRoleAllowed)
            {
                await FollowupAsync(embed:
                    _discordFormatter.BuildErrorEmbed("Role Is Already Not Allowed",
                        "Sorry, this role is not allowed to manage the bot so there's nothing to change.",
                        Context.User));
                return;
            }
            await _configurationBusinessLayer.SetApprovedRole(Context.Guild, roleToSet.Id.ToString(), false);
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Configuring Bot Permissions",
            $"The role {roleToSet.Mention} can now {(setAllowed ? "" : "no longer")} manage the bot.",
            Context.User));
    }
}
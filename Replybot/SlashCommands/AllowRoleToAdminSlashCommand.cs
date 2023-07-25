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

    [SlashCommand("allow-admin-users-in-role", "Allow the users currently in the specified role to administrate the bot.")]
    public async Task AllowUsersInRoleToAdmin(
        [Summary("role", "The name of the role")] IRole roleToSet,
        [Summary("set-allowed", "Whether to allow or disallow all users currently in this role to administrate the bot")] bool setAllowed
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

        var usersInRole = Context.Guild.Users.Where(u => u.Roles.Any(r => r.Id == roleToSet.Id));

        var usersToProcess = new List<string>();
        var usersAlreadyProcessed = new List<string>();

        if (setAllowed)
        {
            foreach (var user in usersInRole)
            {
                if (config.AdminUserIds.Contains(user.Id.ToString()))
                {
                    usersAlreadyProcessed.Add(user.Id.ToString());
                }
                else
                {
                    usersToProcess.Add(user.Id.ToString());
                }
            }
        }
        else
        {
            foreach (var user in usersInRole)
            {
                if (!config.AdminUserIds.Contains(user.Id.ToString()))
                {
                    usersAlreadyProcessed.Add(user.Id.ToString());
                }
                else
                {
                    usersToProcess.Add(user.Id.ToString());
                }
            }
        }

        var isSuccess = await _configurationBusinessLayer.SetApprovedUsers(Context.Guild, usersToProcess, setAllowed);

        if (isSuccess)
        {
            var usersProcessed = Context.Guild.Users.Where(u => usersToProcess.Contains(u.Id.ToString())).ToList();
            var usersNotProcessed = Context.Guild.Users.Where(u => usersAlreadyProcessed.Contains(u.Id.ToString())).ToList();

            var embedFieldBuilders = new List<EmbedFieldBuilder>();

            if (usersProcessed.Any())
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = $"Users {(setAllowed ? "Allowed" : "Removed")}",
                    Value = usersProcessed.Any() ? string.Join(", ", usersProcessed.Select(u => u.Mention)) : "None",
                    IsInline = false
                });
            }

            if (usersNotProcessed.Any())
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = $"Users Not Processed (possibly were already {(setAllowed ? "Allowed" : "Removed")})",
                    Value = usersNotProcessed.Any() ? string.Join(", ", usersNotProcessed.Select(u => u.Mention)) : "None",
                    IsInline = false
                });
            }

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Configuring Bot Permissions",
                $"**NOTE:** Users added to this role later will still need to be manually {(setAllowed ? "Allowed" : "Removed")} (or you can run this command again).",
                Context.User, embedFieldBuilders));
        }
        else
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Configuring Bot Permissions",
                "The command failed. Please try again later, or there might be an issue with your request.",
                Context.User));
        }
    }
}
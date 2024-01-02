using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.SlashCommands;

public class IgnoreAvatarAnnouncementForUser(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        RoleHelper roleHelper,
        IDiscordFormatter discordFormatter)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-user-avatar-ignore",
        "Set avatar announcements for the user to be ignored (True) or not (False).")]
    public async Task SetAvatarAnnouncementForUser(
        [Summary("user", "The user to ignore")]
        IUser user,
        [Summary("set-announcement-ignore", "Whether or not to ignore the user's avatar changes")]
        bool setIgnore
    )
    {
        await DeferAsync();

        if (Context.User is not IGuildUser requestingUser)
        {
            await FollowupAsync(embed:
                discordFormatter.BuildErrorEmbedWithUserFooter("Invalid Action",
                    "Sorry, you need to be a valid user in a valid server to use this bot.",
                    Context.User));
            return;
        }

        if (!await roleHelper.CanAdministrate(Context.Guild, requestingUser))
        {
            await FollowupAsync(embed:
                discordFormatter.BuildErrorEmbedWithUserFooter("Insufficient Permissions",
                    "Sorry, you do not have permission to manage the bot.",
                    Context.User));
            return;
        }

        var config = await configurationBusinessLayer.GetGuildConfiguration(Context.Guild);
        if (config == null)
        {
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Oops!",
                "There was a problem reading the configuration for this server. That shouldn't happen, so maybe try again later.",
                Context.User));
            return;
        }

        var isUserAlreadyIgnored = config.IgnoreAvatarChangesUserIds.Contains(user.Id.ToString());

        if (isUserAlreadyIgnored && setIgnore || !isUserAlreadyIgnored && !setIgnore)
        {
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter(
                "User's Avatar Announcements are Already Set This Way",
                "This user's avatar announcements are already configured as you requested.",
                Context.User));
            return;
        }

        var userIdsToProcess = new List<string> { user.Id.ToString() };
        var isSuccess = await configurationBusinessLayer.SetIgnoreUsersForAvatarAnnouncements(Context.Guild, userIdsToProcess, setIgnore);

        if (isSuccess)
        {
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter("Configuring Individual User Avatar Announcements",
                $"{user.Mention}'s avatar announcements will now be **{(setIgnore ? "IGNORED" : "ALLOWED")}**.",
                Context.User));
        }
        else
        {
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Configuring Individual User Avatar Announcements",
                "The command failed. Please try again later, or there might be an issue with your request.",
                Context.User));
        }
    }
}
using Replybot.BusinessLayer;

namespace Replybot;

public class RoleHelper(IGuildConfigurationBusinessLayer configurationBusinessLayer)
{
    public async Task<bool> CanAdministrate(IGuild guild, IGuildUser guildUser)
    {
        return guildUser.GuildPermissions.Administrator
               || guildUser.GuildPermissions.ManageRoles
               || await configurationBusinessLayer.CanUserAdmin(guild, guildUser);
    }
}
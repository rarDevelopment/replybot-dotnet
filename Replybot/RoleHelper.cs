using Replybot.BusinessLayer;

namespace Replybot;

public class RoleHelper
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;

    public RoleHelper(IGuildConfigurationBusinessLayer configurationBusinessLayer)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
    }
    public async Task<bool> CanAdministrate(IGuild guild, IGuildUser guildUser)
    {
        return guildUser.GuildPermissions.Administrator
               || guildUser.GuildPermissions.ManageRoles
               || await _configurationBusinessLayer.CanUserAdmin(guild, guildUser);
    }
}
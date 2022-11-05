﻿using Replybot.DataLayer.SchemaModels;
using Replybot.Models;

namespace Replybot.DataLayer
{
    public interface IResponseDataLayer
    {
        IList<TriggerResponse>? GetDefaultResponses();
        Task<IList<TriggerResponse>> GetResponsesForGuild(ulong guildId);
        Task<GuildConfiguration> GetConfigurationForGuild(ulong guildId, string guildName);
        Task<bool> SetEnableAvatarAnnouncements(ulong guildId, bool isEnabled);
        Task<bool> SetEnableAvatarMentions(ulong guildId, bool isEnabled);
    }
}

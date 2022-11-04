using Replybot.DataLayer.SchemaModels;
using Replybot.Models;

namespace Replybot.DataLayer
{
    public interface IResponseDataLayer
    {
        Task<IList<TriggerResponse>> GetResponsesForGuild(ulong guildId);
        IList<TriggerResponse>? GetDefaultResponses();
    }
}

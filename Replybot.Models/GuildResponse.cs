namespace Replybot.Models
{
    public class GuildResponse
    {
        public GuildResponse(ulong guildId, TriggerResponse[] responses)
        {
            GuildId = guildId;
            Responses = responses;
        }

        public ulong GuildId { get; set; }
        public TriggerResponse[] Responses { get; set; }
    }
}

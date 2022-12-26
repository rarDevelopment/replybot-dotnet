namespace Replybot.Models
{
    public class GuildConfiguration
    {
        public ulong GuildId { get; set; }
        public string? GuildName { get; set; }
        public bool EnableAvatarAnnouncements { get; set; }
        public bool EnableAvatarMentions { get; set; }
        public ulong? LogChannelId { get; set; }
    }
}

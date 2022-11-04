namespace Replybot.DataLayer.SchemaModels
{
    public class BotConfigurationEntity
    {
        public ulong GuildId { get; init; }
        public string? GuildName { get; init; }
        public bool EnableAvatarAnnouncements { get; init; }
        public bool EnableAvatarMentions { get; init; }
    }
}

using Replybot.Models;

namespace Replybot
{
    public class LogMessageBuilder
    {
        private readonly DiscordSettings _discordSettings;
        private const string TruncationString = "[...]";

        public LogMessageBuilder(DiscordSettings discordSettings)
        {
            _discordSettings = discordSettings;
        }

        public EmbedBuilder CreateEmbedBuilder(string title, string description, IMessage message)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(title);

            if (message.Embeds.Any())
            {
                var embed = message.Embeds.First();
                if (embed.Image.HasValue)
                {
                    embedBuilder.WithImageUrl(embed.Image.Value.Url);
                }

                if (!string.IsNullOrEmpty(embed.Description))
                {
                    embedBuilder.WithDescription(embed.Description);
                }
            }

            if (message.Attachments.Any())
            {
                embedBuilder.AddField("Attachments",
                    string.Join(", ",
                        message.Attachments.Select((attachment, index) => $"[Attachment {index + 1}]({attachment.Url})")));
            }

            if (string.IsNullOrEmpty(embedBuilder.Description))
            {
                embedBuilder.WithDescription($"{description}: {message.Content}");
            }

            if (embedBuilder.Description.Length > _discordSettings.MaxCharacters)
            {
                var maxCharacters = _discordSettings.MaxCharacters - TruncationString.Length;
                embedBuilder.WithDescription($"{embedBuilder.Description[..maxCharacters]}{TruncationString}");
            }

            return embedBuilder;
        }

        public EmbedBuilder CreateEmbedBuilderWithFields(string title, string description,
            Dictionary<string, string> messagesToEmbed)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description);

            embedBuilder.WithFields(messagesToEmbed.Select(m => new EmbedFieldBuilder
            {
                Name = m.Key,
                Value = TruncateIfLongerThanMaxCharacters(m.Value, _discordSettings.MaxCharacters, TruncationString),
                IsInline = false
            }));

            return embedBuilder;
        }

        private static string TruncateIfLongerThanMaxCharacters(string messageContent, int maxCharacters, string truncationString)
        {
            if (messageContent.Length <= maxCharacters)
            {
                return messageContent;
            }
            var maxCharactersAllowed = maxCharacters - truncationString.Length;
            return $"{messageContent[..maxCharactersAllowed]}{truncationString}";
        }
    }
}

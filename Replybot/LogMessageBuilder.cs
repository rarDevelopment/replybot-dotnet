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

        public EmbedBuilder CreateEmbedBuilder(IMessage message, string title)
        {
            var embedBuilder = new EmbedBuilder().WithTitle(title);

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
                embedBuilder.WithDescription(message.Content);
            }

            if (embedBuilder.Description.Length > _discordSettings.MaxCharacters)
            {
                var maxCharacters = _discordSettings.MaxCharacters - TruncationString.Length;
                embedBuilder.WithDescription($"{embedBuilder.Description[..maxCharacters]}{TruncationString}");
            }

            return embedBuilder;
        }

        public EmbedBuilder CreateEmbedBuilderWithFields(IMessage newMessage, string oldMessageContent, string title)
        {
            var embedBuilder = new EmbedBuilder().WithTitle(title);

            var original = new EmbedFieldBuilder
            {
                Name = "Original:",
                Value = TruncateIfLongerThanMaxCharacters(oldMessageContent, _discordSettings.MaxCharacters, TruncationString),
                IsInline = false
            };

            var updated = new EmbedFieldBuilder
            {
                Name = "Edited:",
                Value = TruncateIfLongerThanMaxCharacters(newMessage.Content, _discordSettings.MaxCharacters, TruncationString),
                IsInline = false
            };

            embedBuilder.WithFields(original, updated);

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

namespace Replybot.TextCommands
{
    public class FixTwitterCommand
    {
        public async Task<(string fixedTwitterMessage, MessageReference messageToReplyTo)?> GetFixedTwitterMessage(ISocketMessageChannel channel, SocketMessage message)
        {
            var messageToFix = message;

            // ReSharper disable once UseNullPropagation
            if (message.Reference != null)
            {
                var messageReferenceId = message.Reference.MessageId.GetValueOrDefault(default);
                if (messageReferenceId == default)
                {
                    return (FixTwitterUrl(message), new MessageReference(message.Id));
                }

                var messageReferenced = await message.Channel.GetMessageAsync(messageReferenceId);
                if (messageReferenced is not SocketMessage referencedSocketMessage)
                {
                    return ("I couldn't read that message for some reason, sorry!", new MessageReference(message.Id));
                }

                messageToFix = referencedSocketMessage;
            }

            if (!DoesMessageContainTwitterUrl(messageToFix))
            {
                return ("I don't think there's a twitter link there.", new MessageReference(message.Id));
            }

            var fixedMessage = FixTwitterUrl(messageToFix);
            return (fixedMessage, new MessageReference(messageToFix.Id));
        }

        private static bool DoesMessageContainTwitterUrl(IMessage messageReferenced)
        {
            return messageReferenced.Content.Contains("twitter.com", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string FixTwitterUrl(IMessage messageToFix)
        {
            return messageToFix.Content.Replace("twitter.com", "fxtwitter.com");
        }
    }
}

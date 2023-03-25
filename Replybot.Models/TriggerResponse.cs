namespace Replybot.Models
{
    public class TriggerResponse
    {
        public TriggerResponse(string[] triggers,
            string[]? responses,
            UserResponse[]? userResponses,
            bool mentionAuthor,
            bool requiresBotName,
            string[]? reactions)
        {
            Triggers = triggers;
            Responses = responses;
            UserResponses = userResponses;
            MentionAuthor = mentionAuthor;
            RequiresBotName = requiresBotName;
            Reactions = reactions;
        }

        public string[] Triggers { get; set; }
        public string[]? Responses { get; set; }
        public UserResponse[]? UserResponses { get; set; }
        public bool MentionAuthor { get; set; }
        public bool RequiresBotName { get; set; }
        public string[]? Reactions { get; set; }
    }
}

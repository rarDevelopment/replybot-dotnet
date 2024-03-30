using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class PollCommand(BotSettings botSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private readonly IReadOnlyList<string> _pollOptionsAlphabet = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹" };
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"poll (?<{SearchTermKey}>(.)*)";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               Regex.IsMatch(replyCriteria.MessageText,
                   TriggerRegexPattern,
                   RegexOptions.IgnoreCase,
                   _matchTimeout);
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        var embedWithReactions = BuildPollEmbed(message);

        return Task.FromResult(new CommandResponse
        {
            Embed = embedWithReactions.Embed,
            Reactions = embedWithReactions.ReactionEmotes,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }

    private PollEmbed BuildPollEmbed(SocketMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

        if (match.Success)
        {
            var matchedGroup = match.Groups[SearchTermKey];
            var messageWithoutTrigger = matchedGroup.Value;

            if (messageWithoutTrigger.Length <= 0)
            {
                return new PollEmbed(discordFormatter.BuildErrorEmbed("Error Making Poll",
                    "You need at least two answers in your poll"));
            }

            var splitArgs = messageWithoutTrigger.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

            if (splitArgs.Count <= 2)
            {
                return new PollEmbed(discordFormatter.BuildErrorEmbed("Error Making Poll",
                    "You need at least two answers in your poll"));
            }

            var question = splitArgs[0];
            var answers = splitArgs.Skip(1).ToList(); //take only the answers

            if (answers.Count > _pollOptionsAlphabet.Count)
            {
                return new PollEmbed(discordFormatter.BuildErrorEmbed("Error Making Poll",
                        $"You can't have more than {_pollOptionsAlphabet.Count} answers. Nobody is going to read a poll that long anyway 😌"));
            }

            var reactions = answers.Select((_, index) => _pollOptionsAlphabet[index]).ToList();
            var answersToDisplay = answers.Select((answer, index) => $"{reactions[index]} {answer}").ToList();

            var messageToDisplay =
                $"{string.Join("\n", answersToDisplay)}\n\n(Note: The poll feature has been deprecated and will be removed when Discord official polls roll out to all servers)";

            var pollEmbed = discordFormatter.BuildRegularEmbedWithUserFooter(question, messageToDisplay, message.Author);
            var reactionEmotes = reactions.Select(e => new Emoji(e)).ToList();

            return new PollEmbed(pollEmbed, reactionEmotes);
        }

        logger.Log(LogLevel.Error, $"Error in PollCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        var errorEmbed = discordFormatter.BuildErrorEmbedWithUserFooter("Error Defining Word",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
        return new PollEmbed(errorEmbed);
    }
}
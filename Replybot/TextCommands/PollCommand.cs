using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class PollCommand : ITextCommand
{
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IReadOnlyList<string> _pollOptionsAlphabet = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹" };
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"poll (?<{SearchTermKey}>(.)*)";
    private readonly TimeSpan _matchTimeout;
    private readonly ILogger<DiscordBot> _logger;

    public PollCommand(BotSettings botSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _discordFormatter = discordFormatter;
        _logger = logger;
        _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    }

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
        var messageWithoutBotName = KeywordHandler.RemoveBotName(message.Content);
        var match = Regex.Match(messageWithoutBotName, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

        if (match.Success)
        {
            var matchedGroup = match.Groups[SearchTermKey];
            var messageWithoutTrigger = matchedGroup.Value;

            if (messageWithoutTrigger.Length <= 0)
            {
                return new PollEmbed(_discordFormatter.BuildErrorEmbed("Error Making Poll",
                    "You need at least two answers in your poll"));
            }

            var splitArgs = messageWithoutTrigger.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

            if (splitArgs.Count <= 2)
            {
                return new PollEmbed(_discordFormatter.BuildErrorEmbed("Error Making Poll",
                    "You need at least two answers in your poll"));
            }

            var question = splitArgs[0];
            var answers = splitArgs.Skip(1).ToList(); //take only the answers

            if (answers.Count > _pollOptionsAlphabet.Count)
            {
                return new PollEmbed(_discordFormatter.BuildErrorEmbed("Error Making Poll",
                        $"You can't have more than {_pollOptionsAlphabet.Count} answers. Nobody is going to read a poll that long anyway 😌"));
            }

            var reactions = answers.Select((_, index) => _pollOptionsAlphabet[index]).ToList();
            var answersToDisplay = answers.Select((answer, index) => $"{reactions[index]} {answer}").ToList();

            var pollEmbed = _discordFormatter.BuildRegularEmbedWithUserFooter(question, string.Join("\n", answersToDisplay), message.Author);
            var reactionEmotes = reactions.Select(e => new Emoji(e)).ToList();

            return new PollEmbed(pollEmbed, reactionEmotes);
        }

        _logger.Log(LogLevel.Error, $"Error in PollCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        var errorEmbed = _discordFormatter.BuildErrorEmbedWithUserFooter("Error Defining Word",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
        return new PollEmbed(errorEmbed);
    }
}
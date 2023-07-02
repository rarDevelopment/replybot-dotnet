using System.Globalization;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;

namespace Replybot.TextCommands;

public class PollCommand
{
    private readonly KeywordHandler _keywordHandler;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IReadOnlyList<string> _pollOptionsAlphabet = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹" };

    public PollCommand(KeywordHandler keywordHandler, IDiscordFormatter discordFormatter)
    {
        _keywordHandler = keywordHandler;
        _discordFormatter = discordFormatter;
    }

    public (Embed? pollEmbed, IReadOnlyList<IEmote>? reactionEmotes) BuildPollEmbed(SocketMessage message)
    {
        var messageContent = message.Content;
        var messageWithoutBotName = _keywordHandler.RemoveBotName(messageContent);
        var messageWithoutTrigger = RemoveTriggerFromMessage(messageWithoutBotName, "poll");

        if (messageWithoutTrigger.Length == 0)
        {
            return (_discordFormatter.BuildErrorEmbed("Error Making Poll",
                "You need at least two answers in your poll"), null);
        }

        var splitArgs = messageWithoutTrigger.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

        if (splitArgs.Count <= 2)
        {
            return (_discordFormatter.BuildErrorEmbed("Error Making Poll",
                "You need at least two answers in your poll"), null);
        }

        var question = splitArgs[0];
        var answers = splitArgs.Skip(1).ToList(); //take only the answers

        if (answers.Count > _pollOptionsAlphabet.Count)
        {
            return (_discordFormatter.BuildErrorEmbed("Error Making Poll",
                    $"You can't have more than {_pollOptionsAlphabet.Count} answers. Nobody is going to read a poll that long anyway 😌"),
                null);
        }

        var reactions = answers.Select((_, index) => _pollOptionsAlphabet[index]).ToList();
        var answersToDisplay = answers.Select((answer, index) => $"{reactions[index]} {answer}").ToList();

        var pollEmbed = _discordFormatter.BuildRegularEmbed(question, string.Join("\n", answersToDisplay), message.Author);
        var reactionEmotes = reactions.Select(e => new Emoji(e)).ToList();

        return (pollEmbed, reactionEmotes);
    }

    private static string RemoveTriggerFromMessage(string message, string trigger)
    {
        var firstIndexOfTrigger = message.ToLowerInvariant()
            .IndexOf(trigger.ToLower(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase);
        return firstIndexOfTrigger == -1
            ? message
            : $"{message[..firstIndexOfTrigger].Trim()} {message[(firstIndexOfTrigger + trigger.Length)..].Trim()}";
    }
}
using System.Globalization;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class PollCommand : ITextCommand
{
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly IReadOnlyList<string> _pollOptionsAlphabet = new[] { "🇦", "🇧", "🇨", "🇩", "🇪", "🇫", "🇬", "🇭", "🇮", "🇯", "🇰", "🇱", "🇲", "🇳", "🇴", "🇵", "🇶", "🇷", "🇸", "🇹" };
    private readonly string[] _triggers = { "poll" };

    public PollCommand(IReplyBusinessLayer replyBusinessLayer, IDiscordFormatter discordFormatter)
    {
        _replyBusinessLayer = replyBusinessLayer;
        _discordFormatter = discordFormatter;
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned && _triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
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
        var messageWithoutTrigger = RemoveTriggerFromMessage(messageWithoutBotName, _triggers[0]);

        if (messageWithoutTrigger.Length == 0)
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

    private static string RemoveTriggerFromMessage(string message, string trigger)
    {
        var firstIndexOfTrigger = message.ToLowerInvariant()
            .IndexOf(trigger.ToLower(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase);
        return firstIndexOfTrigger == -1
            ? message
            : $"{message[..firstIndexOfTrigger].Trim()} {message[(firstIndexOfTrigger + trigger.Length)..].Trim()}";
    }
}
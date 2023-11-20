using System.Text.RegularExpressions;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class ChooseCommand(IReplyBusinessLayer replyBusinessLayer,
        BotSettings botSettings,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"(choose|select|random|decide|pick)[^:]*: (?<{SearchTermKey}>(.*))";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned && Regex.IsMatch(replyCriteria.MessageText,
            TriggerRegexPattern,
            RegexOptions.IgnoreCase,
            _matchTimeout);
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        var response = ChooseResponse(message);
        return Task.FromResult(new CommandResponse
        {
            Description = response,
            StopProcessing = true,
            NotifyWhenReplying = true
        });
    }

    private string? ChooseResponse(IMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        var matchedGroup = match.Groups[SearchTermKey];
        var messageWithoutTrigger = matchedGroup.Value;

        if (messageWithoutTrigger.Length <= 0)
        {
            return "You need to give me at least two options to choose from!";
        }

        var splitArgs = messageWithoutTrigger.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

        if (splitArgs.Count < 2)
        {
            return "You need to give me at least two options to choose from!";
        }

        if (match.Success)
        {
            return replyBusinessLayer.ChooseReply(splitArgs.ToArray());
        }

        logger.Log(LogLevel.Error,
            $"Error in ChooseCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!";
    }
}
using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class ChooseCommand(
    BotSettings botSettings,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string SearchTermKey = "searchTerm";
    private const string NumberOfItemsKey = "numberOfItems";
    private const string TriggerRegexPattern = $"(choose|select|random|decide|pick) *(?<{NumberOfItemsKey}>( \\d+)*): (?<{SearchTermKey}>(.*))";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    private static readonly string[] DelimiterOptions = ["|", ",", " "];

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
        var numberOfItemsInput = match.Groups[NumberOfItemsKey].Value.Trim();

        var messageWithoutTrigger = matchedGroup.Value;

        if (messageWithoutTrigger.Length <= 0)
        {
            return "You need to give me at least two options to choose from!\nNote: You can separate options with a comma, pipe, or space.";
        }

        var delimiter = DetermineDelimiter(messageWithoutTrigger);

        var splitArgs = messageWithoutTrigger.Split(delimiter).Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

        var numberOfItemsToChoose = 0;
        var isNumberSpecified = !string.IsNullOrWhiteSpace(numberOfItemsInput) && int.TryParse(numberOfItemsInput, out numberOfItemsToChoose) && numberOfItemsToChoose > 1;
        if (!isNumberSpecified)
        {
            numberOfItemsToChoose = 1;
        }

        if (splitArgs.Count <= numberOfItemsToChoose)
        {
            return $"If you want me to pick {numberOfItemsToChoose}, you need to give me at least {numberOfItemsToChoose + 1} options to choose from!";
        }

        if (match.Success)
        {
            var chosenOptions = new List<string>();
            while (chosenOptions.Count < numberOfItemsToChoose)
            {
                var randomIndex = new Random().Next(0, splitArgs.Count);
                chosenOptions.Add(splitArgs[randomIndex]);
                splitArgs.RemoveAt(randomIndex);
            }
            return string.Join("\n", chosenOptions);
        }

        logger.Log(LogLevel.Error,
            $"Error in ChooseCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!";
    }

    private static string DetermineDelimiter(string messageWithoutTrigger)
    {
        foreach (var delimiter in DelimiterOptions)
        {
            if (messageWithoutTrigger.Contains(delimiter))
            {
                return delimiter;
            }
        }
        return DelimiterOptions.Last();
    }
}
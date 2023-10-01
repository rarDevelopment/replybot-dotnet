using System.Text.RegularExpressions;
using Replybot.BusinessLayer.Extensions;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class SearchCommand : ITextCommand
{
    private readonly ILogger<DiscordBot> _logger;
    private const string SearchTermKey = "searchTerm";
    private readonly TimeSpan _matchTimeout;
    private readonly List<SearchLinkMap> _searchLinkMappings = new()
    {
        new SearchLinkMap($"(ddg|duckduckgo) (?<{SearchTermKey}>(.*))", "DuckDuckGo", "https://duckduckgo.com/?q="),
        new SearchLinkMap($"google (?<{SearchTermKey}>(.*))", "Google", "https://www.google.com/search?q="),
        new SearchLinkMap($"bing (?<{SearchTermKey}>(.*))", "Bing", "https://www.bing.com/search?q="),
    };

    public SearchCommand(ILogger<DiscordBot> logger)
    {
        _logger = logger;
        _matchTimeout = TimeSpan.FromMilliseconds(100);
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        if (!replyCriteria.IsBotNameMentioned)
        {
            return false;
        }

        //only matching without bot name and full string to prevent eager matching over other commands
        return _searchLinkMappings.Select(searchLinkMapping => Regex.IsMatch(replyCriteria.MessageText,
                $"{searchLinkMapping.TriggerRegexPattern}",
                RegexOptions.IgnoreCase,
                _matchTimeout))
            .Any(isMatch => isMatch);
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        var searchToUse = GetSearchToUse(message.Content);

        var match = Regex.Match(message.Content, searchToUse.TriggerRegexPattern, RegexOptions.IgnoreCase,
            _matchTimeout);

        if (match.Success)
        {
            var messageEncodedWithoutTriggers =
                match.Groups[SearchTermKey]
                    .Value.UrlEncode();

            return Task.FromResult(new CommandResponse
            {
                Description = $"{searchToUse.Url}{messageEncodedWithoutTriggers}",
                Reactions = null,
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }

        _logger.Log(LogLevel.Error, $"Error in GameSearchCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return Task.FromResult(new CommandResponse
        {
            Description = "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true
        });
    }

    private SearchLinkMap GetSearchToUse(string messageContent)
    {
        var searchToUse = _searchLinkMappings.FirstOrDefault(searchLinkMap => Regex.IsMatch(messageContent,
            searchLinkMap.TriggerRegexPattern,
            RegexOptions.IgnoreCase,
            _matchTimeout));

        return searchToUse ?? _searchLinkMappings.First();
    }
}
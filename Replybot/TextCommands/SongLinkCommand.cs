using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;
using System.Text.RegularExpressions;
using Replybot.BusinessLayer.Extensions;

namespace Replybot.TextCommands;

public class SongLinkCommand : ITextCommand
{
    private const string SongLinkBaseUrl = "https://song.link/";
    private readonly ILogger<DiscordBot> _logger;
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"song( *)link +(?<{SearchTermKey}>(.*))";
    private readonly TimeSpan _matchTimeout;

    public SongLinkCommand(ILogger<DiscordBot> logger)
    {
        _logger = logger;
        _matchTimeout = TimeSpan.FromMilliseconds(100);
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
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var searchText = match.Groups[SearchTermKey].Value.UrlEncode();
            return Task.FromResult(new CommandResponse
            {
                Description = $"Visit the following link to find that media on other music services:\n{SongLinkBaseUrl}{searchText}",
                Reactions = null,
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }
        _logger.Log(LogLevel.Error, $"Error in SongLinkCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return Task.FromResult(new CommandResponse
        {
            Description = "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}
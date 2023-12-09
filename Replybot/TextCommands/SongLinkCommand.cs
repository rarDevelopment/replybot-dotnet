using Replybot.TextCommands.Models;
using System.Text.RegularExpressions;
using Replybot.BusinessLayer.Extensions;

namespace Replybot.TextCommands;

public class SongLinkCommand(ILogger<DiscordBot> logger) : ITextCommand
{
    private const string SongLinkBaseUrl = "https://song.link/";
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"song( *)link +(?<{SearchTermKey}>(.*))";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(100);

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
        logger.Log(LogLevel.Error, $"Error in SongLinkCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return Task.FromResult(new CommandResponse
        {
            Description = "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}
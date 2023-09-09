using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GameSearchCommand : ITextCommand
{
    private readonly InternetGameDatabaseApi _internetGameDatabaseApi;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"when (does|did|will) (?<{SearchTermKey}>(.*)) (come out|release|drop)\\??";
    private const int MaxGamesToShow = 2;
    private readonly TimeSpan _matchTimeout;

    public GameSearchCommand(InternetGameDatabaseApi internetGameDatabaseApi,
        TheMovieDbSettings theMovieDbSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _internetGameDatabaseApi = internetGameDatabaseApi;
        _discordFormatter = discordFormatter;
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

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var embed = await GetGameSearchEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed?> GetGameSearchEmbed(SocketMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var searchText = match.Groups[SearchTermKey].Value;

            try
            {
                var gameSearchResults = await _internetGameDatabaseApi.SearchGames(searchText);

                var embedFieldBuilders = new List<EmbedFieldBuilder>();

                foreach (var game in gameSearchResults.Take(MaxGamesToShow))
                {
                    if (game.ReleaseDates == null)
                    {
                        embedFieldBuilders.Add(new EmbedFieldBuilder
                        {
                            Name = game.Name,
                            Value = "N/A",
                            IsInline = false
                        });
                        continue;
                    }

                    var releaseData = new List<string>();

                    var groupedData = game.ReleaseDates.Values.GroupBy(x => $"{x.Date?.ToString("yyyy-MM-dd")}")
                        .Select(x => new
                        {
                            Date = x.Key,
                            Regions = x.ToList().GroupBy(y => y.Region),
                            Platforms = x.ToList().GroupBy(z => z.Platform)
                        });

                    foreach (var releaseDate in groupedData)
                    {
                        var platformNames = releaseDate.Platforms.Select(p =>
                        {
                            var platform = game.Platforms.Values.FirstOrDefault(pl => p.Key.Id == pl.Id);
                            return platform?.Name ?? "N/A";
                        }).Distinct();

                        var platforms = string.Join(", ", platformNames);

                        var regions = string.Join(", ", releaseDate.Regions.Select(r => r.Key != null ? r.Key.Value.ToString() : "N/A"));

                        releaseData.Add(
                            $"**{releaseDate.Date}**\n_Platform(s): {platforms}_\n_Region(s): {regions}_");
                    }

                    var releaseDates = string.Join("\n", releaseData.OrderBy(s => s));

                    embedFieldBuilders.Add(new EmbedFieldBuilder
                    {
                        Name = game.Name,
                        Value = releaseDates,
                        IsInline = false
                    });
                }

                if (embedFieldBuilders.Count == 0)
                {
                    return _discordFormatter.BuildRegularEmbedWithUserFooter("No Game(s) Found",
                        "Sorry, I couldn't find any games that matched your search.",
                        message.Author);
                }

                return _discordFormatter.BuildRegularEmbedWithUserFooter("Game Information",
                    "",
                    message.Author,
                    embedFieldBuilders);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Error in GameSearchCommand command - {0} -- {1}", ex.Message, ((RestEase.ApiException)ex).Content);
                return _discordFormatter.BuildErrorEmbed("Game Information",
                    "Hmm, couldn't do that search for some reason. Try again later!");
            }
        }

        _logger.Log(LogLevel.Error, $"Error in GameSearchCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return _discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Movie Duration",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }
}
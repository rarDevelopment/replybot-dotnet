using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class HowLongIsMovieCommand(TheMovieDbApi theMovieDbApi,
        TheMovieDbSettings theMovieDbSettings,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"(how long is|hltw|how long to watch|movie duration|film duration|movie runtime|film runtime) (?<{SearchTermKey}>(.*))\\??";
    private const int MaxMoviesToShow = 3;
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(100);

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
        var embed = await GetHowLongIsMovieEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed?> GetHowLongIsMovieEmbed(SocketMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var searchText = match.Groups[SearchTermKey].Value;

            try
            {
                var movieSearchResults = await theMovieDbApi.SearchMovies(searchText);

                var moviesToProcess = movieSearchResults.Results.Take(MaxMoviesToShow);

                var embedFieldBuilders = new List<EmbedFieldBuilder>();

                foreach (var movieResult in moviesToProcess)
                {
                    var movie = await theMovieDbApi.GetMovie(movieResult.Id);
                    var releaseYear = movie.ReleaseDate?.Year;
                    var director = movie.Credits.Crew.FirstOrDefault(c => c.Job.ToLower() == "director")?.Name ?? "No director found.";
                    var castMembers = movie.Credits.Cast.Take(3).Select(c => c.Name).ToList();
                    var castText = castMembers.Count > 0 ? string.Join(", ", castMembers) : "No star(s) found";

                    var imdbLink = !string.IsNullOrEmpty(movie.ImdbId)
                        ? $"{theMovieDbSettings.ImdbBaseUrl}{movie.ImdbId}"
                        : "No IMDB page found.";

                    var runtimeText = movie.Runtime is > 0 ? $"{ConvertMinutesToDisplayTime(movie.Runtime.Value)}" : "No runtime found.";

                    embedFieldBuilders.Add(new EmbedFieldBuilder
                    {
                        Name = movie.Title + (releaseYear != null ? $" ({releaseYear})" : ""),
                        Value = $"Directed By: {director}\nStarring: {castText}\nRuntime: {runtimeText}\nLink: [IMDB]({imdbLink})",
                        IsInline = false
                    });
                }

                if (embedFieldBuilders.Count == 0)
                {
                    return discordFormatter.BuildRegularEmbedWithUserFooter("No Movie Found",
                        "Sorry, I couldn't find any movies that matched your search.",
                        message.Author);
                }

                return discordFormatter.BuildRegularEmbedWithUserFooter("Movie Duration(s)",
                    "",
                    message.Author,
                    embedFieldBuilders);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Error in HowLongIsMovie command - {0}", ex.Message);
                return discordFormatter.BuildErrorEmbed("Movie Duration",
                    "Hmm, couldn't do that search for some reason. Try again later!");
            }
        }

        logger.Log(LogLevel.Error, $"Error in HowLongIsMovieCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Movie Duration",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }

    private static string ConvertMinutesToDisplayTime(int movieRuntime)
    {
        return TimeSpan.FromMinutes(movieRuntime).ToString(@"h\hmm\m");
    }
}
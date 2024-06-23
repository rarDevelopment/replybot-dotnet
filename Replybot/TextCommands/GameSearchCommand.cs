using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GameSearchCommand(InternetGameDatabaseApi internetGameDatabaseApi,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"when +(does|did|will|is) +(?<{SearchTermKey}>(.*)) +(come out|release|drop|releasing|dropping|coming out)\\??";
    private const int MaxGamesToShow = 3;
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
                var gameSearchResults = await internetGameDatabaseApi.SearchGames(searchText);

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

                    var releaseDateDisplayStrings = new List<string>();

                    var groupedData = game.ReleaseDates.Values.GroupBy(x => $"{x.Date?.ToString("yyyy-MM-dd")}")
                        .Select(x => new
                        {
                            Date = x.Key,
                            Regions = x.ToList().GroupBy(y => y.Region),
                            Platforms = x.ToList().GroupBy(z => z.Platform)
                        });

                    foreach (var releaseDateGroup in groupedData)
                    {
                        var platformNames = releaseDateGroup.Platforms.Select(p =>
                        {
                            var platform = game.Platforms.Values.FirstOrDefault(pl => p.Key.Id == pl.Id);
                            return platform?.Name ?? "N/A";
                        }).Distinct();


                        var releaseDateDisplay = "No Date Available";

                        if (!string.IsNullOrEmpty(releaseDateGroup.Date) && DateTime.TryParse(releaseDateGroup.Date, out _))
                        {
                            releaseDateDisplay = releaseDateGroup.Date;
                        }

                        var platforms = string.Join(", ", platformNames);

                        var regions = string.Join(", ", releaseDateGroup.Regions.Select(r => r.Key != null ? r.Key.Value.ToString() : "N/A"));

                        releaseDateDisplayStrings.Add(
                            $"**{releaseDateDisplay}**\n_Platform(s): {platforms}_\n_Region(s): {regions}_");
                    }

                    var statusDisplay = game.Status != null ? $"Release Status: **{game.Status}**\n" : "";

                    var releaseDates = string.Join("\n", releaseDateDisplayStrings.OrderBy(s => s));

                    embedFieldBuilders.Add(new EmbedFieldBuilder
                    {
                        Name = game.Name,
                        Value = $"{statusDisplay}{releaseDates}",
                        IsInline = false
                    });
                }

                if (embedFieldBuilders.Count == 0)
                {
                    return discordFormatter.BuildRegularEmbedWithUserFooter("No Game(s) Found",
                        "Sorry, I couldn't find any games that matched your search.",
                        message.Author);
                }

                return discordFormatter.BuildRegularEmbedWithUserFooter("Game Information",
                    "",
                    message.Author,
                    embedFieldBuilders);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Error in GameSearchCommand command - {0} -- {1}", ex.Message, ((RestEase.ApiException)ex).Content);
                return discordFormatter.BuildErrorEmbed("Game Information",
                    "Hmm, couldn't do that search for some reason. Try again later!");
            }
        }

        logger.Log(LogLevel.Error, $"Error in GameSearchCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Game Information",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }
}

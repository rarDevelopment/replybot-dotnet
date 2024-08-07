﻿using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer.Extensions;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class HowLongToBeatCommand(HowLongToBeatSettings howLongToBeatSettings,
        HowLongToBeatApi howLongToBeatApi,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string UrlKeyword = "{{URL}}";
    private const string QueryKeyword = "{{QUERY}}";
    private const string GameIdKeyword = "{{GAME_ID}}";
    private const string SearchUrlTemplate = $"{UrlKeyword}?q={QueryKeyword}#search";
    private const string GameUrlTemplate = $"{UrlKeyword}game?id={GameIdKeyword}";
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"(hltb|how long to beat|game length) +(?<{SearchTermKey}>(.*))\\??";
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
        var embed = await GetHowLongToBeatEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed?> GetHowLongToBeatEmbed(SocketMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var searchText = match.Groups[SearchTermKey].Value.CleanString();
            var searchUrl =
                SearchUrlTemplate
                    .Replace(UrlKeyword, howLongToBeatSettings.BaseUrl)
                    .Replace(QueryKeyword, Uri.EscapeDataString(searchText));

            try
            {
                var howLongToBeatInfo = await howLongToBeatApi.GetHowLongToBeatInformation(searchText);
                if (howLongToBeatInfo == null)
                {
                    var embedFieldBuilder = new List<EmbedFieldBuilder>
                    {
                        new()
                        {
                            Name = "Want to search on the website instead?",
                            Value = $"[Click here to try a manual search]({searchUrl})",
                            IsInline = false
                        }
                    };

                    return discordFormatter.BuildErrorEmbedWithUserFooter("No Response from How Long To Beat",
                        "I couldn't get a response from How Long To Beat, maybe they changed their URL again.",
                        message.Author,
                        embedFieldBuilder,
                        searchUrl);
                }

                var gamesToProcess = howLongToBeatInfo.Data.Take(2);

                var embedFieldBuilders = new List<EmbedFieldBuilder>();

                foreach (var game in gamesToProcess)
                {
                    var mainStoryHours = ConvertSecondsToHoursForDisplay(game.CompMain, 1);
                    var mainStoryPlusSides = ConvertSecondsToHoursForDisplay(game.CompPlus, 1);
                    var completionist = ConvertSecondsToHoursForDisplay(game.Comp100, 1);
                    var allStyles = ConvertSecondsToHoursForDisplay(game.CompAll, 1);

                    var gameUrl = GameUrlTemplate
                        .Replace(UrlKeyword, howLongToBeatSettings.BaseUrl)
                        .Replace(GameIdKeyword, game.GameId.ToString());

                    var messageForField =
                        $"Main Story: {FormatHoursForDisplay(mainStoryHours)}\nMain + Extra: {FormatHoursForDisplay(mainStoryPlusSides)}\n Completionist: {FormatHoursForDisplay(completionist)}\n All Styles: {FormatHoursForDisplay(allStyles)}\n[More Info]({gameUrl})";

                    if (mainStoryHours == 0 && mainStoryPlusSides == 0 && completionist == 0 && allStyles == 0)
                    {
                        messageForField =
                            $"_It looks like this game might not have any information (yet?)_\n{messageForField}";
                    }

                    embedFieldBuilders.Add(new EmbedFieldBuilder
                    {
                        Name = game.GameName,
                        Value = messageForField,
                        IsInline = false
                    });
                }

                var searchLinkTitle = embedFieldBuilders.Count > 0
                    ? "Not what you were looking for?"
                    : "I didn't find anything, but...";
                embedFieldBuilders.Add(new EmbedFieldBuilder
                {
                    Name = searchLinkTitle,
                    Value = $"[Click here for more results]({searchUrl})",
                    IsInline = false
                });

                return discordFormatter.BuildRegularEmbedWithUserFooter("How Long To Beat",
                    "",
                    message.Author,
                    embedFieldBuilders,
                    searchUrl);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Error in HowLongToBeat command - {0}", ex.Message);
                return discordFormatter.BuildErrorEmbed("How Long To Beat",
                    $"Hmm, couldn't reach the site, but here's a link to try yourself: {searchUrl}");
            }
        }

        logger.Log(LogLevel.Error, $"Error in HowLongToBeatCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Game",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }

    private static string FormatHoursForDisplay(decimal mainStoryHours)
    {
        return mainStoryHours == 0 ? "-" : $"{mainStoryHours} hours";
    }

    private static decimal ConvertSecondsToHoursForDisplay(int seconds, int decimalPlaces)
    {
        return Math.Round(ConvertSecondsToHours(seconds), decimalPlaces);
    }

    private static decimal ConvertSecondsToHours(int seconds)
    {
        return seconds / 60m / 60m;
    }
}
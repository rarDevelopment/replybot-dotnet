﻿using DiscordDotNetUtilities.Interfaces;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using System.Text.RegularExpressions;
using Replybot.BusinessLayer.Extensions;
using Replybot.Models;

namespace Replybot.TextCommands;

public class CanIStreamCommand(CountryConfigService countryConfigService,
        ILogger<DiscordBot> logger,
        IDiscordFormatter discordFormatter)
    : ITextCommand
{
    private const string JustWatchBaseUrl = "https://www.justwatch.com/";
    private const string CountryListGitHubUrl = "https://github.com/rarDevelopment/justwatch-country-config/";
    private const string Description = "Use the following link to see streaming availability search results in the specified country.";

    private const string SearchTermKey = "searchTerm";
    private const string CountryTermKey = "countryTerm";
    private const string BeginningTriggerWordsPattern = "((((where )? *((can|do) i )|just)?) *(watch|stream))";
    private const string TriggerRegexPattern = $"{BeginningTriggerWordsPattern}(.*)\\??";
    private const string SearchRegexPattern = $"{BeginningTriggerWordsPattern} +(?<{SearchTermKey}>(.*))";
    private const string CountryRegexPattern = $"in +(?<{CountryTermKey}>([A-Za-z- ]*))";
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
        var embed = await GetStreamLinksEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed> GetStreamLinksEmbed(SocketMessage message)
    {
        var searchAndCountry = DetermineSearchAndCountry(message.Content);

        if (searchAndCountry != null && searchAndCountry.IsValid())
        {
            var countryConfigs = await GetCountryConfigs();

            if (countryConfigs == null)
            {
                return discordFormatter.BuildErrorEmbed("Error Finding Streaming Options",
                    $"Sorry, I couldn't get the configurations to construct the streaming links. You can search for yourself at {JustWatchBaseUrl}!");
            }

            var countryToUse =
                countryConfigs.FirstOrDefault(c => c.TriggerNames != null && c.TriggerNames.Any() && c.TriggerNames.Contains(searchAndCountry.Country, StringComparer.InvariantCultureIgnoreCase));

            if (countryToUse == null)
            {
                var supportedCountries = countryConfigs.OrderBy(s => s.Name).Select(c => c.Name).ToList();
                return discordFormatter.BuildErrorEmbed("Could Not Find Specified Country",
                    $"Sorry, I couldn't get the configurations for the specified country: `{searchAndCountry.Country}`.\n" +
                    $"Note that I do not have all countries configured, and you can [request a country be added on GitHub]({CountryListGitHubUrl})!\n" +
                    $"Alternatively, if you believe this is an error, let me know!\n\nSupported countries are: **{string.Join(", ", supportedCountries)}**");
            }

            var embedFieldBuilder = new EmbedFieldBuilder
            {
                Name = $"{countryToUse.Emoji} {countryToUse.Name}",
                Value =
                    $"{JustWatchBaseUrl}{countryToUse.Code}/{countryToUse.UrlSearchWord}?q={searchAndCountry.SearchText}",
                IsInline = false
            };

            return discordFormatter.BuildRegularEmbedWithUserFooter("Stream Link Options", Description, message.Author, new List<EmbedFieldBuilder> { embedFieldBuilder });
        }
        logger.Log(LogLevel.Error, $"Error in CanIStreamCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return discordFormatter.BuildErrorEmbedWithUserFooter("Error Building JustWatch Link",
            "You need to specify the country that you're looking for.\nTry something like:\n`can I stream back to the future in canada`.",
            message.Author);
    }

    private SearchAndCountryPair? DetermineSearchAndCountry(string messageText)
    {
        var lastInIndex = messageText.LastIndexOf("in", StringComparison.InvariantCultureIgnoreCase);
        if (lastInIndex == -1)
        {
            return null;
        }

        var countrySection = messageText[lastInIndex..];

        var countryMatch = Regex.Match(countrySection, CountryRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (!countryMatch.Success)
        {
            return null;
        }

        var country = countryMatch.Groups[CountryTermKey].Value.Trim();

        var otherSection = messageText[..lastInIndex];

        var searchMatch = Regex.Match(otherSection, SearchRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

        if (!searchMatch.Success)
        {
            return null;
        }

        var searchText = searchMatch.Groups[SearchTermKey].Value.Trim().UrlEncode();

        return new SearchAndCountryPair(searchText, country);
    }

    private async Task<IReadOnlyList<CountryConfig>?> GetCountryConfigs()
    {
        var countryConfigList = await countryConfigService.GetCountryConfigList();
        return countryConfigList;
    }
}
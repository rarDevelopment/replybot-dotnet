using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class HowLongToBeatCommand : ITextCommand
{
    private readonly HowLongToBeatSettings _howLongToBeatSettings;
    private readonly HowLongToBeatApi _howLongToBeatApi;
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private const string UrlKeyword = "{{URL}}";
    private const string QueryKeyword = "{{QUERY}}";
    private const string GameIdKeyword = "{{GAME_ID}}";
    private const string SearchUrlTemplate = $"{UrlKeyword}?q={QueryKeyword}#search";
    private const string GameUrlTemplate = $"{UrlKeyword}game?id={GameIdKeyword}";
    private readonly string[] _triggers = { "hltb" };

    public HowLongToBeatCommand(HowLongToBeatSettings howLongToBeatSettings,
        HowLongToBeatApi howLongToBeatApi,
        IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _howLongToBeatSettings = howLongToBeatSettings;
        _howLongToBeatApi = howLongToBeatApi;
        _replyBusinessLayer = replyBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned && _triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var embed = await GetHowLongToBeatEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true
        };
    }

    private async Task<Embed?> GetHowLongToBeatEmbed(SocketMessage message)
    {
        var messageWithoutBotName = KeywordHandler.RemoveBotName(message.Content);
        var messageWithoutTrigger =
            messageWithoutBotName.Replace(_triggers[0], "", StringComparison.InvariantCultureIgnoreCase);
        var searchText = messageWithoutTrigger.Trim();

        var searchUrl =
            SearchUrlTemplate
                .Replace(UrlKeyword, _howLongToBeatSettings.BaseUrl)
                .Replace(QueryKeyword, Uri.EscapeDataString(searchText));

        try
        {
            var howLongToBeatInfo = await _howLongToBeatApi.GetHowLongToBeatInformation(searchText);
            if (howLongToBeatInfo == null)
            {
                return null;
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
                    .Replace(UrlKeyword, _howLongToBeatSettings.BaseUrl)
                    .Replace(GameIdKeyword, game.GameId.ToString());

                var messageForField = $"Main Story: {mainStoryHours} hours\nMain + Extra: {mainStoryPlusSides} hours\n Completionist: {completionist} hours\n All Styles: {allStyles} hours\n[More Info]({gameUrl})";

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

            return _discordFormatter.BuildRegularEmbed("How Long To Beat",
                "",
                message.Author,
                embedFieldBuilders,
                searchUrl);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, "Error in HowLongToBeat command - {0}", ex.Message);
            return _discordFormatter.BuildErrorEmbed("How Long To Beat",
                $"Hmm, couldn't reach the site, but here's a link to try yourself: {searchUrl}", embedFooterBuilder: null);
        }
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
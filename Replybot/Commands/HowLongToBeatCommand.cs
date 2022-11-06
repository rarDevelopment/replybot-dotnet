using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;

namespace Replybot.Commands
{
    public class HowLongToBeatCommand
    {
        private readonly HowLongToBeatSettings _howLongToBeatSettings;
        private readonly HowLongToBeatApi _howLongToBeatApi;
        private readonly KeywordHandler _keywordHandler;
        private readonly IDiscordFormatter _discordFormatter;
        private readonly ILogger<DiscordBot> _logger;
        private const string UrlKeyword = "{{URL}}";
        private const string QueryKeyword = "{{QUERY}}";
        private const string GameIdKeyword = "{{GAME_ID}}";
        private const string SearchUrlTemplate = $"{UrlKeyword}?q={QueryKeyword}#search";
        private const string GameUrlTemplate = $"{UrlKeyword}game?id={GameIdKeyword}";

        public HowLongToBeatCommand(HowLongToBeatSettings howLongToBeatSettings,
            HowLongToBeatApi howLongToBeatApi,
            KeywordHandler keywordHandler,
            IDiscordFormatter discordFormatter,
            ILogger<DiscordBot> logger)
        {
            _howLongToBeatSettings = howLongToBeatSettings;
            _howLongToBeatApi = howLongToBeatApi;
            _keywordHandler = keywordHandler;
            _discordFormatter = discordFormatter;
            _logger = logger;
        }

        public async Task<Embed?> GetHowLongToBeatEmbed(SocketMessage message)
        {
            var messageContent = message.Content;

            var messageWithoutBotName = _keywordHandler.RemoveBotName(messageContent);
            var messageWithoutTrigger =
                messageWithoutBotName.Replace("hltb", "", StringComparison.InvariantCultureIgnoreCase);
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
                    $"Hmm, couldn't reach the site, but here's a link to try yourself: {searchUrl}");
            }
        }

        private decimal ConvertSecondsToHoursForDisplay(int seconds, int decimalPlaces)
        {
            return Math.Round(ConvertSecondsToHours(seconds), decimalPlaces);
        }

        private decimal ConvertSecondsToHours(int seconds)
        {
            return seconds / 60m / 60m;
        }
    }
}

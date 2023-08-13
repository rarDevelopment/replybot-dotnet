using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Models.FreeDictionary;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class DefineWordCommand : ITextCommand
{
    private readonly FreeDictionaryApi _freeDictionaryApi;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private const string SearchTermKey = "searchTerm";
    private const string TriggerRegexPattern = $"define (?<{SearchTermKey}>([a-z0-9 ])*)|what does (?<{SearchTermKey}>([a-z0-9 ]*)) mean\\??";
    private readonly TimeSpan _matchTimeout;

    public DefineWordCommand(BotSettings botSettings,
        FreeDictionaryApi freeDictionaryApi,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _freeDictionaryApi = freeDictionaryApi;
        _discordFormatter = discordFormatter;
        _logger = logger;
        _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
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
        var embed = await GetWordDefinitionEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed?> GetWordDefinitionEmbed(SocketMessage message)
    {
        var messageWithoutBotName = KeywordHandler.RemoveBotName(message.Content);

        var match = Regex.Match(messageWithoutBotName, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var matchedGroup = match.Groups[SearchTermKey];
            var messageWithoutTrigger = matchedGroup.Value;

            var splitWords = messageWithoutTrigger.Trim().Split(' ').Where(w => !string.IsNullOrEmpty(w)).ToList();

            if (splitWords.Count <= 0)
            {
                return _discordFormatter.BuildErrorEmbed("No Word Provided",
                    "What would you like me to define?",
                    message.Author);
            }

            try
            {
                var definition = await _freeDictionaryApi.GetDefinition(splitWords[0]);
                if (definition == null)
                {
                    return _discordFormatter.BuildErrorEmbed("No Definition Found",
                        "I couldn't find a definition for that word.",
                        message.Author);
                }

                var origin = !string.IsNullOrEmpty(definition.Origin) ? $"Origin: {definition.Origin}" : "";

                var embedFieldBuilders = BuildDefinitionFields(definition);

                return _discordFormatter.BuildRegularEmbed(
                    definition.Word,
                    origin,
                    message.Author,
                    embedFieldBuilders);
            }
            catch (HttpRequestException ex)
            {
                _logger.Log(LogLevel.Error, "Error in DefineWordCommand: {0}", ex.Message);
                return _discordFormatter.BuildErrorEmbed("Error Defining Word",
                    "There was an error retrieving that definition, please try again later.",
                    message.Author);
            }
        }

        _logger.Log(LogLevel.Error, $"Error in DefineWordCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return _discordFormatter.BuildErrorEmbed("Error Defining Word",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }

    private static List<EmbedFieldBuilder> BuildDefinitionFields(FreeDictionaryResponse definition)
    {
        var embedFieldBuilders = definition.Meanings.Select(meaning =>
        {
            var definitions = meaning.Definitions
                .Take(3)
                .Select((def, index) =>
                    BuildDefinitionString(index, def));

            return new EmbedFieldBuilder
            {
                Name = meaning.PartOfSpeech,
                Value = string.Join("\n", definitions),
                IsInline = false
            };
        }).ToList();

        return embedFieldBuilders;
    }

    private static string BuildDefinitionString(int index, Definition def)
    {
        var exampleUsage = !string.IsNullOrEmpty(def.Example) ? $"\n_ex: {def.Example}_" : "";
        var synonyms = def.Synonyms.Any() ? $"\nSynonyms: {string.Join(",", def.Synonyms)}" : "";
        return $"{index + 1}. {def.DefinitionText}{exampleUsage}{synonyms}";
    }
}
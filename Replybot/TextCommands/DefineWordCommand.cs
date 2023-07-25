using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models.FreeDictionary;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class DefineWordCommand : ITextCommand
{
    private readonly FreeDictionaryApi _freeDictionaryApi;
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private readonly string[] _triggers = { "define" };
    private const string TriggerRegexPattern = "what does (?<searchTerm>[a-z0-9 ]*) mean\\??";
    private readonly TimeSpan _matchTimeout;

    public DefineWordCommand(FreeDictionaryApi freeDictionaryApi,
        IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _freeDictionaryApi = freeDictionaryApi;
        _replyBusinessLayer = replyBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
        _matchTimeout = TimeSpan.FromMinutes(1);
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               (_triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText)) ||
                Regex.IsMatch(replyCriteria.MessageText, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var embed = await GetWordDefinitionEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true
        };
    }

    private async Task<Embed?> GetWordDefinitionEmbed(SocketMessage message)
    {
        var messageContent = message.Content;
        var messageWithoutBotName = KeywordHandler.RemoveBotName(messageContent);

        string messageWithoutTrigger;

        var match = Regex.Match(messageWithoutBotName, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var matchedGroup = match.Groups["searchTerm"];
            messageWithoutTrigger = matchedGroup.Value;
        }
        else
        {
            messageWithoutTrigger = messageWithoutBotName.ReplaceTriggerInMessage(_triggers);
        }

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
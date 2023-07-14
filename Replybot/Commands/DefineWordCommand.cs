using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models.FreeDictionary;
using Replybot.ServiceLayer;

namespace Replybot.Commands;

public class DefineWordCommand : IReplyCommand
{
    private readonly FreeDictionaryApi _freeDictionaryApi;
    private readonly KeywordHandler _keywordHandler;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private readonly string[] _triggers = { "define" };

    public DefineWordCommand(FreeDictionaryApi freeDictionaryApi,
        KeywordHandler keywordHandler,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _freeDictionaryApi = freeDictionaryApi;
        _keywordHandler = keywordHandler;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    public bool CanHandle(string? reply)
    {
        return reply == _keywordHandler.BuildKeyword("DefineWord");
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
        var messageWithoutBotName = _keywordHandler.RemoveBotName(messageContent);

        var messageWithoutTrigger = ReplaceTriggerInMessage(messageWithoutBotName);

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

    private string ReplaceTriggerInMessage(string text)
    {
        var replacedText = text;
        for (int replacementsDone = 0, index = 0; replacementsDone == 0 && index < _triggers.Length; index++)
        {
            var indexOfTrigger = text.IndexOf(_triggers[index], StringComparison.InvariantCultureIgnoreCase);
            if (indexOfTrigger == -1)
            {
                continue;
            }
            replacedText = text.Remove(indexOfTrigger, _triggers[index].Length);
            replacementsDone++;
        }

        return replacedText;
    }

    private static List<EmbedFieldBuilder> BuildDefinitionFields(FreeDictionaryResponse definition)
    {
        var embedFieldBuilders = new List<EmbedFieldBuilder>();
        foreach (var meaning in definition.Meanings)
        {
            var definitions = meaning.Definitions
                .Take(3)
                .Select((def, index) =>
                    BuildDefinitionString(index, def));

            embedFieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = meaning.PartOfSpeech,
                Value = string.Join("\n", definitions),
                IsInline = false
            });
        }

        return embedFieldBuilders;
    }

    private static string BuildDefinitionString(int index, Definition def)
    {
        var exampleUsage = !string.IsNullOrEmpty(def.Example) ? $"\n_ex: {def.Example}_" : "";
        var synonyms = def.Synonyms.Any() ? $"\nSynonyms: {string.Join(",", def.Synonyms)}" : "";
        return $"{index + 1}. {def.DefinitionText}{exampleUsage}{synonyms}";
    }
}
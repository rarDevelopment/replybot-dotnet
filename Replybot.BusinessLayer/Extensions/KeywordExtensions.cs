using System.Globalization;
using System.Web;
using Discord.WebSocket;
using Replybot.Models;

namespace Replybot.BusinessLayer.Extensions;

public static class KeywordExtensions
{
    public static string ReplaceKeywords(this string text,
        SocketUser author,
        string? versionNumber,
        string messageContent,
        GuildReplyDefinition guildReplyDefinition)
    {
        var messageWithoutReplybot = RemoveBotName(messageContent);
        var messageWithoutTrigger = RemoveTriggerFromMessage(messageContent, guildReplyDefinition);

        return text
            .Replace(TriggerKeyword.Username.BuildKeyword(), author.Username).Replace(TriggerKeyword.UserTag.BuildKeyword(), author.Mention)
            .Replace(TriggerKeyword.VersionNumber.BuildKeyword(), versionNumber ?? "[unavailable]")
            .Replace(TriggerKeyword.Message.BuildKeyword(), messageContent).Replace(TriggerKeyword.MessageWithoutReplybot.BuildKeyword(), messageWithoutReplybot).Replace((string)TriggerKeyword.MessageWithoutTrigger.BuildKeyword(), messageWithoutTrigger).Replace((string)TriggerKeyword.MessageSpongebob.BuildKeyword(), Spongebobify(messageContent))
            .Replace(TriggerKeyword.MessageEncoded.BuildKeyword(), HttpUtility.UrlPathEncode(messageWithoutReplybot))
            .Replace(TriggerKeyword.MessageEncodedWithoutTrigger.BuildKeyword(), HttpUtility.UrlPathEncode(messageWithoutTrigger), StringComparison.InvariantCultureIgnoreCase)
            .Replace(TriggerKeyword.MessageUpperCase.BuildKeyword(),
                messageContent.ToUpper())
            .Replace(TriggerKeyword.DeleteMessage.BuildKeyword(), "");
    }

    public static string BuildKeyword(this TriggerKeyword keyword)
    {
        return $"{{{{{keyword.ToString().ToUpper(CultureInfo.InvariantCulture)}}}}}";
    }

    private static string RemoveTriggerFromMessage(string messageContent, GuildReplyDefinition guildReplyDefinitions)
    {
        var messageWithoutTrigger = messageContent;
        foreach (var triggerToUse in guildReplyDefinitions.Triggers)
        {
            var triggerWithReplacedKeywords = triggerToUse;
            var botNameInTrigger = messageWithoutTrigger.GetBotNameInMessage();
            if (!string.IsNullOrEmpty(botNameInTrigger))
            {
                triggerWithReplacedKeywords =
                    triggerToUse.Replace(TriggerKeyword.BotName.BuildKeyword(), botNameInTrigger);
            }

            var indexOfTrigger = messageWithoutTrigger.ToLower().IndexOf(triggerWithReplacedKeywords.ToLower(), StringComparison.InvariantCultureIgnoreCase);
            if (indexOfTrigger != -1)
            {
                messageWithoutTrigger = messageWithoutTrigger
                    .Substring(indexOfTrigger + triggerWithReplacedKeywords.Length).Trim();
            }
        }

        return messageWithoutTrigger;
    }

    private static string RemoveBotName(string messageContent)
    {
        var messageWithoutBotName = messageContent;
        foreach (var botName in BotNames.Names)
        {
            messageWithoutBotName =
                messageWithoutBotName.Replace(botName, "", StringComparison.InvariantCultureIgnoreCase);
        }

        return messageWithoutBotName;
    }

    private static string Spongebobify(string text) // sponch
    {
        var splitWords = text.Split(" ");
        var spongebobifiedWords = new List<string>();
        foreach (var w in splitWords)
        {
            if (w.Contains("<"))
            {
                spongebobifiedWords.Add(w);
            }
            else
            {
                var newWord = SpongebobifyWord(w);
                spongebobifiedWords.Add(newWord);
            }
        }

        return string.Join(" ", spongebobifiedWords);
    }

    private static string SpongebobifyWord(string word)
    {
        // found here: https://stackoverflow.com/a/36254270/386869
        var newWord = string.Concat(
            word
                .ToLower()
                .AsEnumerable()
                .Select((c, i) => i % 2 == 0 ? c : char.ToUpper(c)));
        return newWord;
    }
}
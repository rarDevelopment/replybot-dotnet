﻿using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public static class KeywordHandler
{
    public static string EscapeRegExp(string text, TimeSpan matchTimeout)
    {
        var pattern = "[.*+?^${}()|[\\]\\\\]";
        var escapedText = Regex.Replace(text, pattern, "\\$&", RegexOptions.None, matchTimeout);
        return escapedText;
    }

    public static string ReplaceKeywords(string replyText,
        string username,
        ulong userId,
        string? versionNumber,
        string messageContent,
        GuildReplyDefinition guildReplyDefinition)
    {
        var userTag = BuildUserTag(userId);
        var messageWithoutReplybot = RemoveBotName(messageContent);
        var messageWithoutTrigger = RemoveTriggerFromMessage(messageContent, guildReplyDefinition);

        return replyText
            .Replace(BuildKeyword(TriggerKeyword.Username), username).Replace(BuildKeyword(TriggerKeyword.UserTag), userTag)
            .Replace(BuildKeyword(TriggerKeyword.VersionNumber), versionNumber ?? "[unavailable]")
            .Replace(BuildKeyword(TriggerKeyword.Message), messageContent).Replace(BuildKeyword(TriggerKeyword.MessageWithoutReplybot), messageWithoutReplybot).Replace((string)BuildKeyword(TriggerKeyword.MessageWithoutTrigger), messageWithoutTrigger).Replace((string)BuildKeyword(TriggerKeyword.MessageSpongebob), Spongebobify(messageContent))
            .Replace(BuildKeyword(TriggerKeyword.MessageEncoded), HttpUtility.UrlPathEncode(messageWithoutReplybot))
            .Replace(BuildKeyword(TriggerKeyword.MessageEncodedWithoutTrigger), HttpUtility.UrlPathEncode(messageWithoutTrigger), StringComparison.InvariantCultureIgnoreCase)
            .Replace(BuildKeyword(TriggerKeyword.MessageUpperCase),
                messageContent.ToUpper())
            .Replace(BuildKeyword(TriggerKeyword.DeleteMessage), "");
    }

    public static string UrlEncode(this string text)
    {
        return HttpUtility.UrlPathEncode(text);
    }

    public static string RemoveTriggersFromMessage(this string messageContent, string[] triggersToRemove)
    {
        var messageWithoutTrigger = messageContent;
        foreach (var triggerToUse in triggersToRemove)
        {
            var triggerWithReplacedKeywords = triggerToUse;
            var botNameInTrigger = GetBotNameInMessage(messageWithoutTrigger);
            if (!string.IsNullOrEmpty(botNameInTrigger))
            {
                triggerWithReplacedKeywords =
                    triggerToUse.Replace(BuildKeyword(TriggerKeyword.BotName), botNameInTrigger);
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

    private static string RemoveTriggerFromMessage(string messageContent, GuildReplyDefinition guildReplyDefinitions)
    {
        var messageWithoutTrigger = messageContent;
        foreach (var triggerToUse in guildReplyDefinitions.Triggers)
        {
            var triggerWithReplacedKeywords = triggerToUse;
            var botNameInTrigger = GetBotNameInMessage(messageWithoutTrigger);
            if (!string.IsNullOrEmpty(botNameInTrigger))
            {
                triggerWithReplacedKeywords =
                    triggerToUse.Replace(BuildKeyword(TriggerKeyword.BotName), botNameInTrigger);
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

    public static string? GetBotNameInMessage(string messageContent, string? botNickname = null)
    {
        string? botNameFound = null;
        var namesToCheck = BotNames.Names.ToList();
        if (!string.IsNullOrEmpty(botNickname)
            && !namesToCheck
                .Select(name => name.ToLower(CultureInfo.InvariantCulture))
                .Contains(botNickname.ToLower(CultureInfo.InvariantCulture)))
        {
            namesToCheck.Add(botNickname);
        }

        foreach (var nameToCheck in namesToCheck)
        {
            if (string.IsNullOrEmpty(botNameFound))
            {
                var indexOfName = messageContent.IndexOf(nameToCheck, StringComparison.InvariantCultureIgnoreCase);
                if (indexOfName != -1)
                {
                    botNameFound = messageContent.Substring(indexOfName, nameToCheck.Length);
                }
            }
        }

        return botNameFound;
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

    private static string BuildUserTag(ulong userId)
    {
        return $"<@!{userId}>";
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

    public static string CleanMessageForTrigger(string message)
    {
        string cleanedMessage = message.Replace(BuildKeyword(TriggerKeyword.BotName), "", StringComparison.InvariantCultureIgnoreCase);
        foreach (var botName in BotNames.Names)
        {
            cleanedMessage = cleanedMessage.Replace(botName, BuildKeyword(TriggerKeyword.BotName), StringComparison.InvariantCultureIgnoreCase);
        }
        // TODO: replace accented characters here
        return cleanedMessage;
    }

    public static string BuildKeyword(TriggerKeyword keyword)
    {
        return BuildKeyword(keyword.ToString());
    }

    private static string BuildKeyword(string keyword)
    {
        return $"{{{{{keyword.ToUpper(CultureInfo.InvariantCulture)}}}}}";
    }
}
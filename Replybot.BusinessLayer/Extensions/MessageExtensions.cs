using System.Globalization;
using System.Web;
using Replybot.Models;

namespace Replybot.BusinessLayer.Extensions;

public static class MessageExtensions
{
    public static string UrlEncode(this string text)
    {
        return HttpUtility.UrlPathEncode(text);
    }

    public static string? GetBotNameInMessage(this string messageContent, string? botNickname = null)
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
            if (!string.IsNullOrEmpty(botNameFound))
            {
                continue;
            }

            var indexOfName = messageContent.IndexOf(nameToCheck, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfName != -1)
            {
                botNameFound = messageContent.Substring(indexOfName, nameToCheck.Length);
            }

            if (string.IsNullOrEmpty(botNameFound))
            {
                continue;
            }

            foreach (var botNameToIgnore in BotNames.NamesToIgnore.ToList())
            {
                var indexOfNameToIgnore = messageContent.IndexOf(botNameToIgnore, StringComparison.InvariantCultureIgnoreCase);
                if (indexOfNameToIgnore != -1 && indexOfNameToIgnore == indexOfName)
                {
                    botNameFound = null; //remove found name since we ignore this name
                }
            }

        }

        return botNameFound;
    }
}
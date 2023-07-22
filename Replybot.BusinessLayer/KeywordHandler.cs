using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Discord;
using Discord.WebSocket;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public class KeywordHandler
{
    public string EscapeRegExp(string text)
    {
        var regex = new Regex("[.*+?^${}()|[\\]\\\\]");
        var escapedText = regex.Replace(text, "\\$&");
        return escapedText;
    }

    public string ReplaceKeywords(string replyText,
        string username,
        ulong userId,
        string? versionNumber,
        string messageContent,
        GuildReplyDefinition guildReplyDefinition,
        IReadOnlyList<SocketUser> mentionedUsers,
        IGuild? guild, SocketGuildChannel? socketGuildChannel)
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
            .Replace(BuildKeyword(TriggerKeyword.MentionedUserAvatar),
                GetUserAvatarsAsString(mentionedUsers))
            .Replace(BuildKeyword(TriggerKeyword.MentionedUserServerAvatar),
                GetUserServerAvatarsAsString(mentionedUsers))
            .Replace(BuildKeyword(TriggerKeyword.ServerIcon),
                GetGuildIcon(guild))
            .Replace(BuildKeyword(TriggerKeyword.ServerBanner),
                GetGuildBanner(guild))
            .Replace(BuildKeyword(TriggerKeyword.MemberCount),
                GetGuildMemberCount(guild)?.ToString())
            .Replace(BuildKeyword(TriggerKeyword.ChannelCreateDate), GetChannelAgeString(socketGuildChannel))
            .Replace(BuildKeyword(TriggerKeyword.DeleteMessage), "");
    }

    private static string GetChannelAgeString(SocketGuildChannel? socketGuildChannel)
    {
        if (socketGuildChannel == null)
        {
            return "...this is not a channel.";
        }

        var createdAtDate = socketGuildChannel.CreatedAt;
        var timeAgo = DateTime.UtcNow - createdAtDate;
        return $"{createdAtDate} ({timeAgo:d'd 'h'h 'm'm 's's'} ago)";
    }

    private string RemoveTriggerFromMessage(string messageContent, GuildReplyDefinition guildReplyDefinitions)
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

    public string? GetBotNameInMessage(string messageContent, string? botNickname = null)
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

    public string RemoveBotName(string messageContent)
    {
        var messageWithoutBotName = messageContent;
        foreach (var botName in BotNames.Names)
        {
            messageWithoutBotName =
                messageWithoutBotName.Replace(botName, "", StringComparison.InvariantCultureIgnoreCase);
        }

        return messageWithoutBotName;
    }

    private string BuildUserTag(ulong userId)
    {
        return $"<@!{userId}>";
    }

    private int? GetGuildMemberCount(IGuild? guild)
    {
        return guild?.ApproximateMemberCount;
    }

    private string? GetGuildBanner(IGuild? guild)
    {
        return string.IsNullOrEmpty(guild?.BannerUrl) ? "No banner." : guild.BannerUrl;
    }

    private string? GetGuildIcon(IGuild? guild)
    {
        return string.IsNullOrEmpty(guild?.IconUrl) ? "No icon." : guild.IconUrl;
    }

    private string GetUserAvatarsAsString(IReadOnlyList<SocketUser> mentionedUsers)
    {
        var avatarUrls = mentionedUsers.Select(u => u.GetAvatarUrl(ImageFormat.Png));
        return string.Join("\n", avatarUrls);
    }

    private string GetUserServerAvatarsAsString(IReadOnlyList<SocketUser> mentionedUsers)
    {
        var avatarUrls = mentionedUsers.Select(u => (u as IGuildUser)?.GetDisplayAvatarUrl() ?? u.GetAvatarUrl(ImageFormat.Png));
        return string.Join("\n", avatarUrls);
    }

    private string Spongebobify(string text) // sponch
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

    public string CleanMessageForTrigger(string message)
    {
        string cleanedMessage = message.Replace(BuildKeyword(TriggerKeyword.BotName), "", StringComparison.InvariantCultureIgnoreCase);
        foreach (var botName in BotNames.Names)
        {
            cleanedMessage = cleanedMessage.Replace(botName, BuildKeyword(TriggerKeyword.BotName), StringComparison.InvariantCultureIgnoreCase);
        }
        // TODO: replace accented characters here
        return cleanedMessage;
    }

    public string BuildKeyword(TriggerKeyword keyword)
    {
        return BuildKeyword(keyword.ToString());
    }

    public string BuildKeyword(string keyword)
    {
        return $"{{{{{keyword.ToUpper(CultureInfo.InvariantCulture)}}}}}";
    }
}
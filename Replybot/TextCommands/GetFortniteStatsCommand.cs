using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Fortnite_API.Objects.V1;
using Replybot.BusinessLayer.Extensions;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetFortniteStatsCommand(
    FortniteApi fortniteApi,
    IDiscordFormatter discordFormatter,
    ILogger<DiscordBot> logger)
    : ITextCommand
{
    private const string UsernameKey = "searchTerm";
    private const string TriggerRegexPattern = $"(fortnite stats) +(?<{UsernameKey}>[a-zA-Z0-9_-]*)";
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
        var embed = await GetFortniteStatsEmbed(message);

        return new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };
    }

    private async Task<Embed?> GetFortniteStatsEmbed(SocketMessage message)
    {
        var match = Regex.Match(message.Content, TriggerRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (match.Success)
        {
            var username = match.Groups[UsernameKey].Value.CleanString();

            try
            {
                var fortniteStats = await fortniteApi.GetFortniteStats(username);
                if (fortniteStats == null)
                {
                    return discordFormatter.BuildErrorEmbedWithUserFooter("No Response from Fortnite API",
                        "I couldn't find anything for that username.",
                        message.Author);
                }

                var embedFieldBuilders = new List<EmbedFieldBuilder>();

                if (fortniteStats.Stats.All.Overall != null)
                {
                    embedFieldBuilders.Add(MakeStatsEmbedField("Overall",
                        fortniteStats.Stats.All.Overall.Matches,
                        fortniteStats.Stats.All.Overall.Wins,
                        fortniteStats.Stats.All.Overall.WinRate,
                        fortniteStats.Stats.All.Overall.Kills,
                        fortniteStats.Stats.All.Overall.KillsPerMatch,
                        fortniteStats.Stats.All.Overall.Deaths,
                        fortniteStats.Stats.All.Overall.Kd,
                        fortniteStats.Stats.All.Overall.MinutesPlayed,
                        fortniteStats.Stats.All.Overall.Score));
                }

                if (fortniteStats.Stats.All.Solo != null)
                {
                    embedFieldBuilders.Add(MakeStatsEmbedField("Solo",
                        fortniteStats.Stats.All.Solo.Matches,
                        fortniteStats.Stats.All.Solo.Wins,
                        fortniteStats.Stats.All.Solo.WinRate,
                        fortniteStats.Stats.All.Solo.Kills,
                        fortniteStats.Stats.All.Solo.KillsPerMatch,
                        fortniteStats.Stats.All.Solo.Deaths,
                        fortniteStats.Stats.All.Solo.Kd,
                        fortniteStats.Stats.All.Solo.MinutesPlayed,
                        fortniteStats.Stats.All.Solo.Score));
                }

                if (fortniteStats.Stats.All.Duo != null)
                {
                    embedFieldBuilders.Add(MakeStatsEmbedField("Duo",
                        fortniteStats.Stats.All.Duo.Matches,
                        fortniteStats.Stats.All.Duo.Wins,
                        fortniteStats.Stats.All.Duo.WinRate,
                        fortniteStats.Stats.All.Duo.Kills,
                        fortniteStats.Stats.All.Duo.KillsPerMatch,
                        fortniteStats.Stats.All.Duo.Deaths,
                        fortniteStats.Stats.All.Duo.Kd,
                        fortniteStats.Stats.All.Duo.MinutesPlayed,
                        fortniteStats.Stats.All.Duo.Score));
                }

                if (fortniteStats.Stats.All.Trio != null)
                {
                    embedFieldBuilders.Add(MakeStatsEmbedField("Trio",
                        fortniteStats.Stats.All.Trio.Matches,
                        fortniteStats.Stats.All.Trio.Wins,
                        fortniteStats.Stats.All.Trio.WinRate,
                        fortniteStats.Stats.All.Trio.Kills,
                        fortniteStats.Stats.All.Trio.KillsPerMatch,
                        fortniteStats.Stats.All.Trio.Deaths,
                        fortniteStats.Stats.All.Trio.Kd,
                        fortniteStats.Stats.All.Trio.MinutesPlayed,
                        fortniteStats.Stats.All.Trio.Score));
                }

                if (fortniteStats.Stats.All.Squad != null)
                {
                    embedFieldBuilders.Add(MakeStatsEmbedField("Squad",
                        fortniteStats.Stats.All.Squad.Matches,
                        fortniteStats.Stats.All.Squad.Wins,
                        fortniteStats.Stats.All.Squad.WinRate,
                        fortniteStats.Stats.All.Squad.Kills,
                        fortniteStats.Stats.All.Squad.KillsPerMatch,
                        fortniteStats.Stats.All.Squad.Deaths,
                        fortniteStats.Stats.All.Squad.Kd,
                        fortniteStats.Stats.All.Squad.MinutesPlayed,
                        fortniteStats.Stats.All.Squad.Score));
                }

                return discordFormatter.BuildRegularEmbedWithUserFooter("Fortnite",
                    $"Fortnite Stats for {username}",
                    message.Author,
                    embedFieldBuilders);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "Error in GetFortniteStatsCommand - {0}", ex.Message);
                return discordFormatter.BuildErrorEmbed("Fortnite Stats",
                    $"Sorry, there was an error.");
            }
        }

        logger.Log(LogLevel.Error,
            $"Error in GetFortniteStatsCommand: CanHandle passed, but regular expression was not a match. Input: {message.Content}");
        return discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Game",
            "Sorry, I couldn't make sense of that for some reason. This shouldn't happen, so try again or let the developer know there's an issue!",
            message.Author);
    }

    private static EmbedFieldBuilder MakeStatsEmbedField(string title, long matches, long wins, double winRate, long kills,
        double killsPerMatch, long deaths, double kdr, long minutesPlayed, long score)
    {
        var statsString = $"Matches: {matches}\n" +
                          $"Wins: {wins}\n" +
                          $"Win Rate: {winRate}\n" +
                          $"Kills: {kills}\n" +
                          $"Kills Per Match: {killsPerMatch}\n" +
                          $"Deaths: {deaths}\n" +
                          $"Kill-Death Ratio: {kdr}\n" +
                          $"Minutes Played: {minutesPlayed}\n" +
                          $"Score: {score}\n";

        var embedField = new EmbedFieldBuilder
        {
            Name = title,
            Value = statsString,
            IsInline = false
        };
        return embedField;
    }
}
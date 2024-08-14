using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetFortniteMapLocationCommand(FortniteApi fortniteApi, BotSettings botSettings)
    : ITextCommand
{
    private const string TriggerRegexPattern = "(where(( a|')re)? we droppin(g)?)|(whither shall we descend)";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

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
        var fortniteMapLocation = await GetFortniteMapLocation();

        return new CommandResponse
        {
            Description = fortniteMapLocation,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = false
        };
    }

    private async Task<string?> GetFortniteMapLocation()
    {
        var mapLocations = await fortniteApi.GetFortniteMapLocations();
        if (mapLocations == null)
        {
            return null;
        }

        var namedLocations = mapLocations.POIs.Where(l => !l.Id.ToLower().Contains("unnamed")).ToList();
        var randomIndex = new Random().Next(namedLocations.Count);
        var randomLocation = namedLocations[randomIndex];

        return randomLocation.Name;
    }
}
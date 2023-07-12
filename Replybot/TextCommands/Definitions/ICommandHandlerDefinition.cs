using Replybot.Models;

namespace Replybot.TextCommands.Definitions;

public interface ICommandHandlerDefinition
{
    /// <summary>
    ///     The keyword to match for triggering this command
    /// </summary>
    TriggerKeyword TriggerKeyword { get; set; }

    /// <summary>
    ///     If this command runs, stop checking for commands afterwards
    /// </summary>
    bool StopCheckingForCommands { get; set; }
}
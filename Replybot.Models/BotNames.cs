using System.Diagnostics.CodeAnalysis;

namespace Replybot.Models;

[ExcludeFromCodeCoverage]
public static class BotNames
{
    public static string[] Names { get; } = ["toby", "replybot", "tobias", "tobster"];
    public static string[] NamesToIgnore { get; } = ["toby fox", "replybot-dotnet"];
}